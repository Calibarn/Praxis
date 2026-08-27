# Praxis Application Rules

These rules extend the repository-root `AGENTS.md` for the Praxis application.

- Keep `frontend` and `backend` independently buildable and testable.
- The backend is the sole owner of MariaDB access; the frontend calls `/api/*` and never connects to MariaDB or internal service URLs directly.
- Treat all News content as untrusted plain text. Do not use HTML trust bypasses.
- Store timestamps and compare validity in UTC.
- Pin production and build dependencies. A critical or high dependency finding blocks completion.
- Keep tests beside their owning project and use RED -> GREEN -> REFACTOR for behavior changes.
