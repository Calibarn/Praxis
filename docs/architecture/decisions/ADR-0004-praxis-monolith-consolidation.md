# ADR-0004: Consolidate to one backend and one frontend, on .NET

- Status: ACCEPTED
- Date: 2026-08-27
- Scope: Praxis application

## Context

ADR-0001 split the backend into a Gateway (transport only) and a News Service
(sole owner of News data), implemented in Python/FastAPI/SQLAlchemy. ADR-0002
split the frontend into an Angular Shell host and a News Native Federation
remote. At the app's actual size — one public endpoint, one content type, one
team — that split added indirection (an internal HTTP hop, two deployable
frontends, runtime module federation) without a corresponding benefit.

## Decision

- Backend: one ASP.NET Core (.NET 10) service replaces the Python Gateway and
  News Service. It talks to MariaDB via EF Core directly; there is no internal
  proxy hop. Structured as `Praxis.Backend.Application` (domain: entities, EF
  Core `DbContext`, repositories, controllers) and a thin
  `Praxis.Backend.Host` (composition root: `Program.cs`, `appsettings`),
  plus `Praxis.Backend.Tests` (unit) and `Praxis.Backend.Tests.Integration`
  (self-migrating, skip unless `NEWS_DATABASE_URL` is set — no setup script or
  bundled container required).
- Frontend: one Angular application replaces the Shell host and the News
  Native Federation remote. No runtime module federation; the News feature is
  a lazily-loaded route in the same build.
- The public wire contract (`GET /api/news`, `GET /health`, JSON shapes,
  status codes) is unchanged, except the Gateway-specific 502 ("upstream
  returned an invalid response") is removed from `contracts/news-api.openapi.yaml`
  — there is no upstream left to misbehave.

## Alternatives

- Keep the Python Gateway/News Service split and only migrate languages:
  rejected — the split was the thing adding cost, not the language.
  Superseded by the separate decision to migrate off Python (see the backend
  migration work in this same change).
- Keep Native Federation for future microfrontend growth: rejected for now —
  no second frontend team or independently-deployed feature exists yet: add
  federation back if and when that becomes true, rather than carrying its
  runtime/build complexity speculatively.

## Consequences

- One backend deployable, one frontend deployable, one MariaDB — `compose.yaml`
  now has `mariadb` + `backend` + `frontend`.
- `AGENTS.md`'s ownership rules (Gateway owns transport only; Shell/News import
  boundary) no longer apply as written and are updated to match a single
  backend/frontend; the still-relevant rules (UTC storage, plain-text News
  content, pinned dependencies, RED→GREEN→REFACTOR) carry over unchanged.
- ADR-0001 and ADR-0002 remain as the historical record of the original
  decision and why it was made; this ADR documents why it was reversed.
