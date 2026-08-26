from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """Runtime configuration for the API Gateway."""

    model_config = SettingsConfigDict(env_prefix="GATEWAY_")

    news_service_url: str
