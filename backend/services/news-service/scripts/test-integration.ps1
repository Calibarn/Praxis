[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$serviceRoot = Split-Path -Parent $PSScriptRoot
$composeFile = Join-Path $serviceRoot 'compose.integration.yaml'
$python = Join-Path $serviceRoot '.venv\Scripts\python.exe'
$env:NEWS_DATABASE_URL = 'mysql+asyncmy://news_service:test-only-password@127.0.0.1:43306/praxis_news'
$env:NEWS_DEPLOYMENT_ENVIRONMENT = 'development'

if (-not (Test-Path -LiteralPath $python)) {
    throw 'Python-Umgebung fehlt. Zuerst requirements.lock installieren.'
}

Push-Location $serviceRoot
try {
    docker compose --file $composeFile down --volumes --remove-orphans
    docker compose --file $composeFile up --detach --wait

    & $python -m alembic upgrade head
    if ($LASTEXITCODE -ne 0) { throw 'Alembic upgrade ist fehlgeschlagen.' }
    & $python -m alembic check
    if ($LASTEXITCODE -ne 0) { throw 'Modell und Alembic-Migration sind nicht synchron.' }

    & $python -m pytest -q -m integration
    if ($LASTEXITCODE -ne 0) { throw 'MariaDB-Integrationstest ist fehlgeschlagen.' }

    docker compose --file $composeFile restart mariadb
    docker compose --file $composeFile up --detach --wait
    & $python -m pytest -q -m integration
    if ($LASTEXITCODE -ne 0) { throw 'Persistenztest nach Neustart ist fehlgeschlagen.' }

    & $python -m alembic downgrade base
    if ($LASTEXITCODE -ne 0) { throw 'Alembic downgrade ist fehlgeschlagen.' }
    & $python -m alembic upgrade head
    if ($LASTEXITCODE -ne 0) { throw 'Alembic Wiederholungs-Upgrade ist fehlgeschlagen.' }
}
finally {
    docker compose --file $composeFile down --volumes --remove-orphans
    Pop-Location
    Remove-Item Env:NEWS_DATABASE_URL -ErrorAction SilentlyContinue
    Remove-Item Env:NEWS_DEPLOYMENT_ENVIRONMENT -ErrorAction SilentlyContinue
}
