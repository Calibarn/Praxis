from datetime import UTC, datetime
from typing import Annotated

from fastapi import APIRouter, Depends, Query

from news_service.api.dependencies import check_database_connection, get_repository
from news_service.api.schemas import NewsItem, NewsPage, Problem
from news_service.persistence.repository import NewsRepository

router = APIRouter()

RepositoryDep = Annotated[NewsRepository, Depends(get_repository)]


@router.get(
    "/api/news",
    response_model=NewsPage,
    responses={
        422: {"model": Problem, "description": "Invalid pagination parameters."},
        503: {"model": Problem, "description": "The News Service is unavailable."},
    },
)
async def list_news(
    repository: RepositoryDep,
    page: Annotated[int, Query(ge=1)] = 1,
    page_size: Annotated[int, Query(ge=1, le=100, alias="pageSize")] = 20,
) -> NewsPage:
    now = datetime.now(UTC)
    items, total = await repository.list_public_page(now=now, page=page, page_size=page_size)
    return NewsPage(
        items=[NewsItem.model_validate(item, from_attributes=True) for item in items],
        page=page,
        pageSize=page_size,
        total=total,
        hasMore=page * page_size < total,
    )


@router.get(
    "/health",
    responses={503: {"model": Problem, "description": "The News Service is unavailable."}},
)
async def health(_: Annotated[None, Depends(check_database_connection)]) -> dict[str, str]:
    return {"status": "ok"}
