"""
Application configuration using Pydantic BaseSettings.

Loads settings from environment variables with sensible defaults
for local development. In production, values are injected via
Docker environment or .env files.
"""

from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    """Central configuration for the AI Engine microservice."""

    # ── Gemini AI ──────────────────────────────────────────────
    GEMINI_API_KEY: str
    GEMINI_MODEL: str = "gemini-2.0-flash"

    # ── Database ───────────────────────────────────────────────
    DATABASE_URL: str

    # ── Application ────────────────────────────────────────────
    APP_ENV: str = "development"
    LOG_LEVEL: str = "debug"

    model_config = {
        "env_file": ".env",
        "env_file_encoding": "utf-8",
        "case_sensitive": True,
    }


def get_settings() -> Settings:
    """Factory that returns a cached Settings instance."""
    return Settings()
