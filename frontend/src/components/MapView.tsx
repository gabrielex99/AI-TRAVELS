"use client";

import React, { useEffect, useRef } from "react";
import { ActivityOption } from "@/lib/types";

interface MapViewProps {
  morning: ActivityOption;
  afternoon: ActivityOption;
  evening: ActivityOption;
}

export default function MapView({ morning, afternoon, evening }: MapViewProps) {
  const mapContainerRef = useRef<HTMLDivElement>(null);
  const mapRef = useRef<any>(null);
  const markersRef = useRef<any[]>([]);
  const polylineRef = useRef<any>(null);

  useEffect(() => {
    // Only run on client-side
    if (typeof window === "undefined" || !mapContainerRef.current) return;

    let isMounted = true;

    // Dynamically load Leaflet
    const initMap = async () => {
      const L = (await import("leaflet")).default;
      await import("leaflet/dist/leaflet.css");

      if (!isMounted) return;

      // Fix icon marker paths in Leaflet
      delete (L.Icon.Default.prototype as any)._getIconUrl;
      L.Icon.Default.mergeOptions({
        iconRetinaUrl: "https://unpkg.com/leaflet@1.7.1/dist/images/marker-icon-2x.png",
        iconUrl: "https://unpkg.com/leaflet@1.7.1/dist/images/marker-icon.png",
        shadowUrl: "https://unpkg.com/leaflet@1.7.1/dist/images/marker-shadow.png",
      });

      // Clear existing map instance if any
      if (mapRef.current) {
        mapRef.current.remove();
        mapRef.current = null;
      }

      const activities = [
        { ...morning, label: "Mattina" },
        { ...afternoon, label: "Pomeriggio" },
        { ...evening, label: "Sera" },
      ];

      // Use first activity coordinates as center
      const centerLat = morning.latitude || 0;
      const centerLng = morning.longitude || 0;

      // Initialize map
      const map = L.map(mapContainerRef.current).setView([centerLat, centerLng], 13);
      mapRef.current = map;

      // Add dark map tiles (CartoDB Dark Matter fits our dark premium theme beautifully!)
      L.tileLayer("https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png", {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>',
        subdomains: "abcd",
        maxZoom: 20,
      }).addTo(map);

      updateMarkers(L, activities);
    };

    initMap();

    return () => {
      isMounted = false;
      if (mapRef.current) {
        mapRef.current.remove();
        mapRef.current = null;
      }
    };
  }, [morning, afternoon, evening]);

  const updateMarkers = (L: any, activities: any[]) => {
    const map = mapRef.current;
    if (!map) return;

    // Remove existing markers
    markersRef.current.forEach((marker) => marker.remove());
    markersRef.current = [];

    // Remove existing polyline
    if (polylineRef.current) {
      polylineRef.current.remove();
      polylineRef.current = null;
    }

    const latlngs: any[] = [];

    activities.forEach((activity, idx) => {
      if (activity.latitude && activity.longitude) {
        const latlng = [activity.latitude, activity.longitude] as [number, number];
        latlngs.push(latlng);

        // Custom icon styling for premium feel
        const markerColor = idx === 0 ? "#06b6d4" : idx === 1 ? "#8b5cf6" : "#ec4899";
        const divIcon = L.divIcon({
          className: "custom-leaflet-marker-div",
          html: `<div style="
            width: 24px;
            height: 24px;
            border-radius: 50%;
            background: ${markerColor};
            border: 3px solid white;
            box-shadow: 0 0 10px ${markerColor};
          "></div>`,
          iconSize: [24, 24],
          iconAnchor: [12, 12],
        });

        const marker = L.marker(latlng, { icon: divIcon })
          .addTo(map)
          .bindPopup(`
            <div style="color: #030303; font-family: sans-serif; font-size: 0.85rem;">
              <strong>${activity.label}: ${activity.title}</strong>
              <p style="margin: 4px 0 0 0; color: #666;">${activity.description.substring(0, 80)}...</p>
            </div>
          `);

        markersRef.current.push(marker);
      }
    });

    // Draw route line between activities
    if (latlngs.length > 1) {
      const polyline = L.polyline(latlngs, {
        color: "rgba(6, 182, 212, 0.4)",
        weight: 3,
        dashArray: "6, 6",
      }).addTo(map);
      polylineRef.current = polyline;

      // Adjust map bounds to show all markers
      const group = L.featureGroup(markersRef.current);
      map.fitBounds(group.getBounds().pad(0.15));
    }
  };

  return (
    <div className="map-wrapper" style={{ width: "100%", height: "100%", position: "relative" }}>
      <div ref={mapContainerRef} style={{ width: "100%", height: "100%", minHeight: "350px" }} />
    </div>
  );
}
