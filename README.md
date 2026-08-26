# Praxis application

Greenfield implementation of Story 1. The application is isolated from the JAF
runtime and consists of independently buildable projects:

```text
frontend/                         Angular workspace
  projects/shell/                 Native Federation host
  projects/news/                  Native Federation remote
backend/services/news-service/    News source of truth
gateway/                          Public FastAPI gateway
contracts/                        Public API contracts
```

## Version baseline

- Node.js 24 LTS-compatible line (`>=24.15 <25`)
- Angular 22.1.3, Angular CLI/build 22.1.5
- Native Federation 22.1.1 (v4 runtime)
- Python 3.14
- FastAPI 0.141.1
- SQLAlchemy 2.0.52 and Alembic 1.19.1

Direct dependencies are pinned in each owning `pyproject.toml`. Python services
also provide a hash-locked `requirements.lock`; reproduce an environment with
`python -m pip install --require-hashes -r requirements.lock`.

## Frontend lokal starten

Beide Angular-Entwicklungsserver lassen sich gemeinsam aus PowerShell starten:

```powershell
.\start-local.ps1
```

Die Shell ist anschließend unter `http://localhost:4200` und das News-
Microfrontend unter `http://localhost:4201` erreichbar. Beim ersten Start werden
fehlende Node-Abhängigkeiten mit `npm ci` installiert.

## Stack in Docker starten

Docker Desktop muss laufen. Einmalig `.env.example` nach `.env` kopieren und
die MariaDB-Passwörter setzen. Das Skript baut echte optimierte Production-
Images und startet Shell, News-Remote, News-Service und MariaDB:

```powershell
Copy-Item .env.example .env   # einmalig, danach Passwörter anpassen
.\start-docker.ps1
```

Bereits vorhandene Container mit den exakten Namen `praxis-shell`,
`praxis-news`, `praxis-news-service` und `praxis-mariadb` werden vorher
entfernt. Abweichende Container werden nicht verändert. Die Anwendung ist
danach unter folgenden Adressen erreichbar:

- Shell: `http://localhost:4200`
- News-Remote: `http://localhost:4201`
- News-Service: `http://localhost:8000/api/news`, `http://localhost:8000/health`

Der News-Service wendet beim Start automatisch seine Alembic-Migrationen an;
die MariaDB-Daten liegen im benannten Volume `praxis_mariadb_data` und
überleben `docker compose down` (nicht aber `docker compose down --volumes`).
Der Gateway ist noch nicht implementiert und deshalb kein Teil des Compose-
Stacks; Frontends sprechen aktuell keinen Backend-Dienst an.

Der lokale Start über `.\start-local.ps1` bleibt bewusst ein schneller
Development-Start mit Live-Reload; der Docker-Start prüft die Release-Builds.
