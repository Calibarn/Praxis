from datetime import UTC, datetime
from uuid import UUID

import pytest
from httpx import ASGITransport, AsyncClient
from sqlalchemy.exc import SQLAlchemyError

from news_service.api.dependencies import get_repository
from news_service.app import create_app
from news_service.persistence.model import News


class _FakeRepository:
    def __init__(self, items: list[News], total: int) -> None:
        self._items = items
        self._total = total

    async def list_public_page(
        self, *, now: datetime, page: int, page_size: int
    ) -> tuple[list[News], int]:
        return self._items, self._total


class _FailingRepository:
    async def list_public_page(
        self, *, now: datetime, page: int, page_size: int
    ) -> tuple[list[News], int]:
        raise SQLAlchemyError("connection refused")


def _news(id_: str) -> News:
    return News(
        id=UUID(id_),
        title="Titel",
        summary="Zusammenfassung",
        content="Inhalt",
        published_at=datetime(2026, 8, 26, 8, tzinfo=UTC),
        valid_from=datetime(2026, 8, 26, 8, tzinfo=UTC),
        valid_until=None,
        is_active=True,
    )


def _client_with_repository(repository: object) -> AsyncClient:
    app = create_app(database_url="mysql+asyncmy://unused:unused@unused/unused")
    app.dependency_overrides[get_repository] = lambda: repository
    return AsyncClient(transport=ASGITransport(app=app), base_url="http://testserver")


@pytest.mark.asyncio
async def test_list_news_returns_camel_case_page_matching_the_contract() -> None:
    item = _news("00000000-0000-0000-0000-000000000001")
    async with _client_with_repository(_FakeRepository([item], total=1)) as client:
        response = await client.get("/api/news", params={"page": 1, "pageSize": 20})

    assert response.status_code == 200
    body = response.json()
    assert body == {
        "items": [
            {
                "id": "00000000-0000-0000-0000-000000000001",
                "title": "Titel",
                "summary": "Zusammenfassung",
                "content": "Inhalt",
                "publishedAt": "2026-08-26T08:00:00Z",
                "validFrom": "2026-08-26T08:00:00Z",
                "validUntil": None,
            }
        ],
        "page": 1,
        "pageSize": 20,
        "total": 1,
        "hasMore": False,
    }


@pytest.mark.asyncio
async def test_list_news_defaults_to_page_1_and_page_size_20() -> None:
    async with _client_with_repository(_FakeRepository([], total=0)) as client:
        response = await client.get("/api/news")

    assert response.status_code == 200
    body = response.json()
    assert body["page"] == 1
    assert body["pageSize"] == 20


@pytest.mark.asyncio
async def test_list_news_reports_has_more_when_further_pages_remain() -> None:
    item = _news("00000000-0000-0000-0000-000000000002")
    async with _client_with_repository(_FakeRepository([item], total=5)) as client:
        response = await client.get("/api/news", params={"page": 1, "pageSize": 1})

    assert response.json()["hasMore"] is True


@pytest.mark.asyncio
async def test_list_news_reports_no_more_pages_on_the_last_page() -> None:
    item = _news("00000000-0000-0000-0000-000000000003")
    async with _client_with_repository(_FakeRepository([item], total=1)) as client:
        response = await client.get("/api/news", params={"page": 1, "pageSize": 20})

    assert response.json()["hasMore"] is False


@pytest.mark.asyncio
@pytest.mark.parametrize(
    "params",
    [{"page": 0}, {"page": -1}, {"pageSize": 0}, {"pageSize": 101}],
)
async def test_list_news_rejects_out_of_range_parameters_with_a_stable_problem_shape(
    params: dict[str, int],
) -> None:
    async with _client_with_repository(_FakeRepository([], total=0)) as client:
        response = await client.get("/api/news", params=params)

    assert response.status_code == 422
    body = response.json()
    assert set(body.keys()) == {"code", "message"}
    assert body["code"] == "invalid_request"


@pytest.mark.asyncio
async def test_list_news_maps_database_failures_to_503_without_leaking_details() -> None:
    async with _client_with_repository(_FailingRepository()) as client:
        response = await client.get("/api/news")

    assert response.status_code == 503
    body = response.json()
    assert set(body.keys()) == {"code", "message"}
    assert "connection refused" not in body["message"]
    assert body["code"] == "news_service_unavailable"


@pytest.mark.asyncio
async def test_health_reports_ok_when_the_database_is_reachable() -> None:
    app = create_app(database_url="mysql+asyncmy://unused:unused@unused/unused")

    async def _healthy() -> None:
        return None

    from news_service.api.dependencies import check_database_connection

    app.dependency_overrides[check_database_connection] = _healthy
    async with AsyncClient(
        transport=ASGITransport(app=app), base_url="http://testserver"
    ) as client:
        response = await client.get("/health")

    assert response.status_code == 200
    assert response.json() == {"status": "ok"}


@pytest.mark.asyncio
async def test_health_reports_503_when_the_database_is_unreachable() -> None:
    app = create_app(database_url="mysql+asyncmy://unused:unused@unused/unused")

    async def _unhealthy() -> None:
        raise SQLAlchemyError("connection refused")

    from news_service.api.dependencies import check_database_connection

    app.dependency_overrides[check_database_connection] = _unhealthy
    async with AsyncClient(
        transport=ASGITransport(app=app), base_url="http://testserver"
    ) as client:
        response = await client.get("/health")

    assert response.status_code == 503
    body = response.json()
    assert "connection refused" not in body["message"]
