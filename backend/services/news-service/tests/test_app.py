import pytest

from news_service.app import create_app_from_env


def test_create_app_from_env_reads_the_database_url(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setenv("NEWS_DATABASE_URL", "mysql+asyncmy://unused:unused@unused/unused")

    app = create_app_from_env()

    assert app.state.session_factory is not None


def test_create_app_from_env_requires_the_database_url(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.delenv("NEWS_DATABASE_URL", raising=False)

    with pytest.raises(ValueError, match="database_url"):
        create_app_from_env()
