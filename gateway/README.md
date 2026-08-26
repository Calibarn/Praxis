# Praxis API Gateway

Der Gateway ist der einzige öffentliche Einstiegspunkt für Backend-APIs. Er
besitzt keine eigene Businesslogik und dupliziert keine News-Regeln; er leitet
`GET /api/news` unverändert an den News Service weiter (siehe
`contracts/news-api.openapi.yaml`).

## API lokal starten

```powershell
$env:GATEWAY_NEWS_SERVICE_URL = 'http://localhost:8000'
.\.venv\Scripts\python.exe -m uvicorn praxis_gateway.app:create_app_from_env --factory --port 8080
```

`GET /api/news` reicht Query-Parameter, Statuscode und Body des News Service
durch (200, 422, 503). Ist der News Service nicht erreichbar, antwortet der
Gateway selbst mit `503`; ein unerwarteter Upstream-Statuscode wird als `502`
gemeldet. `GET /health` prüft nur die Erreichbarkeit des Gateways selbst.
