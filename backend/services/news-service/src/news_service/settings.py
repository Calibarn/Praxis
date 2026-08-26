from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """Runtime configuration for the News Service API."""

    model_config = SettingsConfigDict(env_prefix="NEWS_")

    database_url: str
