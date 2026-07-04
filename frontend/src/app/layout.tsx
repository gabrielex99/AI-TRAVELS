import type { Metadata } from "next";
import "./globals.css";

export const metadata: Metadata = {
  title: "AI Trip Planner SaaS — Itinerari di Viaggio Istantanei",
  description: "Pianifica il tuo prossimo viaggio in meno di 10 secondi con l'intelligenza artificiale. Trova alloggi, esperienze e voli convenienti con link affiliati.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="it">
      <body>
        {/* Ambient background glows */}
        <div className="ambient-glow-1" />
        <div className="ambient-glow-2" />

        <header className="navbar">
          <div className="container nav-content">
            <a href="/" className="logo">
              🧭 AI Travels
            </a>
            <div style={{ display: "flex", gap: "1rem" }}>
              <span style={{ fontSize: "0.85rem", color: "var(--text-muted)", fontWeight: 500 }}>
                Powered by Gemini AI
              </span>
            </div>
          </div>
        </header>

        {children}
      </body>
    </html>
  );
}
