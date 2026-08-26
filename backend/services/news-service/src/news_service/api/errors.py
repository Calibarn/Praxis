import logging

from fastapi import Request
from fastapi.exceptions import RequestValidationError
from fastapi.responses import JSONResponse
from sqlalchemy.exc import SQLAlchemyError

from news_service.api.schemas import Problem

logger = logging.getLogger(__name__)


async def handle_validation_error(_: Request, exc: RequestValidationError) -> JSONResponse:
    problem = Problem(code="invalid_request", message="One or more query parameters are invalid.")
    return JSONResponse(status_code=422, content=problem.model_dump())


async def handle_database_error(request: Request, exc: SQLAlchemyError) -> JSONResponse:
    logger.error("News Service database access failed for %s", request.url.path, exc_info=exc)
    problem = Problem(
        code="news_service_unavailable", message="The News Service is temporarily unavailable."
    )
    return JSONResponse(status_code=503, content=problem.model_dump())
