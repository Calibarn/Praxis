# Praxis Backend

Single ASP.NET Core (.NET 10) backend and sole owner of the `news` table.
There is no built-in production value and no production seeds.

## DB user separation

The long-running app process and schema migrations use **different** MariaDB
credentials (see `docs/architecture/threat-model.md`, "DB privilege
separation"):

- `NEWS_DATABASE_URL` — the runtime user (`news_service` in `compose.yaml`).
  DML only (`SELECT`/`INSERT`/`UPDATE`/`DELETE`), scoped to the `news` table.
  Used by the app itself and by the seed CLI.
- `NEWS_MIGRATION_DATABASE_URL` — the migration user (`news_migrator` in
  `compose.yaml`). DDL rights on the whole schema. Used only by
  `dotnet ... --migrate` / `dotnet ef database update`, never by the
  long-running app.

`db-init/01-restrict-privileges.sh` sets this up automatically for the
Compose-managed MariaDB (runs once, on first container init — an existing
data volume from before this change keeps the old, unrestricted grants until
the volume is recreated or the SQL is applied by hand).

## Migration

```powershell
$env:NEWS_MIGRATION_DATABASE_URL = 'mysql+asyncmy://news_migrator:PASSWORD@HOST:3306/praxis_news'
dotnet ef database update --project Praxis.Backend.Application --startup-project Praxis.Backend.Host
```

Downgrading the additive initial migration drops the `news` table and its
data. Only run this against a disposable test database.

## Non-production seeds

```powershell
$env:NEWS_DATABASE_URL = 'mysql+asyncmy://USER:PASSWORD@HOST:3306/praxis_news'
$env:NEWS_DEPLOYMENT_ENVIRONMENT = 'development'
dotnet run --project Praxis.Backend.Host -- seed development
```

Only `development` and `test` are allowed. Stable UUIDs make this idempotent;
re-running it updates the same records. The requested seed environment must
match `NEWS_DEPLOYMENT_ENVIRONMENT`. `production`, unknown, and missing target
environments are rejected before any database access.

## Run the API locally

```powershell
$env:NEWS_DATABASE_URL = 'mysql+asyncmy://USER:PASSWORD@HOST:3306/praxis_news'
dotnet run --project Praxis.Backend.Host
```

`GET /api/news?page=1&pageSize=20` returns only active, published, and
currently valid News, sorted descending by `publishedAt` and `id`. `pageSize`
is between 1 and 100 (default 20); invalid parameters return `422`.
`GET /health` checks the database connection and returns `503` if it's
unreachable. Error messages never contain internal details.

## Tests

```powershell
dotnet test Praxis.Backend.slnx
```

Unit tests (`Praxis.Backend.Tests`) always run. Integration tests
(`Praxis.Backend.Tests.Integration`) migrate and seed themselves against
whatever `NEWS_DATABASE_URL` points to, and self-skip if it isn't set — no
setup script or bundled container required. Because they call
`Database.MigrateAsync()` themselves, point `NEWS_DATABASE_URL` at a
DDL-capable credential when running them against the Compose-managed
MariaDB — the `news_migrator` connection string, not the DML-only
`news_service` one (see "DB user separation" above) — since it's a disposable
test database anyway, the split doesn't need to be mirrored there.
