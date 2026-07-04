"""
Pydantic model for incoming trip generation requests.

Validated by FastAPI automatically on POST /generate.
"""

from datetime import date, datetime
from typing import Optional

from pydantic import BaseModel, Field, computed_field, field_validator


class TripRequest(BaseModel):
    """Schema for the trip parameters sent by the .NET backend."""

    destination: str = Field(
        ...,
        min_length=2,
        description="Travel destination city or region.",
        examples=["Tokyo", "Costiera Amalfitana"],
    )
    start_date: str = Field(
        ...,
        description="Trip start date in ISO 8601 format (YYYY-MM-DD).",
        examples=["2025-06-15"],
    )
    end_date: str = Field(
        ...,
        description="Trip end date in ISO 8601 format (YYYY-MM-DD).",
        examples=["2025-06-22"],
    )
    budget_max: float = Field(
        ...,
        gt=0,
        description="Maximum budget in EUR.",
        examples=[2500.00],
    )
    num_people: int = Field(
        default=1,
        ge=1,
        description="Number of travellers.",
        examples=[2],
    )
    preferences: Optional[list[str]] = Field(
        default=None,
        description="Optional list of travel preferences / interests.",
        examples=[["cultura", "gastronomia", "avventura"]],
    )

    @field_validator("start_date", "end_date")
    @classmethod
    def validate_date_format(cls, value: str) -> str:
        """Ensure dates are valid ISO 8601 strings."""
        try:
            datetime.strptime(value, "%Y-%m-%d")
        except ValueError as exc:
            raise ValueError(
                f"Date must be in YYYY-MM-DD format, got '{value}'"
            ) from exc
        return value

    @computed_field  # type: ignore[misc]
    @property
    def total_days(self) -> int:
        """Calculate the total number of trip days (inclusive)."""
        start = date.fromisoformat(self.start_date)
        end = date.fromisoformat(self.end_date)
        delta = (end - start).days + 1
        if delta < 1:
            return 1
        return delta
