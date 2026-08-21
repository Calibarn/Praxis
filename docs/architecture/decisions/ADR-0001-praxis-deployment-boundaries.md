# ADR-0001: Praxis deployment boundaries

- Status: ACCEPTED
- Date: 2026-08-21
- Scope: Story 1 / Praxis application

## Context

Story 1 requires independently extensible Angular microfrontends and Python
microservices. The existing workspace contains JAF, not an application, and its
root `.gitignore` reserves `/frontend` and `/services` for externally synchronized
repositories.

## Decision

Create the product under `praxis/`. Treat the Angular shell, Angular News remote,
News Service, and API Gateway as independently buildable deployment units. The
News Service alone owns News data and MariaDB access. The Gateway owns only
transport concerns.

## Alternatives

- Root-level product folders: rejected because they collide with JAF ignore and ownership conventions.
- Single application monolith: rejected because it contradicts the approved Story architecture.

## Consequences

- Builds and reviews can be scoped by deployment unit.
- Compose integration is still required to provide one local start command.
- No component may read another service's tables or internal implementation.

