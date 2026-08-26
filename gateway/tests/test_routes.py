from collections.abc import Callable

import httpx
import pytest
from httpx import ASGITransport, AsyncClient, MockTransport, Request, Response

from praxis_gateway.app import create_app


def _client_with_upstream(handler: Callable[[Request], Response]) -> AsyncClient:
    app = create_app(news_service_url="http://news-service.internal")
    app.state.news_service_client = AsyncClient(
        transport=MockTransport(handler), base_url="http://news-service.internal"
    )
    return AsyncClient(transport=ASGITransport(app=app), base_url="http://testserver")


@pytest.mark.asyncio
async def test_list_news_passes_through_a_successful_upstream_response() -> None:
    page = {"items": [], "page": 1, "pageSize": 20, "total": 0, "hasMore": False}

    def handler(request: Request) -> Response:
        assert request.url.path == "/api/news"
        assert dict(request.url.params) == {"page": "1", "pageSize": "20"}
        return Response(200, json=page)

    async with _client_with_upstream(handler) as client:
        response = await client.get("/api/news", params={"page": 1, "pageSize": 20})

    assert response.status_code == 200
    assert response.json() == page


@pytest.mark.asyncio
async def test_list_news_passes_through_upstream_validation_errors() -> None:
    problem = {"code": "invalid_request", "message": "One or more query parameters are invalid."}

    def handler(_: Request) -> Response:
        return Response(422, json=problem)

    async with _client_with_upstream(handler) as client:
        response = await client.get("/api/news", params={"page": 0})

    assert response.status_code == 422
    assert response.json() == problem


@pytest.mark.asyncio
async def test_list_news_passes_through_upstream_unavailability() -> None:
    problem = {"code": "news_service_unavailable", "message": "The News Service is unavailable."}

    def handler(_: Request) -> Response:
        return Response(503, json=problem)

    async with _client_with_upstream(handler) as client:
        response = await client.get("/api/news")

    assert response.status_code == 503
    assert response.json() == problem


@pytest.mark.asyncio
async def test_list_news_maps_unreachable_upstream_to_503_without_leaking_details() -> None:
    def handler(request: Request) -> Response:
        raise httpx.ConnectError("connection refused", request=request)

    async with _client_with_upstream(handler) as client:
        response = await client.get("/api/news")

    assert response.status_code == 503
    body = response.json()
    assert set(body.keys()) == {"code", "message"}
    assert "connection refused" not in body["message"]
    assert body["code"] == "news_service_unavailable"


@pytest.mark.asyncio
async def test_list_news_maps_unexpected_upstream_status_to_502() -> None:
    def handler(_: Request) -> Response:
        return Response(500, json={"detail": "boom"})

    async with _client_with_upstream(handler) as client:
        response = await client.get("/api/news")

    assert response.status_code == 502
    body = response.json()
    assert set(body.keys()) == {"code", "message"}
    assert "boom" not in body["message"]
    assert body["code"] == "news_service_invalid_response"


@pytest.mark.asyncio
async def test_health_reports_ok() -> None:
    async with _client_with_upstream(lambda _: Response(200)) as client:
        response = await client.get("/health")

    assert response.status_code == 200
    assert response.json() == {"status": "ok"}
