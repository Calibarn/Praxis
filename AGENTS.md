# Praxis Application Rules

These rules extend the repository-root `AGENTS.md` for the Praxis application.

- Keep `frontend`, `backend/services/news-service`, and `gateway` independently buildable and testable.
- The News Service is the sole source of truth for News and the only component allowed to access its MariaDB tables.
- The Gateway owns transport concerns only. It must not duplicate News filtering, persistence, or business rules.
- Frontends call `/api/*`; they never connect to MariaDB or internal service URLs.
- Shell and remote communicate only through documented federation and API contracts. Do not add direct feature-to-feature imports.
- Treat all News content as untrusted plain text. Do not use HTML trust bypasses.
- Store timestamps and compare validity in UTC.
- Pin production and build dependencies. A critical or high dependency finding blocks completion.
- Keep tests beside their owning project and use RED -> GREEN -> REFACTOR for behavior changes.

