import argparse
import asyncio
import os

from news_service.persistence.database import create_engine, create_session_factory
from news_service.persistence.seed import seed_news


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Seed non-production News data")
    parser.add_argument("environment", choices=("development", "test"))
    return parser.parse_args()


async def run(environment: str, target_environment: str, database_url: str) -> int:
    if target_environment == "production":
        raise ValueError("Seeds are forbidden for a production target")
    if target_environment not in {"development", "test"}:
        raise ValueError("Seed target environment must be development or test")
    if environment not in {"development", "test"}:
        raise ValueError("Seed environment must be development or test")
    if environment != target_environment:
        raise ValueError("Seed environment must match target environment")
    engine = create_engine(database_url)
    try:
        session_factory = create_session_factory(engine)
        async with session_factory.begin() as session:
            return await seed_news(session, environment, target_environment)
    finally:
        await engine.dispose()


def main() -> None:
    args = parse_args()
    database_url = os.environ.get("NEWS_DATABASE_URL")
    if not database_url:
        raise SystemExit("NEWS_DATABASE_URL must be set")
    target_environment = os.environ.get("NEWS_DEPLOYMENT_ENVIRONMENT")
    if not target_environment:
        raise SystemExit("NEWS_DEPLOYMENT_ENVIRONMENT must be set")
    count = asyncio.run(run(args.environment, target_environment, database_url))
    print(f"Seeded {count} {args.environment} News records.")


if __name__ == "__main__":
    main()
