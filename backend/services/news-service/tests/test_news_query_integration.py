import os
from datetime import UTC, datetime, timedelta
from uuid import UUID

import pytest
import pytest_asyncio

from news_service.persistence.database import create_engine, create_session_factory
from news_service.persistence.model import News
from news_service.persistence.repository import NewsRepository

pytestmark = pytest.mark.integration


def _news(
    id_: str,
    *,
    published_at: datetime,
    valid_from: datetime | None = None,
    valid_until: datetime | None = None,
    is_active: bool = True,
) -> News:
    return News(
        id=UUID(id_),
        title=f"Titel {id_}",
        summary="Zusammenfassung",
        content="Inhalt",
        published_at=published_at,
        valid_from=valid_from if valid_from is not None else published_at,
        valid_until=valid_until,
        is_active=is_active,
    )


@pytest_asyncio.fixture
async def repository() -> NewsRepository:
    database_url = os.environ.get("NEWS_DATABASE_URL")
    if not database_url:
        pytest.skip("NEWS_DATABASE_URL is not configured")

    engine = create_engine(database_url)
    session_factory = create_session_factory(engine)
    try:
        async with session_factory.begin() as session:
            await session.execute(News.__table__.delete())
        async with session_factory() as session:
            yield NewsRepository(session)
    finally:
        async with session_factory.begin() as session:
            await session.execute(News.__table__.delete())
        await engine.dispose()


NOW = datetime(2026, 8, 26, 12, 0, tzinfo=UTC)


@pytest.mark.asyncio
async def test_list_public_page_excludes_future_published_at(
    repository: NewsRepository,
) -> None:
    future = _news(
        "00000000-0000-0000-0000-000000000001", published_at=NOW + timedelta(hours=1)
    )
    past = _news("00000000-0000-0000-0000-000000000002", published_at=NOW - timedelta(hours=1))
    await repository.upsert_many([future, past])
    await repository._session.flush()

    items, total = await repository.list_public_page(now=NOW, page=1, page_size=20)

    assert [item.id for item in items] == [past.id]
    assert total == 1


@pytest.mark.asyncio
async def test_list_public_page_excludes_not_yet_valid_entries(
    repository: NewsRepository,
) -> None:
    not_yet_valid = _news(
        "00000000-0000-0000-0000-000000000003",
        published_at=NOW - timedelta(hours=1),
        valid_from=NOW + timedelta(hours=1),
    )
    await repository.upsert_many([not_yet_valid])
    await repository._session.flush()

    items, total = await repository.list_public_page(now=NOW, page=1, page_size=20)

    assert items == []
    assert total == 0


@pytest.mark.asyncio
async def test_list_public_page_treats_valid_until_as_exclusive_upper_bound(
    repository: NewsRepository,
) -> None:
    expiring_now = _news(
        "00000000-0000-0000-0000-000000000004",
        published_at=NOW - timedelta(hours=2),
        valid_until=NOW,
    )
    still_valid = _news(
        "00000000-0000-0000-0000-000000000005",
        published_at=NOW - timedelta(hours=2),
        valid_until=NOW + timedelta(seconds=1),
    )
    await repository.upsert_many([expiring_now, still_valid])
    await repository._session.flush()

    items, total = await repository.list_public_page(now=NOW, page=1, page_size=20)

    assert [item.id for item in items] == [still_valid.id]
    assert total == 1


@pytest.mark.asyncio
async def test_list_public_page_treats_null_valid_until_as_always_valid(
    repository: NewsRepository,
) -> None:
    open_ended = _news(
        "00000000-0000-0000-0000-000000000006",
        published_at=NOW - timedelta(days=365),
        valid_until=None,
    )
    await repository.upsert_many([open_ended])
    await repository._session.flush()

    items, total = await repository.list_public_page(now=NOW, page=1, page_size=20)

    assert [item.id for item in items] == [open_ended.id]
    assert total == 1


@pytest.mark.asyncio
async def test_list_public_page_excludes_inactive_entries(repository: NewsRepository) -> None:
    inactive = _news(
        "00000000-0000-0000-0000-000000000007",
        published_at=NOW - timedelta(hours=1),
        is_active=False,
    )
    await repository.upsert_many([inactive])
    await repository._session.flush()

    items, total = await repository.list_public_page(now=NOW, page=1, page_size=20)

    assert items == []
    assert total == 0


@pytest.mark.asyncio
async def test_list_public_page_sorts_by_published_at_descending(
    repository: NewsRepository,
) -> None:
    older = _news("00000000-0000-0000-0000-000000000008", published_at=NOW - timedelta(days=2))
    newer = _news("00000000-0000-0000-0000-000000000009", published_at=NOW - timedelta(days=1))
    await repository.upsert_many([older, newer])
    await repository._session.flush()

    items, _ = await repository.list_public_page(now=NOW, page=1, page_size=20)

    assert [item.id for item in items] == [newer.id, older.id]


@pytest.mark.asyncio
async def test_list_public_page_breaks_ties_on_published_at_by_id_descending(
    repository: NewsRepository,
) -> None:
    same_time = NOW - timedelta(hours=1)
    lower_id = _news("00000000-0000-0000-0000-00000000000a", published_at=same_time)
    higher_id = _news("00000000-0000-0000-0000-00000000000b", published_at=same_time)
    await repository.upsert_many([lower_id, higher_id])
    await repository._session.flush()

    first_page, _ = await repository.list_public_page(now=NOW, page=1, page_size=1)
    second_page, _ = await repository.list_public_page(now=NOW, page=2, page_size=1)

    assert [item.id for item in first_page] == [higher_id.id]
    assert [item.id for item in second_page] == [lower_id.id]


@pytest.mark.asyncio
async def test_list_public_page_returns_empty_items_on_out_of_range_page(
    repository: NewsRepository,
) -> None:
    only_entry = _news(
        "00000000-0000-0000-0000-00000000000c", published_at=NOW - timedelta(hours=1)
    )
    await repository.upsert_many([only_entry])
    await repository._session.flush()

    items, total = await repository.list_public_page(now=NOW, page=2, page_size=20)

    assert items == []
    assert total == 1


@pytest.mark.asyncio
async def test_list_public_page_reports_total_across_pages(repository: NewsRepository) -> None:
    entries = [
        _news(
            f"00000000-0000-0000-0000-0000000000{index:02x}",
            published_at=NOW - timedelta(hours=index),
        )
        for index in range(1, 4)
    ]
    await repository.upsert_many(entries)
    await repository._session.flush()

    first_page, total = await repository.list_public_page(now=NOW, page=1, page_size=2)
    second_page, _ = await repository.list_public_page(now=NOW, page=2, page_size=2)

    assert total == 3
    assert len(first_page) == 2
    assert len(second_page) == 1
