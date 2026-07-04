"""
Gemini AI service for itinerary generation.

Handles communication with the Google Gemini API, including prompt
construction, API calls with structured JSON output, response parsing,
Pydantic validation, and retry logic.
"""

import json
import logging
import time
from typing import Optional

import google.generativeai as genai

from app.config import Settings, get_settings
from app.models.itinerary_schema import ItineraryResponse
from app.models.trip_request import TripRequest
from app.services.prompt_builder import PromptBuilder

logger = logging.getLogger(__name__)


class GeminiServiceError(Exception):
    """Raised when Gemini API communication or response parsing fails."""

    pass


class GeminiService:
    """Service that interacts with Google Gemini to generate travel itineraries."""

    def __init__(self, settings: Optional[Settings] = None) -> None:
        """
        Initialize the Gemini service.

        Args:
            settings: Application settings. If None, loaded from environment.
        """
        self._settings = settings or get_settings()
        genai.configure(api_key=self._settings.GEMINI_API_KEY)
        self._model = genai.GenerativeModel(self._settings.GEMINI_MODEL)
        self._prompt_builder = PromptBuilder()
        logger.info(
            "GeminiService initialized with model: %s",
            self._settings.GEMINI_MODEL,
        )

    async def generate_itinerary(
        self, request: TripRequest
    ) -> ItineraryResponse:
        """
        Generate a structured travel itinerary using Gemini.

        Builds the prompt, calls Gemini with JSON response mode,
        parses and validates the response. Retries once on validation failure.

        Args:
            request: Validated trip request parameters.

        Returns:
            A validated ItineraryResponse object.

        Raises:
            GeminiServiceError: If Gemini fails after retries or returns
                                unparseable content.
        """
        prompt = self._prompt_builder.build_itinerary_prompt(request)
        max_attempts = 2
        last_error: Optional[Exception] = None

        for attempt in range(1, max_attempts + 1):
            start_time = time.monotonic()

            try:
                logger.info(
                    "Gemini API call attempt %d/%d for destination='%s'",
                    attempt,
                    max_attempts,
                    request.destination,
                )

                response = await self._call_gemini(prompt)
                elapsed = time.monotonic() - start_time

                logger.info(
                    "Gemini API responded in %.2fs (attempt %d)",
                    elapsed,
                    attempt,
                )

                itinerary = self._parse_and_validate(response)

                logger.info(
                    "Itinerary validated successfully: %d days for '%s'",
                    itinerary.total_days,
                    itinerary.destination,
                )

                return itinerary

            except (json.JSONDecodeError, ValueError) as exc:
                elapsed = time.monotonic() - start_time
                last_error = exc
                logger.warning(
                    "Validation failed on attempt %d (%.2fs): %s",
                    attempt,
                    elapsed,
                    str(exc),
                )

                if attempt < max_attempts:
                    logger.info("Retrying Gemini call...")
                    continue

            except Exception as exc:
                elapsed = time.monotonic() - start_time
                last_error = exc
                logger.error(
                    "Gemini API error on attempt %d (%.2fs): %s",
                    attempt,
                    elapsed,
                    str(exc),
                )

                if attempt < max_attempts:
                    logger.info("Retrying Gemini call...")
                    continue

        raise GeminiServiceError(
            f"Failed to generate itinerary after {max_attempts} attempts. "
            f"Last error: {last_error}"
        )

    async def _call_gemini(self, prompt: str) -> str:
        """
        Call the Gemini API with structured JSON output mode.

        Args:
            prompt: The complete prompt string.

        Returns:
            Raw text response from Gemini.

        Raises:
            GeminiServiceError: If the API call fails or returns empty content.
        """
        try:
            response = self._model.generate_content(
                prompt,
                generation_config=genai.GenerationConfig(
                    response_mime_type="application/json",
                    temperature=0.7,
                    max_output_tokens=8192,
                ),
            )

            if not response.text:
                raise GeminiServiceError(
                    "Gemini returned an empty response."
                )

            return response.text

        except GeminiServiceError:
            raise
        except Exception as exc:
            raise GeminiServiceError(
                f"Gemini API call failed: {exc}"
            ) from exc

    @staticmethod
    def _parse_and_validate(raw_json: str) -> ItineraryResponse:
        """
        Parse raw JSON string and validate against the ItineraryResponse schema.

        Args:
            raw_json: Raw JSON string from Gemini.

        Returns:
            A validated ItineraryResponse instance.

        Raises:
            json.JSONDecodeError: If the response is not valid JSON.
            ValueError: If the JSON doesn't match the expected schema.
        """
        # Strip potential markdown code fences that Gemini sometimes adds
        cleaned = raw_json.strip()
        if cleaned.startswith("```"):
            # Remove opening fence (```json or ```)
            first_newline = cleaned.index("\n")
            cleaned = cleaned[first_newline + 1 :]
        if cleaned.endswith("```"):
            cleaned = cleaned[:-3]
        cleaned = cleaned.strip()

        try:
            data = json.loads(cleaned)
        except json.JSONDecodeError as exc:
            logger.error("Failed to parse Gemini response as JSON: %s", exc)
            logger.debug("Raw response: %s", raw_json[:500])
            raise

        try:
            return ItineraryResponse.model_validate(data)
        except Exception as exc:
            logger.error("Schema validation failed: %s", exc)
            raise ValueError(
                f"Gemini response doesn't match expected schema: {exc}"
            ) from exc
