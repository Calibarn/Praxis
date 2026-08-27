# Praxis Threat Model

- Status: DRAFT
- Date: 2026-08-27
- Scope: Praxis application post-consolidation (one .NET backend, one Angular frontend, MariaDB, Docker Compose) — see ADR-0004.

This is an architecture-level threat model (trust boundaries, data flows,
STRIDE), not a line-level code review. A separate automated code-diff security
scan of this same change found no high-confidence exploitable findings (see
session record) — this document covers structural/systemic risk that a diff
scan cannot see.

## System overview

```
Browser (untrusted)
   │  HTTP(S), no auth
   ▼
┌─────────────────────────┐
│ frontend (nginx)          :4200→80
│  - serves Angular SPA     │
│  - proxy_pass /api/* ────┼──────┐
└─────────────────────────┘      │  plain HTTP, compose-internal network
                                  ▼
                        ┌─────────────────────────┐
                        │ backend (ASP.NET Core)    :8000
                        │  - GET /api/news (public) │
                        │  - GET /health             │
                        │  - runs EF Core migrations │
                        │    on every container start│
                        └─────────────┬─────────────┘
                                      │  MySqlConnector, compose-internal
                                      │  network only (no published port)
                                      ▼
                        ┌─────────────────────────┐
                        │ mariadb                    │
                        │  news_service: DML only     │
                        │  news_migrator: DDL, used   │
                        │  only by --migrate           │
                        └─────────────────────────┘
```

Trust boundaries: (1) Internet ↔ frontend container, (2) frontend ↔ backend
(inside the Compose network — trusted today, but see below), (3) backend ↔
MariaDB (inside the Compose network), (4) host/operator ↔ `.env` secrets.

Assets: News content (currently non-sensitive public announcements), the
`news_service` DB credential, the MariaDB data volume. No PII/PHI is handled
by this application today, despite it being a medical practice's site — see
Consequence 3 below.

## STRIDE findings

### 1. No schema/data privilege separation for the backend's DB user (Tampering / Elevation of Privilege) — MITIGATED (2026-08-27)

**Finding:** `compose.yaml` gave the `backend` container the same
`news_service` MariaDB credential for two different jobs: (a) normal
request-time reads (`SELECT`) and future writes, and (b) running EF Core
migrations (`DDL: CREATE TABLE`, `ALTER`, etc.) automatically on every
container start. There was one MariaDB user with both rights, used by one
long-running process.

**Why it mattered:** if the backend process were ever compromised (a future
dependency RCE, an unsafe deserialization bug introduced later, etc.), the
attacker would inherit schema-modification rights on `praxis_news`, not just
data access — they could drop/alter tables or plant a rogue migration-history
row, not merely read/exfiltrate rows.

**Mitigation applied:** two MariaDB users now exist —
`news_migrator` (DDL, used only by `--migrate` via `NEWS_MIGRATION_DATABASE_URL`)
and `news_service` (DML only, no DDL, used by the long-running app process via
`NEWS_DATABASE_URL`). `db-init/01-restrict-privileges.sh` sets this up
automatically for the Compose-managed MariaDB; `Praxis.Backend.Host/Program.cs`'s
`--migrate` path now requires `NEWS_MIGRATION_DATABASE_URL` and never uses the
runtime credential. Verified end-to-end: `news_service` gets
`ERROR 1142 CREATE command denied` on a DDL attempt but can still read/write
`news` rows; `news_migrator` runs migrations successfully. See
`backend/README.md`, "DB user separation", for operational details — an
existing MariaDB data volume from before this change keeps the old,
unrestricted grants until the volume is recreated or the SQL is applied by
hand.

### 2. No TLS termination in this repo (Tampering / Information Disclosure)

**Finding:** `frontend/nginx.conf` listens on plain HTTP (`listen 80`); no
TLS/certificate configuration exists anywhere in this repo, and
`compose.yaml` publishes `4200:80` and `8000:8000` as plain HTTP.

**Why it matters:** as shipped, traffic between a browser and this stack is
unencrypted, making it interceptable/tamperable on-path. Currently the only
data in flight is public News content, so today's actual exposure is low —
but this is a medical practice's site (see the project's own HIPAA/ADA
grounding note), and the day a patient-facing or PII-bearing feature is added
without first putting TLS in front of this stack, that data ships in the
clear.

**Recommendation:** treat "TLS termination in front of `frontend`" as a
hard prerequisite for any feature that touches personal data, not an
optional hardening step. Document where TLS is expected to terminate (a
cloud load balancer, a reverse proxy this repo doesn't own, etc.) — it isn't
in scope of this repo today, which is fine only as long as nothing sensitive
flows through it.

### 3. Fully unauthenticated, unauthorized-by-design API (Spoofing / Elevation of Privilege — informational)

**Finding:** `GET /api/news` and `GET /health` have no authentication or
authorization check; this is intentional (public read-only content).
`INewsRepository`/`BaseRepository` already expose `UpsertAsync`/`DeleteAsync`
capability at the domain layer, but nothing in `NewsController` currently
routes to them — the write surface is CLI-only (`dotnet run -- seed
<environment>`), not network-reachable.

**Why it matters:** not a vulnerability today. It's the single biggest
assumption the rest of this threat model rests on, and the one most likely to
silently become false: the moment any controller exposes `POST`/`PUT`/`DELETE`
on `/api/news` (or any other resource), this system goes from "no identity
needed" to "needs an auth/authz story," and every finding above compounds
(an unauthenticated write endpoint over plain HTTP with DDL-capable DB
credentials behind it is a materially worse position).

**Recommendation:** if/when a write path is added, revisit this document —
don't bolt auth onto individual endpoints ad hoc.

### 4. Denial of service surface (excluded from the code scan's scope, included here)

**Finding:** `/api/news` has no rate limiting; pagination bounds the
per-request cost (`pageSize` capped at 100) but nothing bounds request
*volume*. `/health` is equally open.

**Why it matters:** low severity given the content is cheap to serve and
there's no auth to brute-force, but worth naming explicitly since a full
threat model should cover it even though the automated code-diff scan
deliberately excludes DoS.

**Recommendation:** acceptable to defer; revisit if this ever sits directly
on the public internet without a CDN/WAF in front of it.

### 5. Container privilege posture (informational, mostly already good)

- `backend`: runs as non-root `appuser` (Dockerfile), base images pinned by
  digest — good.
- `mariadb`: official image, root password only used for bootstrap; the app
  itself uses the lower-privileged, DML-only `news_service` user (Finding 1,
  mitigated).
- `frontend`: nginx official image, no explicit non-root `USER` — runs with
  the base image's defaults (unchanged from before this migration, not a
  regression introduced by it, but worth tightening alongside Finding 1 if
  this stack is hardened for real deployment).

## Summary of recommended follow-ups, by priority

1. ~~Separate the migration DB user from the runtime app DB user~~ — done,
   see Finding 1.
2. Decide and document where TLS terminates before any PII/PHI-adjacent
   feature ships (Finding 2).
3. Treat "add an authenticated write endpoint" as a trigger to redo this
   document, not a drive-by change (Finding 3).
4. Low priority / defer: rate limiting (Finding 4), frontend container user
   hardening (Finding 5).
