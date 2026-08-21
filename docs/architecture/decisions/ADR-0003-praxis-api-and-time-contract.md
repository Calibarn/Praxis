# ADR-0003: Praxis News API, pagination, and time semantics

- Status: ACCEPTED
- Date: 2026-08-21
- Scope: Praxis News API

## Context

The UI must load more News while scrolling. News visibility depends on active,
publication, and validity timestamps. The public entry point is a Python Gateway.

## Decision

Expose `GET /api/news` through the Gateway with page-based pagination (default
20, maximum 100), `total`, and `hasMore`. Sort by `PublishedAt DESC, Id DESC`.
Store and compare times in UTC. Use a half-open interval where `ValidUntil` is
exclusive and nullable. Treat content as untrusted plain text.

## Alternatives

- Cursor pagination: deferred because page pagination is sufficient for Story 1.
- Inclusive validity end: rejected because half-open intervals are unambiguous.
- Markdown or HTML: rejected to avoid an unrequested sanitization pipeline.

## Consequences

- The client deduplicates IDs if concurrent publication shifts page boundaries.
- The Gateway must proxy the contract without duplicating News rules.
- Contract, boundary, and XSS-as-text tests are mandatory.

