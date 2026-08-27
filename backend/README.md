# Praxis Backend

Single ASP.NET Core (.NET 10) backend and sole owner of the `news` table.
Migrations need an explicit `NEWS_DATABASE_URL`; there is no built-in
production value and no production seeds.

## Migration

```powershell
$env:NEWS_DATABASE_URL = 'mysql+asyncmy://USER:PASSWORD@HOST:3306/praxis_news'
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
setup script or bundled container required. Point it at a disposable database
(e.g. the `mariadb` service from the repo-root `compose.yaml`) to run them for
real.
