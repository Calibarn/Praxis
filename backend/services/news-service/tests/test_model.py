from datetime import UTC, datetime
from typing import cast
from uuid import UUID

import pytest
from sqlalchemy import Table, inspect

from news_service.persistence.model import News
from news_service.persistence.types import UTCDateTime


def test_news_model_owns_the_required_persistence_contract() -> None:
    mapper = inspect(News)

    assert set(mapper.columns.keys()) == {
        "id",
        "title",
        "summary",
        "content",
        "published_at",
        "valid_from",
        "valid_until",
        "is_active",
        "created_at",
        "updated_at",
    }
    assert mapper.columns.id.primary_key
    assert mapper.columns.valid_until.nullable
    assert not mapper.columns.is_active.nullable
    table = cast(Table, mapper.local_table)
    assert {index.name for index in table.indexes} == {
        "ix_news_active_published_id",
    }


def test_utc_datetime_rejects_naive_values() -> None:
    column_type = UTCDateTime()

    with pytest.raises(ValueError, match="timezone-aware"):
        column_type.process_bind_param(datetime(2026, 8, 21, 12), None)


def test_utc_datetime_normalizes_values_and_results_to_utc() -> None:
    column_type = UTCDateTime()
    value = datetime(2026, 8, 21, 12, tzinfo=UTC)

    assert column_type.process_bind_param(value, None) == value.replace(tzinfo=None)
    assert column_type.process_result_value(value.replace(tzinfo=None), None) == value


def test_news_uses_uuid_identifiers() -> None:
    news = News(
        id=UUID("34b0c18b-95f2-4ea9-ac1e-e7bd4dbce252"),
        title="Titel",
        summary="Zusammenfassung",
        content="Inhalt",
        published_at=datetime(2026, 8, 21, 12, tzinfo=UTC),
        valid_from=datetime(2026, 8, 21, 12, tzinfo=UTC),
        valid_until=None,
        is_active=True,
    )

    assert isinstance(news.id, UUID)
