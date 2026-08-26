# Praxis News Service

Der News Service ist alleiniger Eigentümer der Tabelle `news`. Migrationen
benötigen eine explizite `NEWS_DATABASE_URL`; es gibt keinen eingebauten
Produktionswert und keine produktiven Seeds.

## Migration

```powershell
$env:NEWS_DATABASE_URL = 'mysql+asyncmy://USER:PASSWORD@HOST:3306/praxis_news'
.\.venv\Scripts\python.exe -m alembic upgrade head
```

Ein Downgrade der additiven Initialmigration löscht die News-Tabelle und damit
deren Daten. Es darf nur gegen eine entbehrliche Testdatenbank ausgeführt werden.

## Nichtproduktive Seeds

```powershell
$env:NEWS_DATABASE_URL = 'mysql+asyncmy://USER:PASSWORD@HOST:3306/praxis_news'
$env:NEWS_DEPLOYMENT_ENVIRONMENT = 'development'
.\.venv\Scripts\python.exe -m news_service.persistence.seed_cli development
```

Erlaubt sind ausschließlich `development` und `test`. Stabile UUIDs machen den
Vorgang idempotent; ein erneuter Lauf aktualisiert dieselben Datensätze. Die
angeforderte Seed-Umgebung muss mit `NEWS_DEPLOYMENT_ENVIRONMENT`
übereinstimmen. `production`, unbekannte und fehlende Zielumgebungen werden
vor dem Datenbankzugriff abgelehnt.

## API lokal starten

```powershell
$env:NEWS_DATABASE_URL = 'mysql+asyncmy://USER:PASSWORD@HOST:3306/praxis_news'
.\.venv\Scripts\python.exe -m uvicorn news_service.app:create_app_from_env --factory
```

`GET /api/news?page=1&pageSize=20` liefert nur aktive, veröffentlichte und
aktuell gültige News, absteigend sortiert nach `publishedAt` und `id`.
`pageSize` liegt zwischen 1 und 100 (Standard 20); ungültige Parameter ergeben
`422`. `GET /health` prüft die Datenbankverbindung und liefert `503`, wenn sie
nicht erreichbar ist. Fehlermeldungen enthalten keine internen Details.

## MariaDB-Integrationstest

Docker Desktop muss laufen. Das Skript nutzt ausschließlich die isolierte
Testdatenbank auf Port `43306` und entfernt deren Testvolume anschließend:

```powershell
.\scripts\test-integration.ps1
```
