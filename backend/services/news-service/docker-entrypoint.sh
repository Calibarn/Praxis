#!/bin/sh
set -eu

alembic upgrade head
exec uvicorn news_service.app:create_app_from_env --factory --host 0.0.0.0 --port 8000
