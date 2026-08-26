from datetime import datetime
from uuid import UUID

from pydantic import BaseModel, ConfigDict, Field


class NewsItem(BaseModel):
    """Public representation of one News record. Fields are always plain text."""

    model_config = ConfigDict(extra="forbid", populate_by_name=True)

    id: UUID
    title: str
    summary: str
    content: str
    published_at: datetime = Field(alias="publishedAt")
    valid_from: datetime = Field(alias="validFrom")
    valid_until: datetime | None = Field(alias="validUntil")


class NewsPage(BaseModel):
    """One stable, sorted page of the public News listing."""

    model_config = ConfigDict(extra="forbid", populate_by_name=True)

    items: list[NewsItem]
    page: int = Field(ge=1)
    page_size: int = Field(alias="pageSize", ge=1, le=100)
    total: int = Field(ge=0)
    has_more: bool = Field(alias="hasMore")


class Problem(BaseModel):
    """Stable error envelope that never leaks internal details."""

    model_config = ConfigDict(extra="forbid")

    code: str
    message: str
