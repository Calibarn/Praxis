import os
from datetime import UTC, datetime, timedelta
from uuid import UUID

import pytest
from httpx import ASGITransport, AsyncClient

from news_service.app import create_app
from news_service.persistence.database import create_engine, create_session_factory
from news_service.persistence.model import News
from news_service.persistence.repository import NewsRepository

pytestmark = pytest.mark.integration


@pytest.mark.asyncio
async def test_full_stack_lists_a_seeded_news_item_through_the_real_database() -> None:
    database_url = os.environ.get("NEWS_DATABASE_URL")
    if not database_url:
        pytest.skip("NEWS_DATABASE_URL is not configured")

    engine = create_engine(database_url)
    session_factory = create_session_factory(engine)
    try:
        async with session_factory.begin() as session:
            await session.execute(News.__table__.delete())
            await NewsRepository(session).upsert_many(
                [
                    News(
                        id=UUID("00000000-0000-0000-0000-0000000000ff"),
                        title="Titel",
                        summary="Zusammenfassung",
                        content="Inhalt",
                        published_at=datetime.now(UTC) - timedelta(hours=1),
                        valid_from=datetime.now(UTC) - timedelta(hours=1),
                        valid_until=None,
                        is_active=True,
                    )
                ]
            )

        app = create_app(database_url=database_url)
        async with AsyncClient(
            transport=ASGITransport(app=app), base_url="http://testserver"
        ) as client:
            list_response = await client.get("/api/news")
            health_response = await client.get("/health")
    finally:
        async with session_factory.begin() as session:
            await session.execute(News.__table__.delete())
        await engine.dispose()

    assert list_response.status_code == 200
    body = list_response.json()
    assert body["total"] == 1
    assert body["items"][0]["id"] == "00000000-0000-0000-0000-0000000000ff"
    assert health_response.status_code == 200
