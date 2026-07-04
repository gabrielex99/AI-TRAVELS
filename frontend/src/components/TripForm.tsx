"use client";

import React, { useState } from "react";
import { MapPin, Calendar, DollarSign, Users, Sparkles } from "lucide-react";
import { TripRequest } from "@/lib/types";

interface TripFormProps {
  onSubmit: (request: TripRequest) => void;
  isLoading: boolean;
}

export default function TripForm({ onSubmit, isLoading }: TripFormProps) {
  const [destination, setDestination] = useState("");
  const [startDate, setStartDate] = useState("");
  const [endDate, setEndDate] = useState("");
  const [budgetMax, setBudgetMax] = useState(500);
  const [numPeople, setNumPeople] = useState(2);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!destination || !startDate || !endDate || budgetMax <= 0 || numPeople <= 0) {
      return;
    }
    onSubmit({
      destination,
      start_date: startDate,
      end_date: endDate,
      budget_max: Number(budgetMax),
      num_people: Number(numPeople),
    });
  };

  return (
    <form onSubmit={handleSubmit} className="glass-card">
      <div className="trip-form-grid">
        <div className="form-group">
          <label htmlFor="destination">Destinazione</label>
          <div className="input-wrapper">
            <MapPin size={18} />
            <input
              id="destination"
              type="text"
              className="input-field"
              placeholder="Es. Catania, Parigi, Barcellona..."
              value={destination}
              onChange={(e) => setDestination(e.target.value)}
              required
              disabled={isLoading}
            />
          </div>
        </div>

        <div className="form-group">
          <label htmlFor="dates">Date (Inizio - Fine)</label>
          <div className="input-wrapper" style={{ gap: "0.5rem" }}>
            <Calendar size={18} />
            <input
              id="start_date"
              type="date"
              className="input-field"
              style={{ paddingLeft: "2.8rem" }}
              value={startDate}
              onChange={(e) => setStartDate(e.target.value)}
              required
              disabled={isLoading}
            />
            <input
              id="end_date"
              type="date"
              className="input-field"
              style={{ paddingLeft: "1rem" }}
              value={endDate}
              onChange={(e) => setEndDate(e.target.value)}
              required
              disabled={isLoading}
            />
          </div>
        </div>

        <div className="form-group">
          <label htmlFor="budget">Budget Max (€)</label>
          <div className="input-wrapper">
            <DollarSign size={18} />
            <input
              id="budget"
              type="number"
              className="input-field"
              min="1"
              placeholder="500"
              value={budgetMax}
              onChange={(e) => setBudgetMax(Number(e.target.value))}
              required
              disabled={isLoading}
            />
          </div>
        </div>

        <div className="form-group">
          <label htmlFor="people">Persone</label>
          <div className="input-wrapper">
            <Users size={18} />
            <input
              id="people"
              type="number"
              className="input-field"
              min="1"
              max="20"
              placeholder="2"
              value={numPeople}
              onChange={(e) => setNumPeople(Number(e.target.value))}
              required
              disabled={isLoading}
            />
          </div>
        </div>
      </div>

      <div style={{ marginTop: "2rem", display: "flex", justifyContent: "center" }}>
        <button type="submit" className="btn-primary" disabled={isLoading}>
          {isLoading ? (
            <>
              <div className="shimmer-element" style={{ width: "20px", height: "20px", borderRadius: "50%", marginRight: "0.5rem" }} />
              Generazione in corso...
            </>
          ) : (
            <>
              <Sparkles size={18} />
              Pianifica con AI
            </>
          )}
        </button>
      </div>
    </form>
  );
}
