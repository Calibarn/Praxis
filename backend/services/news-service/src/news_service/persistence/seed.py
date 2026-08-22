from dataclasses import dataclass
from datetime import UTC, datetime
from typing import Literal
from uuid import UUID

from sqlalchemy.ext.asyncio import AsyncSession

from news_service.persistence.model import News
from news_service.persistence.repository import NewsRepository

SeedEnvironment = Literal["development", "test"]


@dataclass(frozen=True)
class NewsSeed:
    id: UUID
    environment: SeedEnvironment
    title: str
    summary: str
    content: str
    published_at: datetime
    valid_from: datetime
    valid_until: datetime | None = None
    is_active: bool = True

    def to_model(self) -> News:
        return News(
            id=self.id,
            title=self.title,
            summary=self.summary,
            content=self.content,
            published_at=self.published_at,
            valid_from=self.valid_from,
            valid_until=self.valid_until,
            is_active=self.is_active,
        )


DEVELOPMENT_SEEDS = (
    NewsSeed(
        id=UUID("18ee8f7d-a5b7-4db2-a0da-40558ae96779"),
        environment="development",
        title="Willkommen in der Praxis",
        summary="Die neue Praxis-Website ist im Aufbau.",
        content="Hier finden Sie künftig aktuelle Informationen aus unserer Praxis.",
        published_at=datetime(2026, 8, 21, 8, tzinfo=UTC),
        valid_from=datetime(2026, 8, 21, 8, tzinfo=UTC),
    ),
    NewsSeed(
        id=UUID("be0d4355-48b3-4010-98f4-a9563512bb22"),
        environment="development",
        title="Hinweis zu Sprechzeiten",
        summary="Bitte vereinbaren Sie vor Ihrem Besuch einen Termin.",
        content="Terminvereinbarungen helfen uns, Wartezeiten möglichst kurz zu halten.",
        published_at=datetime(2026, 8, 20, 8, tzinfo=UTC),
        valid_from=datetime(2026, 8, 20, 8, tzinfo=UTC),
    ),
)

TEST_SEEDS = (
    NewsSeed(
        id=UUID("41bc655a-0a65-4cc7-b4be-e8150137e671"),
        environment="test",
        title="Testnachricht",
        summary="Testzusammenfassung",
        content="<script>alert('als Text behandeln')</script>",
        published_at=datetime(2026, 8, 21, 10, tzinfo=UTC),
        valid_from=datetime(2026, 8, 21, 10, tzinfo=UTC),
    ),
)


async def seed_news(
    session: AsyncSession, environment: str, target_environment: str
) -> int:
    if target_environment == "production":
        raise ValueError("Seeds are forbidden for a production target")
    if target_environment not in {"development", "test"}:
        raise ValueError("Seed target environment must be development or test")
    if environment not in {"development", "test"}:
        raise ValueError("Seed environment must be development or test")
    if environment != target_environment:
        raise ValueError("Seed environment must match target environment")

    seeds: tuple[NewsSeed, ...]
    if environment == "development":
        seeds = DEVELOPMENT_SEEDS
    elif environment == "test":
        seeds = TEST_SEEDS
    else:
        raise ValueError("Seed environment must be development or test")
    repository = NewsRepository(session)
    await repository.upsert_many(seed.to_model() for seed in seeds)
    await session.flush()
    return await repository.count_by_ids(seed.id for seed in seeds)
