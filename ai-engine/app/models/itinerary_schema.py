"""
Pydantic models for the structured itinerary response.

These models mirror the JSON schema defined in the SRS and are used
to validate Gemini's output before returning it to the .NET backend.
"""

from pydantic import BaseModel, Field


# ── Activity & TimeSlot ────────────────────────────────────────────


class ActivityOption(BaseModel):
    """A single activity option with geolocation and affiliate search term."""

    title: str = Field(
        ...,
        description="Name of the activity or point of interest.",
        examples=["Visita al Tempio Senso-ji"],
    )
    description: str = Field(
        ...,
        description="Brief description of the activity.",
        examples=["Esplora il tempio buddista più antico di Tokyo."],
    )
    latitude: float = Field(
        ...,
        description="Latitude of the activity location.",
        examples=[35.7148],
    )
    longitude: float = Field(
        ...,
        description="Longitude of the activity location.",
        examples=[139.7967],
    )
    gyg_search_term: str = Field(
        ...,
        description="Search term for GetYourGuide affiliate integration.",
        examples=["Senso-ji temple tour Tokyo"],
    )


class TimeSlot(BaseModel):
    """A time slot offering two alternative activities."""

    option_a: ActivityOption = Field(
        ...,
        description="Primary activity option.",
    )
    option_b: ActivityOption = Field(
        ...,
        description="Alternative activity option.",
    )


# ── Day Structure ──────────────────────────────────────────────────


class DaySlots(BaseModel):
    """The three time slots for a single day."""

    morning: TimeSlot = Field(..., description="Morning activities.")
    afternoon: TimeSlot = Field(..., description="Afternoon activities.")
    evening: TimeSlot = Field(..., description="Evening activities.")


class DayPlan(BaseModel):
    """Plan for a single day of the trip."""

    day: int = Field(
        ...,
        ge=1,
        description="Day number (1-indexed).",
        examples=[1],
    )
    theme: str = Field(
        ...,
        description="Thematic title for the day.",
        examples=["Tradizione e Modernità"],
    )
    slots: DaySlots = Field(
        ...,
        description="Morning, afternoon, and evening time slots.",
    )


# ── Flight Widget ──────────────────────────────────────────────────


class FlightWidgetParams(BaseModel):
    """Parameters for the Kiwi flight search widget."""

    destination_iata: str = Field(
        ...,
        min_length=3,
        max_length=3,
        description="IATA airport code for the destination.",
        examples=["NRT"],
    )
    suggested_months: list[str] = Field(
        ...,
        description="List of suggested travel months in YYYY-MM format.",
        examples=[["2025-06", "2025-07"]],
    )


# ── Top-Level Response ─────────────────────────────────────────────


class ItineraryResponse(BaseModel):
    """Complete itinerary response returned to the .NET backend."""

    destination: str = Field(
        ...,
        description="The trip destination.",
        examples=["Tokyo"],
    )
    total_days: int = Field(
        ...,
        ge=1,
        description="Total number of days in the itinerary.",
        examples=[7],
    )
    flight_widget_params: FlightWidgetParams = Field(
        ...,
        description="Parameters for the flight search widget.",
    )
    itinerary: list[DayPlan] = Field(
        ...,
        min_length=1,
        description="Day-by-day itinerary.",
    )
