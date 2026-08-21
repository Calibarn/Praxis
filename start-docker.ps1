[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw 'Docker ist nicht installiert oder nicht im PATH.'
}

docker info *> $null
if ($LASTEXITCODE -ne 0) {
    throw 'Docker ist nicht erreichbar. Bitte Docker Desktop starten.'
}

foreach ($containerName in @('praxis-shell', 'praxis-news')) {
    $existingContainerId = docker container ls --all --quiet --filter "name=^/$containerName`$"
    if ($LASTEXITCODE -ne 0) { throw 'Vorhandene Docker-Container konnten nicht ermittelt werden.' }
    if ($existingContainerId) {
        Write-Host "Entferne vorhandenen Container '$containerName'..."
        docker container rm --force $containerName
        if ($LASTEXITCODE -ne 0) { throw "Container '$containerName' konnte nicht entfernt werden." }
    }
}

Push-Location $PSScriptRoot
try {
    Write-Host 'Baue die Production-Images...'
    docker compose build
    if ($LASTEXITCODE -ne 0) { throw 'Docker-Compose-Build ist fehlgeschlagen.' }

    Write-Host 'Starte die Release-Container...'
    docker compose up --detach
    if ($LASTEXITCODE -ne 0) { throw 'Docker-Container konnten nicht gestartet werden.' }
}
finally {
    Pop-Location
}

Write-Host 'Shell: http://localhost:4200'
Write-Host 'News:  http://localhost:4201'
Write-Host 'Status: docker compose ps'
Write-Host 'Logs:   docker compose logs --follow'
