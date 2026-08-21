# ADR-0002: Angular Native Federation for runtime microfrontends

- Status: ACCEPTED
- Date: 2026-08-21
- Scope: Praxis frontend

## Context

The approved Story decision requires runtime federation. Angular 21's build
toolchain contained unresolved high-severity audit findings at implementation
time. Angular 22 and Native Federation v4 provide a compatible, audit-clean line.

## Decision

Use Angular 22 with the Shell as a dynamic host and News as a runtime-loaded
Native Federation remote. Share Angular dependencies as strict singletons. Load
the News remote from a runtime manifest and keep feature state/API mapping inside
the News project.

## Alternatives

- Angular 21 Native Federation v3: rejected due to known high-severity build dependency findings.
- Build-time feature libraries: rejected by the approved Runtime Federation decision.
- Webpack Module Federation: rejected in favor of Angular's esbuild-compatible federation integration.

## Consequences

- Shell and News are independently buildable and deployable.
- Runtime manifest correctness and remote-failure handling require explicit tests.
- Dependency versions must remain compatible and locked across host and remote.

