from collections.abc import Iterable
from datetime import datetime
from uuid import UUID

from sqlalchemy import func, or_, select
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

    async def list_public_page(
        self, *, now: datetime, page: int, page_size: int
    ) -> tuple[list[News], int]:
        """Return one stable page of active, currently valid News plus the total count."""
        conditions = (
            News.is_active.is_(True),
            News.published_at <= now,
            News.valid_from <= now,
            or_(News.valid_until.is_(None), News.valid_until > now),
        )
        total = await self._session.scalar(
            select(func.count()).select_from(News).where(*conditions)
        )
        rows = await self._session.scalars(
            select(News)
            .where(*conditions)
            .order_by(News.published_at.desc(), News.id.desc())
            .offset((page - 1) * page_size)
            .limit(page_size)
        )
        return list(rows.all()), total or 0
