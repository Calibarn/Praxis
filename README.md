# Praxis application

Greenfield implementation of Story 1. The application is isolated from the JAF
runtime and consists of independently buildable projects:

```text
frontend/                                   Angular workspace (single app)
backend/
  Praxis.Backend.Application/               Domain: entities, EF Core DbContext, repositories, controllers
  Praxis.Backend.Host/                      Composition root: Program.cs, appsettings
  Praxis.Backend.Tests/                     Unit tests
  Praxis.Backend.Tests.Integration/         Integration tests (self-migrating; skip unless NEWS_DATABASE_URL is set)
```

## Version baseline

- Node.js 24 LTS-compatible line (`>=24.15 <25`)
- Angular 22.1.3, Angular CLI/build 22.1.5
- .NET 10, ASP.NET Core, EF Core 9.0.19 + Pomelo.EntityFrameworkCore.MySql 9.0.0

Frontend dependencies are pinned in `package.json`/`package-lock.json`. Backend
NuGet package versions are pinned exactly in each project's `.csproj`.

## Frontend lokal starten

```powershell
.\start-local.ps1
```

Die App ist anschließend unter `http://localhost:4200` erreichbar. Beim ersten
Start werden fehlende Node-Abhängigkeiten mit `npm ci` installiert.

## Stack in Docker starten

Docker Desktop muss laufen. Einmalig `.env.example` nach `.env` kopieren und
die MariaDB-Passwörter setzen. Das Skript baut echte optimierte Production-
Images und startet Frontend, Backend und MariaDB:

```powershell
Copy-Item .env.example .env   # einmalig, danach Passwörter anpassen
.\start-docker.ps1
```

Bereits vorhandene Container mit den exakten Namen `praxis-frontend`,
`praxis-backend` und `praxis-mariadb` werden vorher entfernt. Abweichende
Container werden nicht verändert. Die Anwendung ist danach unter folgenden
Adressen erreichbar:

- Frontend: `http://localhost:4200`
- Backend: `http://localhost:8000/api/news`, `http://localhost:8000/health`

Der Frontend-Container proxyt `/api/*` intern per Nginx an den Backend-Container
weiter (`GET http://localhost:4200/api/news` funktioniert also genauso wie der
direkte Zugriff auf den Backend-Port). Der Backend-Container wendet beim Start
automatisch seine EF-Core-Migrationen an; die MariaDB-Daten liegen im
benannten Volume `praxis_mariadb_data` und überleben `docker compose down`
(nicht aber `docker compose down --volumes`).

Der lokale Start über `.\start-local.ps1` bleibt bewusst ein schneller
Development-Start mit Live-Reload; der Docker-Start prüft die Release-Builds.
