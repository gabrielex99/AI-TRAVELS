import { TripRequest, EnrichedItineraryResponse } from "./types";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

export async function generateItinerary(request: TripRequest): Promise<EnrichedItineraryResponse> {
  const response = await fetch(`${API_BASE_URL}/api/itinerary/generate`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      destination: request.destination,
      start_date: request.start_date,
      end_date: request.end_date,
      budget_max: request.budget_max,
      num_people: request.num_people,
    }),
  });

  if (!response.ok) {
    let errorMessage = "Errore durante la generazione dell'itinerario";
    try {
      const errorData = await response.json();
      if (errorData?.detail) {
        errorMessage = errorData.detail;
      } else if (errorData?.message) {
        errorMessage = errorData.message;
      }
    } catch {
      // Ignora errori di parsing del JSON di errore
    }
    throw new Error(errorMessage);
  }

  return response.json();
}

export async function getItineraryById(id: string): Promise<EnrichedItineraryResponse> {
  const response = await fetch(`${API_BASE_URL}/api/itinerary/${id}`);
  if (!response.ok) {
    throw new Error("Itinerario non trovato");
  }
  return response.json();
}

export async function getItineraryBySlug(slug: string): Promise<EnrichedItineraryResponse> {
  const response = await fetch(`${API_BASE_URL}/api/itinerary/slug/${slug}`);
  if (!response.ok) {
    throw new Error("Itinerario non trovato");
  }
  return response.json();
}
