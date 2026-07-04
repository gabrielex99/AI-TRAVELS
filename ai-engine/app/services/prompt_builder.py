"""
Prompt builder for Gemini itinerary generation.

Constructs a detailed Italian-language prompt that instructs Gemini
to return a structured JSON itinerary matching the exact schema.
"""

from app.models.trip_request import TripRequest


class PromptBuilder:
    """Builds structured prompts for Gemini itinerary generation."""

    @staticmethod
    def build_itinerary_prompt(request: TripRequest) -> str:
        """
        Build a detailed prompt for generating a travel itinerary.

        The prompt is written in Italian and instructs Gemini to respond
        ONLY with valid JSON matching the ItineraryResponse schema.

        Args:
            request: Validated trip request parameters.

        Returns:
            A complete prompt string ready for Gemini.
        """
        preferences_text = (
            ", ".join(request.preferences)
            if request.preferences
            else "nessuna preferenza specifica"
        )

        prompt = f"""Sei un esperto travel planner AI. Devi generare un itinerario di viaggio dettagliato e strutturato.

## PARAMETRI DEL VIAGGIO
- **Destinazione**: {request.destination}
- **Data inizio**: {request.start_date}
- **Data fine**: {request.end_date}
- **Durata totale**: {request.total_days} giorni
- **Budget massimo**: €{request.budget_max:.2f}
- **Numero di persone**: {request.num_people}
- **Preferenze**: {preferences_text}

## ISTRUZIONI

1. Genera un itinerario di **esattamente {request.total_days} giorni** per **{request.destination}**.
2. Per ogni giorno, fornisci un **tema** descrittivo e tre fasce orarie: **morning** (mattina), **afternoon** (pomeriggio), **evening** (sera).
3. Per ogni fascia oraria, fornisci **due opzioni** (option_a e option_b) con attività diverse ma entrambe valide.
4. Ogni opzione deve includere:
   - **title**: nome dell'attività o punto di interesse (in italiano)
   - **description**: descrizione breve ma coinvolgente dell'attività (in italiano, 1-2 frasi)
   - **latitude**: latitudine reale e accurata del luogo (numero decimale)
   - **longitude**: longitudine reale e accurata del luogo (numero decimale)
   - **gyg_search_term**: termine di ricerca in inglese per GetYourGuide (es. "Colosseum guided tour Rome")
5. Usa **luoghi reali** con **coordinate GPS accurate**. Non inventare luoghi inesistenti.
6. Le attività devono essere **variate** e rispettare le preferenze indicate.
7. Rispetta il budget indicato nelle scelte proposte.
8. Fornisci i parametri per il widget voli:
   - **destination_iata**: codice IATA dell'aeroporto principale più vicino alla destinazione (3 lettere)
   - **suggested_months**: lista dei mesi suggeriti per il viaggio in formato "YYYY-MM"

## SCHEMA JSON OBBLIGATORIO

Rispondi **ESCLUSIVAMENTE** con un oggetto JSON valido che segua **esattamente** questo schema, senza testo aggiuntivo, senza commenti, senza markdown:

{{
  "destination": "{request.destination}",
  "total_days": {request.total_days},
  "flight_widget_params": {{
    "destination_iata": "XXX",
    "suggested_months": ["YYYY-MM"]
  }},
  "itinerary": [
    {{
      "day": 1,
      "theme": "Titolo tematico del giorno",
      "slots": {{
        "morning": {{
          "option_a": {{
            "title": "Nome attività",
            "description": "Descrizione attività",
            "latitude": 0.0,
            "longitude": 0.0,
            "gyg_search_term": "search term in english"
          }},
          "option_b": {{
            "title": "Nome attività alternativa",
            "description": "Descrizione attività alternativa",
            "latitude": 0.0,
            "longitude": 0.0,
            "gyg_search_term": "search term in english"
          }}
        }},
        "afternoon": {{
          "option_a": {{ ... }},
          "option_b": {{ ... }}
        }},
        "evening": {{
          "option_a": {{ ... }},
          "option_b": {{ ... }}
        }}
      }}
    }}
  ]
}}

IMPORTANTE: La risposta deve contenere SOLO il JSON, nessun altro testo."""

        return prompt
