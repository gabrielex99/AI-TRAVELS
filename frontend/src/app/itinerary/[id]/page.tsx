"use client";

import React, { useState, useEffect } from "react";
import { useParams } from "next/navigation";
import SkeletonLoader from "@/components/SkeletonLoader";
import Timeline from "@/components/Timeline";
import HotelCards from "@/components/HotelCards";
import FlightWidget from "@/components/FlightWidget";
import { EnrichedItineraryResponse } from "@/lib/types";
import { getItineraryById } from "@/lib/api";
import { AlertCircle, Calendar, ArrowLeft } from "lucide-react";
import dynamic from "next/dynamic";

const MapView = dynamic(() => import("@/components/MapView"), {
  ssr: false,
  loading: () => (
    <div className="map-wrapper" style={{ display: "flex", alignItems: "center", justifyContent: "center" }}>
      Caricamento mappa...
    </div>
  ),
});

export default function ItineraryPage() {
  const { id } = useParams() as { id: string };
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [itinerary, setItinerary] = useState<EnrichedItineraryResponse | null>(null);
  const [activeDay, setActiveDay] = useState(1);
  const [selectedOptions, setSelectedOptions] = useState<Record<string, "option_a" | "option_b">>({});

  useEffect(() => {
    if (!id) return;

    const fetchItinerary = async () => {
      try {
        setIsLoading(true);
        const data = await getItineraryById(id);
        
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
        setError(err?.message || "Impossibile caricare l'itinerario richiesto.");
      } finally {
        setIsLoading(false);
      }
    };

    fetchItinerary();
  }, [id]);

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

  if (isLoading) {
    return (
      <div className="container" style={{ paddingTop: "4rem" }}>
        <SkeletonLoader />
      </div>
    );
  }

  if (error || !itinerary) {
    return (
      <div className="container" style={{ paddingTop: "6rem", textAlign: "center" }}>
        <div className="error-banner" style={{ display: "inline-flex", margin: "0 auto 2rem auto" }}>
          <AlertCircle size={18} />
          <span>{error || "Itinerario non trovato."}</span>
        </div>
        <div>
          <a href="/" className="btn-secondary" style={{ display: "inline-flex", alignItems: "center", gap: "0.5rem" }}>
            <ArrowLeft size={16} />
            Torna alla Homepage
          </a>
        </div>
      </div>
    );
  }

  return (
    <main style={{ paddingBottom: "5rem", paddingTop: "3rem" }}>
      <div className="container">
        <a 
          href="/" 
          className="btn-secondary" 
          style={{ display: "inline-flex", alignItems: "center", gap: "0.5rem", marginBottom: "2rem" }}
        >
          <ArrowLeft size={16} />
          Crea un nuovo itinerario
        </a>

        <div className="results-layout">
          {/* Left side: Timeline */}
          <div>
            <h2 style={{ fontSize: "2.2rem", marginBottom: "1.5rem" }}>
              Itinerario per {itinerary.destination}
            </h2>
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
      </div>
    </main>
  );
}
