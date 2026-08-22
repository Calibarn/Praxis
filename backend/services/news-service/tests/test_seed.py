import pytest

from news_service.persistence import seed_cli
from news_service.persistence.seed import DEVELOPMENT_SEEDS, TEST_SEEDS, seed_news


def test_development_seeds_have_stable_unique_ids() -> None:
    ids = [seed.id for seed in DEVELOPMENT_SEEDS]

    assert ids
    assert len(ids) == len(set(ids))


def test_development_seeds_are_explicitly_non_production() -> None:
    assert all(
        seed.environment in {"development", "test"}
        for seed in (*DEVELOPMENT_SEEDS, *TEST_SEEDS)
    )


@pytest.mark.asyncio
async def test_seed_rejects_every_unsupported_environment() -> None:
    with pytest.raises(ValueError, match="development or test"):
        await seed_news(None, "production", "test")  # type: ignore[arg-type]


@pytest.mark.asyncio
async def test_seed_rejects_production_target_before_database_access() -> None:
    with pytest.raises(ValueError, match="production target"):
        await seed_news(None, "development", "production")  # type: ignore[arg-type]


@pytest.mark.asyncio
async def test_seed_environment_must_match_target_environment() -> None:
    with pytest.raises(ValueError, match="must match"):
        await seed_news(None, "development", "test")  # type: ignore[arg-type]


@pytest.mark.asyncio
@pytest.mark.parametrize(
    ("environment", "target_environment"),
    (("development", "production"), ("development", "test"), ("staging", "staging")),
)
async def test_seed_cli_rejects_unsafe_targets_before_engine_creation(
    monkeypatch: pytest.MonkeyPatch, environment: str, target_environment: str
) -> None:
    def fail_if_called(database_url: str) -> None:
        pytest.fail(f"database engine created for unsafe target: {database_url}")

    monkeypatch.setattr(seed_cli, "create_engine", fail_if_called)

    with pytest.raises(ValueError):
        await seed_cli.run(environment, target_environment, "mysql+asyncmy://unused")
