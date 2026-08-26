from collections.abc import AsyncIterator
from typing import Annotated

from fastapi import Depends, Request
from sqlalchemy import text
from sqlalchemy.ext.asyncio import AsyncSession

from news_service.persistence.database import session_scope
from news_service.persistence.repository import NewsRepository


async def get_session(request: Request) -> AsyncIterator[AsyncSession]:
    async for session in session_scope(request.app.state.session_factory):
        yield session


SessionDep = Annotated[AsyncSession, Depends(get_session)]


async def get_repository(session: SessionDep) -> NewsRepository:
    return NewsRepository(session)


async def check_database_connection(session: SessionDep) -> None:
    await session.execute(text("SELECT 1"))
