from collections.abc import Iterable
from uuid import UUID

from sqlalchemy import select
from sqlalchemy.ext.asyncio import AsyncSession

from news_service.persistence.model import News


class NewsRepository:
    def __init__(self, session: AsyncSession) -> None:
        self._session = session

    async def upsert_many(self, records: Iterable[News]) -> None:
        for record in records:
            existing = await self._session.scalar(select(News).where(News.id == record.id))
            if existing is None:
                self._session.add(record)
                continue
            existing.title = record.title
            existing.summary = record.summary
            existing.content = record.content
            existing.published_at = record.published_at
            existing.valid_from = record.valid_from
            existing.valid_until = record.valid_until
            existing.is_active = record.is_active

    async def count_by_ids(self, ids: Iterable[UUID]) -> int:
        result = await self._session.scalars(select(News.id).where(News.id.in_(tuple(ids))))
        return len(result.all())
