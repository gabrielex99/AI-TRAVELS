"""
Unit tests for Pydantic models: TripRequest and ItineraryResponse.

Tests cover validation rules, computed properties, and schema parsing
against a realistic SRS-matching example payload.
"""

import pytest
from pydantic import ValidationError

from app.models.itinerary_schema import (
    ActivityOption,
    DayPlan,
    DaySlots,
    FlightWidgetParams,
    ItineraryResponse,
    TimeSlot,
)
from app.models.trip_request import TripRequest


# ═══════════════════════════════════════════════════════════════════
# TripRequest Tests
# ═══════════════════════════════════════════════════════════════════


class TestTripRequest:
    """Tests for TripRequest model validation."""

    def test_valid_request_minimal(self) -> None:
        """Minimal valid request with only required fields."""
        req = TripRequest(
            destination="Tokyo",
            start_date="2025-06-15",
            end_date="2025-06-22",
            budget_max=2500.00,
        )
        assert req.destination == "Tokyo"
        assert req.num_people == 1
        assert req.preferences is None

    def test_valid_request_full(self) -> None:
        """Fully specified valid request."""
        req = TripRequest(
            destination="Costiera Amalfitana",
            start_date="2025-07-01",
            end_date="2025-07-07",
            budget_max=3000.00,
            num_people=2,
            preferences=["cultura", "gastronomia", "mare"],
        )
        assert req.destination == "Costiera Amalfitana"
        assert req.num_people == 2
        assert len(req.preferences) == 3

    def test_total_days_computation(self) -> None:
        """Total days should be inclusive (end - start + 1)."""
        req = TripRequest(
            destination="Roma",
            start_date="2025-03-10",
            end_date="2025-03-15",
            budget_max=1500.00,
        )
        assert req.total_days == 6

    def test_total_days_single_day(self) -> None:
        """Single-day trip should return 1."""
        req = TripRequest(
            destination="Milano",
            start_date="2025-05-01",
            end_date="2025-05-01",
            budget_max=500.00,
        )
        assert req.total_days == 1

    def test_total_days_inverted_dates(self) -> None:
        """When end_date < start_date, total_days should be clamped to 1."""
        req = TripRequest(
            destination="Firenze",
            start_date="2025-05-10",
            end_date="2025-05-08",
            budget_max=800.00,
        )
        assert req.total_days == 1

    def test_destination_too_short(self) -> None:
        """Destination must be at least 2 characters."""
        with pytest.raises(ValidationError) as exc_info:
            TripRequest(
                destination="X",
                start_date="2025-06-15",
                end_date="2025-06-22",
                budget_max=2500.00,
            )
        errors = exc_info.value.errors()
        assert any(e["loc"] == ("destination",) for e in errors)

    def test_budget_must_be_positive(self) -> None:
        """Budget must be greater than 0."""
        with pytest.raises(ValidationError) as exc_info:
            TripRequest(
                destination="Tokyo",
                start_date="2025-06-15",
                end_date="2025-06-22",
                budget_max=0,
            )
        errors = exc_info.value.errors()
        assert any(e["loc"] == ("budget_max",) for e in errors)

    def test_budget_negative(self) -> None:
        """Negative budget should fail validation."""
        with pytest.raises(ValidationError):
            TripRequest(
                destination="Tokyo",
                start_date="2025-06-15",
                end_date="2025-06-22",
                budget_max=-100,
            )

    def test_num_people_zero(self) -> None:
        """Number of people must be at least 1."""
        with pytest.raises(ValidationError) as exc_info:
            TripRequest(
                destination="Tokyo",
                start_date="2025-06-15",
                end_date="2025-06-22",
                budget_max=2500.00,
                num_people=0,
            )
        errors = exc_info.value.errors()
        assert any(e["loc"] == ("num_people",) for e in errors)

    def test_invalid_date_format(self) -> None:
        """Non-ISO date format should fail validation."""
        with pytest.raises(ValidationError):
            TripRequest(
                destination="Tokyo",
                start_date="15/06/2025",
                end_date="2025-06-22",
                budget_max=2500.00,
            )

    def test_invalid_date_value(self) -> None:
        """Invalid date value should fail validation."""
        with pytest.raises(ValidationError):
            TripRequest(
                destination="Tokyo",
                start_date="2025-13-45",
                end_date="2025-06-22",
                budget_max=2500.00,
            )


# ═══════════════════════════════════════════════════════════════════
# ItineraryResponse Tests
# ═══════════════════════════════════════════════════════════════════


# Realistic SRS-matching example payload
SRS_EXAMPLE_JSON = {
    "destination": "Tokyo",
    "total_days": 2,
    "flight_widget_params": {
        "destination_iata": "NRT",
        "suggested_months": ["2025-06", "2025-07"],
    },
    "itinerary": [
        {
            "day": 1,
            "theme": "Tradizione e Modernità",
            "slots": {
                "morning": {
                    "option_a": {
                        "title": "Visita al Tempio Senso-ji",
                        "description": "Esplora il tempio buddista più antico di Tokyo nel quartiere di Asakusa.",
                        "latitude": 35.7148,
                        "longitude": 139.7967,
                        "gyg_search_term": "Senso-ji temple tour Tokyo",
                    },
                    "option_b": {
                        "title": "Passeggiata nel Giardino Imperiale",
                        "description": "Ammira i giardini del Palazzo Imperiale con vista sullo skyline.",
                        "latitude": 35.6852,
                        "longitude": 139.7528,
                        "gyg_search_term": "Imperial Palace gardens Tokyo tour",
                    },
                },
                "afternoon": {
                    "option_a": {
                        "title": "Esplorazione di Akihabara",
                        "description": "Immergiti nella cultura geek e tecnologica di Akihabara.",
                        "latitude": 35.7023,
                        "longitude": 139.7745,
                        "gyg_search_term": "Akihabara electronics district tour Tokyo",
                    },
                    "option_b": {
                        "title": "Shopping a Shibuya",
                        "description": "Attraversa il famoso incrocio e scopri i negozi di Shibuya.",
                        "latitude": 35.6595,
                        "longitude": 139.7004,
                        "gyg_search_term": "Shibuya crossing walking tour Tokyo",
                    },
                },
                "evening": {
                    "option_a": {
                        "title": "Cena a Shinjuku",
                        "description": "Gusta ramen autentico nelle stradine di Golden Gai.",
                        "latitude": 35.6938,
                        "longitude": 139.7035,
                        "gyg_search_term": "Shinjuku food tour Tokyo",
                    },
                    "option_b": {
                        "title": "Tokyo Tower di notte",
                        "description": "Goditi la vista panoramica notturna dalla Tokyo Tower.",
                        "latitude": 35.6586,
                        "longitude": 139.7454,
                        "gyg_search_term": "Tokyo Tower night visit",
                    },
                },
            },
        },
        {
            "day": 2,
            "theme": "Natura e Relax",
            "slots": {
                "morning": {
                    "option_a": {
                        "title": "Santuario Meiji",
                        "description": "Visita il sereno santuario shintoista immerso nella foresta.",
                        "latitude": 35.6764,
                        "longitude": 139.6993,
                        "gyg_search_term": "Meiji Shrine tour Tokyo",
                    },
                    "option_b": {
                        "title": "Parco di Ueno",
                        "description": "Passeggia tra i musei e i ciliegi del Parco di Ueno.",
                        "latitude": 35.7146,
                        "longitude": 139.7732,
                        "gyg_search_term": "Ueno Park walking tour Tokyo",
                    },
                },
                "afternoon": {
                    "option_a": {
                        "title": "Quartiere di Harajuku",
                        "description": "Scopri la moda eccentrica e i caffè tematici di Harajuku.",
                        "latitude": 35.6702,
                        "longitude": 139.7027,
                        "gyg_search_term": "Harajuku fashion district tour Tokyo",
                    },
                    "option_b": {
                        "title": "Museo Nazionale di Tokyo",
                        "description": "Ammira la più grande collezione di arte giapponese.",
                        "latitude": 35.7189,
                        "longitude": 139.7766,
                        "gyg_search_term": "Tokyo National Museum guided tour",
                    },
                },
                "evening": {
                    "option_a": {
                        "title": "Crociera sulla baia di Tokyo",
                        "description": "Naviga sulla baia ammirando il Rainbow Bridge illuminato.",
                        "latitude": 35.6364,
                        "longitude": 139.7634,
                        "gyg_search_term": "Tokyo Bay evening cruise",
                    },
                    "option_b": {
                        "title": "Izakaya a Roppongi",
                        "description": "Prova le tapas giapponesi in un autentico izakaya.",
                        "latitude": 35.6627,
                        "longitude": 139.7307,
                        "gyg_search_term": "Roppongi izakaya food tour Tokyo",
                    },
                },
            },
        },
    ],
}


class TestItineraryResponse:
    """Tests for ItineraryResponse schema parsing and validation."""

    def test_parse_srs_example(self) -> None:
        """Full SRS example JSON should parse without errors."""
        response = ItineraryResponse.model_validate(SRS_EXAMPLE_JSON)
        assert response.destination == "Tokyo"
        assert response.total_days == 2
        assert len(response.itinerary) == 2

    def test_flight_widget_params(self) -> None:
        """Flight widget params should be correctly parsed."""
        response = ItineraryResponse.model_validate(SRS_EXAMPLE_JSON)
        assert response.flight_widget_params.destination_iata == "NRT"
        assert len(response.flight_widget_params.suggested_months) == 2
        assert "2025-06" in response.flight_widget_params.suggested_months

    def test_day_plan_structure(self) -> None:
        """Each day plan should have a theme and three time slots."""
        response = ItineraryResponse.model_validate(SRS_EXAMPLE_JSON)
        day1 = response.itinerary[0]
        assert day1.day == 1
        assert day1.theme == "Tradizione e Modernità"
        assert day1.slots.morning is not None
        assert day1.slots.afternoon is not None
        assert day1.slots.evening is not None

    def test_activity_options(self) -> None:
        """Each time slot should have option_a and option_b with all fields."""
        response = ItineraryResponse.model_validate(SRS_EXAMPLE_JSON)
        morning = response.itinerary[0].slots.morning

        assert morning.option_a.title == "Visita al Tempio Senso-ji"
        assert morning.option_a.latitude == pytest.approx(35.7148, abs=0.001)
        assert morning.option_a.longitude == pytest.approx(139.7967, abs=0.001)
        assert morning.option_a.gyg_search_term == "Senso-ji temple tour Tokyo"

        assert morning.option_b.title == "Passeggiata nel Giardino Imperiale"
        assert morning.option_b.latitude == pytest.approx(35.6852, abs=0.001)

    def test_missing_required_field(self) -> None:
        """Missing required fields should raise ValidationError."""
        incomplete = {"destination": "Tokyo"}
        with pytest.raises(ValidationError):
            ItineraryResponse.model_validate(incomplete)

    def test_invalid_iata_code_too_long(self) -> None:
        """IATA code longer than 3 characters should fail."""
        data = SRS_EXAMPLE_JSON.copy()
        data["flight_widget_params"] = {
            "destination_iata": "NRTT",
            "suggested_months": ["2025-06"],
        }
        with pytest.raises(ValidationError):
            ItineraryResponse.model_validate(data)

    def test_empty_itinerary_fails(self) -> None:
        """Empty itinerary list should fail validation."""
        data = {
            "destination": "Tokyo",
            "total_days": 1,
            "flight_widget_params": {
                "destination_iata": "NRT",
                "suggested_months": ["2025-06"],
            },
            "itinerary": [],
        }
        with pytest.raises(ValidationError):
            ItineraryResponse.model_validate(data)

    def test_day_number_must_be_positive(self) -> None:
        """Day number must be >= 1."""
        with pytest.raises(ValidationError):
            DayPlan(
                day=0,
                theme="Test",
                slots=DaySlots(
                    morning=TimeSlot(
                        option_a=ActivityOption(
                            title="A", description="A", latitude=0.0,
                            longitude=0.0, gyg_search_term="a",
                        ),
                        option_b=ActivityOption(
                            title="B", description="B", latitude=0.0,
                            longitude=0.0, gyg_search_term="b",
                        ),
                    ),
                    afternoon=TimeSlot(
                        option_a=ActivityOption(
                            title="A", description="A", latitude=0.0,
                            longitude=0.0, gyg_search_term="a",
                        ),
                        option_b=ActivityOption(
                            title="B", description="B", latitude=0.0,
                            longitude=0.0, gyg_search_term="b",
                        ),
                    ),
                    evening=TimeSlot(
                        option_a=ActivityOption(
                            title="A", description="A", latitude=0.0,
                            longitude=0.0, gyg_search_term="a",
                        ),
                        option_b=ActivityOption(
                            title="B", description="B", latitude=0.0,
                            longitude=0.0, gyg_search_term="b",
                        ),
                    ),
                ),
            )

    def test_serialization_roundtrip(self) -> None:
        """Model should survive a serialize-deserialize roundtrip."""
        response = ItineraryResponse.model_validate(SRS_EXAMPLE_JSON)
        json_str = response.model_dump_json()
        reparsed = ItineraryResponse.model_validate_json(json_str)
        assert reparsed.destination == response.destination
        assert reparsed.total_days == response.total_days
        assert len(reparsed.itinerary) == len(response.itinerary)
