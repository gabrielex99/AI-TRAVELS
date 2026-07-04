"use client";

import React, { useState, useEffect } from "react";
import TripForm from "@/components/TripForm";
import SkeletonLoader from "@/components/SkeletonLoader";
import Timeline from "@/components/Timeline";
import HotelCards from "@/components/HotelCards";
import FlightWidget from "@/components/FlightWidget";
import { TripRequest, EnrichedItineraryResponse, ActivityOption } from "@/lib/types";
import { generateItinerary } from "@/lib/api";
import { AlertCircle, Sparkles } from "lucide-react";
import dynamic from "next/dynamic";

// Dynamically import MapView to avoid Leaflet SSR window errors
const MapView = dynamic(() => import("@/components/MapView"), {
  ssr: false,
  loading: () => (
    <div className="map-wrapper" style={{ display: "flex", alignItems: "center", justifyContent: "center" }}>
      Caricamento mappa...
    </div>
  ),
});

export default function Home() {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [itinerary, setItinerary] = useState<EnrichedItineraryResponse | null>(null);
  const [activeDay, setActiveDay] = useState(1);
  const [selectedOptions, setSelectedOptions] = useState<Record<string, "option_a" | "option_b">>({});

  const handleFormSubmit = async (request: TripRequest) => {
    setIsLoading(true);
    setError(null);
    setItinerary(null);
    try {
      const data = await generateItinerary(request);
      
      // Initialize all slots to option_a
      const initialOptions: Record<string, "option_a" | "option_b"> = {};
      data.itinerary.forEach((dayPlan) => {
        initialOptions[`${dayPlan.day}_morning`] = "option_a";
        initialOptions[`${dayPlan.day}_afternoon`] = "option_a";
        initialOptions[`${dayPlan.day}_evening`] = "option_a";
      });

      setSelectedOptions(initialOptions);
      setItinerary(data);
      setActiveDay(1);
    } catch (err: any) {
      setError(err?.message || "Errore sconosciuto durante la generazione dell'itinerario.");
    } finally {
      setIsLoading(false);
    }
  };

  const handleOptionChange = (
    day: number,
    slot: "morning" | "afternoon" | "evening",
    option: "option_a" | "option_b"
  ) => {
    setSelectedOptions((prev) => ({
      ...prev,
      [`${day}_${slot}`]: option,
    }));
  };

  // Get active coordinates for MapView based on selected options
  const getActiveActivities = () => {
    if (!itinerary) return null;
    const currentDayPlan = itinerary.itinerary.find((d) => d.day === activeDay) || itinerary.itinerary[0];
    if (!currentDayPlan) return null;

    const morningOpt = selectedOptions[`${activeDay}_morning`] || "option_a";
    const afternoonOpt = selectedOptions[`${activeDay}_afternoon`] || "option_a";
    const eveningOpt = selectedOptions[`${activeDay}_evening`] || "option_a";

    return {
      morning: currentDayPlan.slots.morning[morningOpt],
      afternoon: currentDayPlan.slots.afternoon[afternoonOpt],
      evening: currentDayPlan.slots.evening[eveningOpt],
    };
  };

  const activeActivities = getActiveActivities();

  return (
    <main style={{ paddingBottom: "5rem" }}>
      <section className="hero">
        <div className="container">
          <h1>
            Crea il tuo viaggio ideale <br />
            <span>in 10 secondi con AI</span>
          </h1>
          <p>
            Addio ore perse a pianificare. Inserisci la tua meta, decidi il budget ed ottieni subito
            un itinerario ottimizzato con attività, mappe e alloggi ideali.
          </p>

          <TripForm onSubmit={handleFormSubmit} isLoading={isLoading} />
        </div>
      </section>

      <section className="container">
        {/* Error banner */}
        {error && (
          <div className="error-banner">
            <AlertCircle size={18} />
            <span>{error}</span>
          </div>
        )}

        {/* Shimmer loading screen */}
        {isLoading && <SkeletonLoader />}

        {/* Render results when ready */}
        {itinerary && !isLoading && (
          <div className="results-layout">
            {/* Left side: Timeline */}
            <div>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "1.5rem" }}>
                <h2 style={{ fontSize: "2rem", display: "flex", alignItems: "center", gap: "0.5rem" }}>
                  <span>Il tuo itinerario a {itinerary.destination}</span>
                </h2>
                {itinerary.slug && (
                  <a
                    href={`/itinerary/${itinerary.trip_id || itinerary.id}`}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="btn-secondary"
                    style={{ fontSize: "0.85rem", padding: "0.5rem 1rem" }}
                  >
                    Condividi Link Pubblico
                  </a>
                )}
              </div>

              <Timeline
                itinerary={itinerary.itinerary}
                activeDay={activeDay}
                onDayChange={setActiveDay}
                selectedOptions={selectedOptions}
                onOptionChange={handleOptionChange}
                affiliateLinks={itinerary.affiliate_links}
              />
            </div>

            {/* Right side: Map, Hotels, Flights */}
            <div>
              <div className="map-sticky-container">
                {activeActivities && (
                  <div style={{ height: "450px" }}>
                    <MapView
                      morning={activeActivities.morning}
                      afternoon={activeActivities.afternoon}
                      evening={activeActivities.evening}
                    />
                  </div>
                )}

                <HotelCards
                  destination={itinerary.destination}
                  budgetMax={itinerary.budget_max || 500}
                />

                {itinerary.flight_widget_params && (
                  <FlightWidget params={itinerary.flight_widget_params} />
                )}
              </div>
            </div>
          </div>
        )}
      </section>
    </main>
  );
}
