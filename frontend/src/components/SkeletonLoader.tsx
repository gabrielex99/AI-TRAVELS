import React from "react";

export default function SkeletonLoader() {
  return (
    <div className="shimmer-wrapper">
      <div className="shimmer-element shimmer-title" style={{ marginBottom: "2rem" }} />
      <div className="shimmer-element shimmer-text" style={{ width: "80%", marginBottom: "1rem" }} />
      <div className="shimmer-element shimmer-text" style={{ width: "95%", marginBottom: "3rem" }} />
      
      <div style={{ display: "flex", gap: "1rem", marginBottom: "2rem" }}>
        <div className="shimmer-element" style={{ width: "100px", height: "40px", borderRadius: "10px" }} />
        <div className="shimmer-element" style={{ width: "100px", height: "40px", borderRadius: "10px" }} />
        <div className="shimmer-element" style={{ width: "100px", height: "40px", borderRadius: "10px" }} />
      </div>

      <div className="shimmer-element shimmer-card" style={{ marginBottom: "2rem" }} />
      <div className="shimmer-element shimmer-card" style={{ marginBottom: "2rem" }} />
      <div className="shimmer-element shimmer-card" />
    </div>
  );
}
