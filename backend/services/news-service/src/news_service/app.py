from collections.abc import AsyncIterator
from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.exceptions import RequestValidationError
from sqlalchemy.exc import SQLAlchemyError

from news_service.api.errors import handle_database_error, handle_validation_error
from news_service.api.routes import router
from news_service.persistence.database import create_engine, create_session_factory
from news_service.settings import Settings


def create_app(*, database_url: str) -> FastAPI:
    engine = create_engine(database_url)
    session_factory = create_session_factory(engine)

    @asynccontextmanager
    async def lifespan(_: FastAPI) -> AsyncIterator[None]:
        try:
            yield
        finally:
            await engine.dispose()

    app = FastAPI(title="Praxis News Service", lifespan=lifespan)
    app.state.session_factory = session_factory
    app.include_router(router)
    app.add_exception_handler(RequestValidationError, handle_validation_error)  # type: ignore[arg-type]
    app.add_exception_handler(SQLAlchemyError, handle_database_error)  # type: ignore[arg-type]
    return app


def create_app_from_env() -> FastAPI:
    """Zero-argument factory for `uvicorn news_service.app:create_app_from_env --factory`."""
    return create_app(database_url=Settings().database_url)  # type: ignore[call-arg]
