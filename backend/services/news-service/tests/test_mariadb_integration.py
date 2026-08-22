import os

import pytest
from sqlalchemy import func, inspect, select, text

from news_service.persistence.database import create_engine, create_session_factory
from news_service.persistence.model import News
from news_service.persistence.seed import DEVELOPMENT_SEEDS, seed_news

pytestmark = pytest.mark.integration


@pytest.mark.asyncio
async def test_migration_schema_and_seed_idempotency() -> None:
    database_url = os.environ.get("NEWS_DATABASE_URL")
    if not database_url:
        pytest.skip("NEWS_DATABASE_URL is not configured")

    engine = create_engine(database_url)
    try:
        async with engine.connect() as connection:
            tables = await connection.run_sync(lambda sync: inspect(sync).get_table_names())
            indexes = await connection.run_sync(lambda sync: inspect(sync).get_indexes("news"))
            revision = await connection.scalar(text("SELECT version_num FROM alembic_version"))
            session_time_zone = await connection.scalar(text("SELECT @@session.time_zone"))

        assert "news" in tables
        assert revision == "20260821_01"
        assert session_time_zone == "+00:00"
        assert {index["name"] for index in indexes} == {"ix_news_active_published_id"}

        session_factory = create_session_factory(engine)
        async with session_factory.begin() as session:
            first_count = await seed_news(session, "development", "development")
        async with session_factory.begin() as session:
            second_count = await seed_news(session, "development", "development")
            persisted_count = await session.scalar(select(func.count()).select_from(News))

        assert first_count == len(DEVELOPMENT_SEEDS)
        assert second_count == len(DEVELOPMENT_SEEDS)
        assert persisted_count == len(DEVELOPMENT_SEEDS)
    finally:
        await engine.dispose()
