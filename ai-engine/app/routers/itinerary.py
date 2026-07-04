"""
Itinerary generation router.

Exposes the POST /generate endpoint that accepts a TripRequest,
calls the GeminiService, and returns a validated ItineraryResponse.
"""

import logging
import time

from fastapi import APIRouter, HTTPException
from pydantic import ValidationError

from app.models.itinerary_schema import ItineraryResponse
from app.models.trip_request import TripRequest
from app.services.gemini_service import GeminiService, GeminiServiceError

logger = logging.getLogger(__name__)

router = APIRouter(tags=["itinerary"])

# Lazily initialized service instance
_gemini_service: GeminiService | None = None


def _get_gemini_service() -> GeminiService:
    """Get or create the singleton GeminiService instance."""
    global _gemini_service
    if _gemini_service is None:
        _gemini_service = GeminiService()
    return _gemini_service


@router.post(
    "/generate",
    response_model=ItineraryResponse,
    summary="Generate a travel itinerary",
    description=(
        "Accepts trip parameters and uses Google Gemini to generate "
        "a structured day-by-day travel itinerary with activity options."
    ),
    responses={
        200: {"description": "Successfully generated itinerary"},
        422: {"description": "Invalid request parameters"},
        502: {"description": "Gemini API failure"},
        500: {"description": "Unexpected server error"},
    },
)
async def generate_itinerary(request: TripRequest) -> ItineraryResponse:
    """
    Generate a complete travel itinerary.

    Takes trip parameters (destination, dates, budget, preferences),
    sends them to Google Gemini, and returns a structured itinerary
    with two activity options per time slot.
    """
    start_time = time.monotonic()

    logger.info(
        "Itinerary generation request: destination='%s', days=%d, budget=€%.2f, people=%d",
        request.destination,
        request.total_days,
        request.budget_max,
        request.num_people,
    )

    try:
        service = _get_gemini_service()
        itinerary = await service.generate_itinerary(request)

        elapsed = time.monotonic() - start_time
        logger.info(
            "Itinerary generated successfully in %.2fs for '%s' (%d days)",
            elapsed,
            request.destination,
            itinerary.total_days,
        )

        return itinerary

    except ValidationError as exc:
        elapsed = time.monotonic() - start_time
        logger.error(
            "Validation error after %.2fs: %s",
            elapsed,
            str(exc),
        )
        raise HTTPException(
            status_code=422,
            detail={
                "error": "validation_error",
                "message": "The generated itinerary failed schema validation.",
                "details": exc.errors(),
            },
        )

    except GeminiServiceError as exc:
        elapsed = time.monotonic() - start_time
        logger.error(
            "Gemini service error after %.2fs: %s",
            elapsed,
            str(exc),
        )
        raise HTTPException(
            status_code=502,
            detail={
                "error": "gemini_error",
                "message": "Failed to generate itinerary from AI service.",
                "details": str(exc),
            },
        )

    except Exception as exc:
        elapsed = time.monotonic() - start_time
        logger.error(
            "Unexpected error after %.2fs: %s",
            elapsed,
            str(exc),
            exc_info=True,
        )
        raise HTTPException(
            status_code=500,
            detail={
                "error": "internal_error",
                "message": "An unexpected error occurred.",
            },
        )
