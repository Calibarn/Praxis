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

## MariaDB-Integrationstest

Docker Desktop muss laufen. Das Skript nutzt ausschließlich die isolierte
Testdatenbank auf Port `43306` und entfernt deren Testvolume anschließend:

```powershell
.\scripts\test-integration.ps1
```
