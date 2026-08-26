from collections.abc import AsyncIterator
from contextlib import asynccontextmanager

import httpx
from fastapi import FastAPI

from praxis_gateway.api.routes import router
from praxis_gateway.settings import Settings


def create_app(*, news_service_url: str) -> FastAPI:
    client = httpx.AsyncClient(base_url=news_service_url, timeout=5.0)

    @asynccontextmanager
    async def lifespan(_: FastAPI) -> AsyncIterator[None]:
        try:
            yield
        finally:
            await client.aclose()

    app = FastAPI(title="Praxis API Gateway", lifespan=lifespan)
    app.state.news_service_client = client
    app.include_router(router)
    return app


def create_app_from_env() -> FastAPI:
    """Zero-argument factory for `uvicorn praxis_gateway.app:create_app_from_env --factory`."""
    return create_app(news_service_url=Settings().news_service_url)  # type: ignore[call-arg]
