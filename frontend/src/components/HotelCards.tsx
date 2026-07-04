"use client";

import React from "react";
import { Hotel, Star, ArrowRight } from "lucide-react";

interface HotelCardsProps {
  destination: string;
  budgetMax: number;
}

export default function HotelCards({ destination, budgetMax }: HotelCardsProps) {
  // Generate 3 mock hotels based on the destination
  const affiliateId = process.env.NEXT_PUBLIC_BOOKING_AID || "default-booking-aid";
  const searchUrl = `https://www.booking.com/searchresults.html?ss=${encodeURIComponent(
    destination
  )}&aid=${affiliateId}&selected_currency=EUR`;

  const hotels = [
    {
      name: `${destination} Grand Hotel & Spa`,
      type: "Lusso / Premium",
      rating: "9.2",
      price: Math.round(budgetMax * 0.3) + "€ / notte",
      reviews: "1,240 recensioni",
    },
    {
      name: `${destination} City Boutique Hotel`,
      type: "Moderno / Centrale",
      rating: "8.7",
      price: Math.round(budgetMax * 0.18) + "€ / notte",
      reviews: "850 recensioni",
    },
    {
      name: `${destination} Cozy & Budget B&B`,
      type: "Economico / Caratteristico",
      rating: "8.4",
      price: Math.round(budgetMax * 0.1) + "€ / notte",
      reviews: "420 recensioni",
    },
  ];

  return (
    <div className="widget-panel">
      <div style={{ display: "flex", alignItems: "center", gap: "0.5rem", marginBottom: "1rem" }}>
        <Hotel size={18} style={{ color: "#8b5cf6" }} />
        <h4 style={{ margin: 0 }}>Alloggi Consigliati</h4>
      </div>
      <p style={{ fontSize: "0.85rem", color: "var(--text-secondary)", marginBottom: "1.25rem" }}>
        Le migliori strutture selezionate in base al budget e con punteggio superiore a 8/10.
      </p>

      <div className="hotel-cards-list">
        {hotels.map((hotel, index) => (
          <a
            key={index}
            href={searchUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="hotel-card"
          >
            <div className="hotel-info">
              <div>
                <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", gap: "0.5rem" }}>
                  <span className="hotel-name">{hotel.name}</span>
                </div>
                <div style={{ fontSize: "0.75rem", color: "var(--text-muted)", marginTop: "0.2rem" }}>
                  {hotel.type}
                </div>
              </div>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginTop: "1rem" }}>
                <div className="hotel-rating">
                  <Star size={12} fill="#fbbf24" stroke="none" />
                  <span>{hotel.rating}</span>
                  <span style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>({hotel.reviews})</span>
                </div>
                <span className="hotel-price" style={{ fontWeight: 700, color: "var(--accent-blue)" }}>
                  {hotel.price}
                </span>
              </div>
            </div>
          </a>
        ))}
      </div>

      <a
        href={searchUrl}
        target="_blank"
        rel="noopener noreferrer"
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          gap: "0.5rem",
          marginTop: "1.5rem",
          color: "var(--text-secondary)",
          textDecoration: "none",
          fontSize: "0.85rem",
          fontWeight: 600,
        }}
        className="hover-bright"
      >
        Vedi tutte le strutture su Booking.com
        <ArrowRight size={14} />
      </a>
    </div>
  );
}
