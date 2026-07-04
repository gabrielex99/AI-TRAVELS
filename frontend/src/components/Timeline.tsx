"use client";

import React from "react";
import { Sunrise, Sun, Sunset, ExternalLink } from "lucide-react";
import { DayPlan, ActivityOption, EnrichedItineraryResponse } from "@/lib/types";

interface TimelineProps {
  itinerary: DayPlan[];
  activeDay: number; // 1-based
  onDayChange: (day: number) => void;
  selectedOptions: Record<string, "option_a" | "option_b">; // Key: `${day}_${slot}`
  onOptionChange: (day: number, slot: "morning" | "afternoon" | "evening", option: "option_a" | "option_b") => void;
  affiliateLinks: Record<string, string>;
}

export default function Timeline({
  itinerary,
  activeDay,
  onDayChange,
  selectedOptions,
  onOptionChange,
  affiliateLinks,
}: TimelineProps) {
  const currentDayPlan = itinerary.find((d) => d.day === activeDay) || itinerary[0];

  if (!currentDayPlan) return null;

  const slots = [
    { key: "morning" as const, label: "Mattina", icon: Sunrise, colorClass: "morning" },
    { key: "afternoon" as const, label: "Pomeriggio", icon: Sun, colorClass: "afternoon" },
    { key: "evening" as const, label: "Sera", icon: Sunset, colorClass: "evening" },
  ];

  return (
    <div className="glass-card" style={{ padding: "2rem" }}>
      {/* Day Selector Tabs */}
      <div className="day-selector">
        {itinerary.map((dayPlan) => (
          <button
            key={dayPlan.day}
            onClick={() => onDayChange(dayPlan.day)}
            className={`day-tab ${activeDay === dayPlan.day ? "active" : ""}`}
          >
            Giorno {dayPlan.day}
          </button>
        ))}
      </div>

      {/* Theme Card */}
      <div className="day-theme-card">
        <h3 style={{ marginBottom: "0.25rem" }}>{currentDayPlan.theme}</h3>
        <p>Programma dettagliato per il giorno {currentDayPlan.day}</p>
      </div>

      {/* Slots List */}
      <div className="timeline-slots">
        {slots.map(({ key, label, icon: Icon, colorClass }) => {
          const slotData = currentDayPlan.slots[key];
          const selectedOptionKey = selectedOptions[`${activeDay}_${key}`] || "option_a";
          const activeActivity: ActivityOption = slotData[selectedOptionKey];

          // Get affiliate link
          const gygTerm = activeActivity.gyg_search_term;
          const affiliateUrl = affiliateLinks[gygTerm] || `https://www.getyourguide.com/s/?q=${encodeURIComponent(gygTerm)}`;

          return (
            <div key={key} className={`slot-card ${colorClass}`}>
              <div className="slot-label" style={{ display: "flex", alignItems: "center", gap: "0.5rem" }}>
                <Icon size={16} />
                {label}
              </div>

              {/* Option Switcher A/B */}
              <div className="option-toggle">
                <button
                  type="button"
                  className={`option-btn ${selectedOptionKey === "option_a" ? "active" : ""}`}
                  onClick={() => onOptionChange(activeDay, key, "option_a")}
                >
                  Opzione A
                </button>
                <button
                  type="button"
                  className={`option-btn ${selectedOptionKey === "option_b" ? "active" : ""}`}
                  onClick={() => onOptionChange(activeDay, key, "option_b")}
                >
                  Opzione B
                </button>
              </div>

              {/* Activity Card */}
              <div className="activity-card">
                <h4>{activeActivity.title}</h4>
                <p>{activeActivity.description}</p>
                <a
                  href={affiliateUrl}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="affiliate-btn"
                >
                  Prenota Esperienza
                  <ExternalLink size={14} />
                </a>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
