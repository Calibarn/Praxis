import pytest

from praxis_gateway.app import create_app_from_env


def test_create_app_from_env_reads_the_news_service_url(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.setenv("GATEWAY_NEWS_SERVICE_URL", "http://news-service:8000")

    app = create_app_from_env()

    assert app.state.news_service_client is not None


def test_create_app_from_env_requires_the_news_service_url(monkeypatch: pytest.MonkeyPatch) -> None:
    monkeypatch.delenv("GATEWAY_NEWS_SERVICE_URL", raising=False)

    with pytest.raises(ValueError, match="news_service_url"):
        create_app_from_env()
