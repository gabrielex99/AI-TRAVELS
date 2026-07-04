"use client";

import React from "react";
import { Plane, Calendar, ExternalLink } from "lucide-react";
import { FlightWidgetParams } from "@/lib/types";

interface FlightWidgetProps {
  params: FlightWidgetParams;
}

export default function FlightWidget({ params }: FlightWidgetProps) {
  const marker = process.env.NEXT_PUBLIC_AVIASALES_MARKER || "default-marker";
  
  // Format search URL (Aviasales / Jetradar affiliate link format)
  // Example format: https://www.aviasales.com/?marker=12345&destination=CTA
  const searchUrl = `https://www.aviasales.com/?marker=${marker}&destination=${params.destination_iata}`;

  return (
    <div className="widget-panel" style={{ marginTop: "1.5rem" }}>
      <div style={{ display: "flex", alignItems: "center", gap: "0.5rem", marginBottom: "1rem" }}>
        <Plane size={18} style={{ color: "#06b6d4" }} />
        <h4 style={{ margin: 0 }}>Cerca Voli Economici</h4>
      </div>
      
      <p style={{ fontSize: "0.85rem", color: "var(--text-secondary)", marginBottom: "1.25rem" }}>
        Trova voli per l'aeroporto di <strong>{params.destination_iata}</strong> nei periodi consigliati.
      </p>

      {/* Suggested months pills */}
      <div style={{ display: "flex", gap: "0.5rem", flexWrap: "wrap", marginBottom: "1.5rem" }}>
        {params.suggested_months.map((month, idx) => (
          <span 
            key={idx} 
            style={{ 
              fontSize: "0.75rem", 
              background: "rgba(6, 182, 212, 0.08)", 
              border: "1px solid rgba(6, 182, 212, 0.15)",
              color: "var(--accent-blue)",
              padding: "0.25rem 0.6rem",
              borderRadius: "20px",
              display: "flex",
              alignItems: "center",
              gap: "0.25rem"
            }}
          >
            <Calendar size={10} />
            Mese: {month}
          </span>
        ))}
      </div>

      <div 
        style={{ 
          background: "var(--bg-tertiary)",
          border: "1px solid var(--border-glass)",
          borderRadius: "12px",
          padding: "1.25rem",
          textAlign: "center"
        }}
      >
        <span style={{ fontSize: "0.8rem", color: "var(--text-muted)", display: "block", marginBottom: "0.5rem" }}>
          Destinazione principale
        </span>
        <span style={{ fontSize: "2rem", fontWeight: 800, fontFamily: "var(--font-display)", color: "var(--text-primary)", display: "block", lineHeight: 1 }}>
          {params.destination_iata}
        </span>
        
        <a 
          href={searchUrl} 
          target="_blank" 
          rel="noopener noreferrer" 
          className="btn-primary" 
          style={{ width: "100%", marginTop: "1.25rem", padding: "0.75rem" }}
        >
          Cerca su Aviasales
          <ExternalLink size={14} />
        </a>
      </div>
    </div>
  );
}
