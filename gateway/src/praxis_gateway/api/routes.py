import httpx
from fastapi import APIRouter, Request
from fastapi.responses import JSONResponse, Response

from praxis_gateway.api.schemas import Problem

router = APIRouter()

_PASSTHROUGH_STATUS_CODES = frozenset({200, 422, 503})


@router.get(
    "/api/news",
    responses={
        422: {"model": Problem, "description": "Invalid pagination parameters."},
        502: {"model": Problem, "description": "The News Service returned an invalid response."},
        503: {"model": Problem, "description": "The News Service is unavailable."},
    },
)
async def list_news(request: Request) -> Response:
    client: httpx.AsyncClient = request.app.state.news_service_client
    try:
        upstream = await client.get("/api/news", params=request.query_params)
    except httpx.HTTPError:
        problem = Problem(
            code="news_service_unavailable",
            message="The News Service is temporarily unavailable.",
        )
        return JSONResponse(status_code=503, content=problem.model_dump())

    if upstream.status_code in _PASSTHROUGH_STATUS_CODES:
        return Response(
            content=upstream.content,
            status_code=upstream.status_code,
            media_type=upstream.headers.get("content-type", "application/json"),
        )

    problem = Problem(
        code="news_service_invalid_response",
        message="The News Service returned an invalid response.",
    )
    return JSONResponse(status_code=502, content=problem.model_dump())


@router.get("/health")
async def health() -> dict[str, str]:
    return {"status": "ok"}
