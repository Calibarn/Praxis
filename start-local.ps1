[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$frontendPath = Join-Path $PSScriptRoot 'frontend'

if (-not (Get-Command node -ErrorAction SilentlyContinue)) {
    throw 'Node.js 24 ist nicht installiert oder nicht im PATH.'
}
if (-not (Get-Command npm -ErrorAction SilentlyContinue)) {
    throw 'npm ist nicht installiert oder nicht im PATH.'
}

Push-Location $frontendPath
try {
    if (-not (Test-Path -LiteralPath 'node_modules')) {
        Write-Host 'Installiere Frontend-Abhängigkeiten...'
        npm ci
        if ($LASTEXITCODE -ne 0) { throw 'npm ci ist fehlgeschlagen.' }
    }

    Write-Host 'Frontend: http://localhost:4200'
    Write-Host 'Beenden mit Strg+C.'
    npm start
    if ($LASTEXITCODE -ne 0) { throw 'Der Frontend-Prozess wurde mit einem Fehler beendet.' }
}
finally {
    Pop-Location
}
