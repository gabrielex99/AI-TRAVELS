-- ============================================================
-- AI Travels – Database Initialization
-- ============================================================

-- Enable required extensions
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- ────────────────────────────────────────────────────────────
-- Function: auto-update updated_at on row change
-- ────────────────────────────────────────────────────────────
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- ────────────────────────────────────────────────────────────
-- Table: trips
-- ────────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS trips (
    id              UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    destination     VARCHAR(255)    NOT NULL,
    total_days      INTEGER         NOT NULL CHECK (total_days > 0),
    budget_max      NUMERIC(10, 2)  CHECK (budget_max >= 0),
    num_people      INTEGER         NOT NULL DEFAULT 1 CHECK (num_people > 0),
    start_date      DATE,
    end_date        DATE,
    itinerary_data  JSONB           NOT NULL DEFAULT '{}',
    affiliate_data  JSONB           NOT NULL DEFAULT '{}',
    search_hash     VARCHAR(64)     UNIQUE,
    is_public       BOOLEAN         NOT NULL DEFAULT FALSE,
    slug            VARCHAR(280)    UNIQUE,
    view_count      INTEGER         NOT NULL DEFAULT 0,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),

    CONSTRAINT chk_dates CHECK (end_date IS NULL OR start_date IS NULL OR end_date >= start_date)
);

-- ────────────────────────────────────────────────────────────
-- Indexes
-- ────────────────────────────────────────────────────────────
CREATE INDEX IF NOT EXISTS idx_trips_search_hash   ON trips (search_hash);
CREATE INDEX IF NOT EXISTS idx_trips_slug          ON trips (slug);
CREATE INDEX IF NOT EXISTS idx_trips_destination   ON trips USING GIN (destination gin_trgm_ops);
CREATE INDEX IF NOT EXISTS idx_trips_itinerary     ON trips USING GIN (itinerary_data);
CREATE INDEX IF NOT EXISTS idx_trips_is_public     ON trips (is_public) WHERE is_public = TRUE;
CREATE INDEX IF NOT EXISTS idx_trips_created_at    ON trips (created_at DESC);

-- ────────────────────────────────────────────────────────────
-- Trigger: auto-update updated_at
-- ────────────────────────────────────────────────────────────
DROP TRIGGER IF EXISTS trg_trips_updated_at ON trips;
CREATE TRIGGER trg_trips_updated_at
    BEFORE UPDATE ON trips
    FOR EACH ROW
    EXECUTE FUNCTION update_updated_at_column();

-- ────────────────────────────────────────────────────────────
-- Seed data: sample trip
-- ────────────────────────────────────────────────────────────
INSERT INTO trips (
    destination,
    total_days,
    budget_max,
    num_people,
    start_date,
    end_date,
    itinerary_data,
    affiliate_data,
    search_hash,
    is_public,
    slug
) VALUES (
    'Rome, Italy',
    5,
    1500.00,
    2,
    '2026-09-01',
    '2026-09-05',
    '{
        "days": [
            {
                "day": 1,
                "title": "Arrival & Ancient Rome",
                "activities": [
                    {"time": "10:00", "name": "Colosseum & Roman Forum", "duration_hours": 3, "cost_estimate": 18.00, "category": "sightseeing"},
                    {"time": "14:00", "name": "Lunch in Monti", "duration_hours": 1.5, "cost_estimate": 25.00, "category": "food"},
                    {"time": "16:00", "name": "Palatine Hill", "duration_hours": 2, "cost_estimate": 0, "category": "sightseeing"},
                    {"time": "20:00", "name": "Dinner in Trastevere", "duration_hours": 2, "cost_estimate": 40.00, "category": "food"}
                ]
            },
            {
                "day": 2,
                "title": "Vatican & Culture",
                "activities": [
                    {"time": "08:30", "name": "Vatican Museums & Sistine Chapel", "duration_hours": 4, "cost_estimate": 17.00, "category": "sightseeing"},
                    {"time": "13:00", "name": "Lunch near Vatican", "duration_hours": 1, "cost_estimate": 20.00, "category": "food"},
                    {"time": "15:00", "name": "St. Peter''s Basilica", "duration_hours": 2, "cost_estimate": 0, "category": "sightseeing"},
                    {"time": "18:00", "name": "Castel Sant''Angelo", "duration_hours": 1.5, "cost_estimate": 15.00, "category": "sightseeing"}
                ]
            },
            {
                "day": 3,
                "title": "Baroque Rome & Piazzas",
                "activities": [
                    {"time": "09:00", "name": "Trevi Fountain & Spanish Steps", "duration_hours": 2, "cost_estimate": 0, "category": "sightseeing"},
                    {"time": "11:30", "name": "Pantheon", "duration_hours": 1, "cost_estimate": 5.00, "category": "sightseeing"},
                    {"time": "13:00", "name": "Lunch at Piazza Navona", "duration_hours": 1.5, "cost_estimate": 30.00, "category": "food"},
                    {"time": "15:30", "name": "Galleria Borghese", "duration_hours": 2, "cost_estimate": 15.00, "category": "sightseeing"}
                ]
            },
            {
                "day": 4,
                "title": "Day Trip – Tivoli",
                "activities": [
                    {"time": "09:00", "name": "Villa d''Este", "duration_hours": 3, "cost_estimate": 13.00, "category": "sightseeing"},
                    {"time": "13:00", "name": "Lunch in Tivoli", "duration_hours": 1, "cost_estimate": 20.00, "category": "food"},
                    {"time": "14:30", "name": "Hadrian''s Villa", "duration_hours": 3, "cost_estimate": 10.00, "category": "sightseeing"},
                    {"time": "19:00", "name": "Dinner back in Rome", "duration_hours": 2, "cost_estimate": 45.00, "category": "food"}
                ]
            },
            {
                "day": 5,
                "title": "Hidden Gems & Departure",
                "activities": [
                    {"time": "09:00", "name": "Aventine Hill & Orange Garden", "duration_hours": 1.5, "cost_estimate": 0, "category": "sightseeing"},
                    {"time": "11:00", "name": "Testaccio Market", "duration_hours": 1.5, "cost_estimate": 15.00, "category": "food"},
                    {"time": "13:00", "name": "Final Gelato Tour", "duration_hours": 1, "cost_estimate": 10.00, "category": "food"},
                    {"time": "15:00", "name": "Transfer to Airport", "duration_hours": 1, "cost_estimate": 50.00, "category": "transport"}
                ]
            }
        ],
        "summary": {
            "total_estimated_cost": 348.00,
            "currency": "EUR",
            "highlights": ["Colosseum", "Vatican Museums", "Tivoli Day Trip", "Trastevere Dining"]
        }
    }'::jsonb,
    '{
        "hotels": [
            {"provider": "booking", "url": "https://www.booking.com/searchresults.html?ss=Rome&aid=", "label": "Find Hotels in Rome"}
        ],
        "activities": [
            {"provider": "getyourguide", "url": "https://www.getyourguide.com/rome-l711/", "label": "Book Tours in Rome"}
        ],
        "flights": [
            {"provider": "aviasales", "url": "https://www.aviasales.com/", "label": "Find Flights to Rome"}
        ]
    }'::jsonb,
    'sha256_rome_5d_1500_2p_20260901',
    TRUE,
    'rome-italy-5-days-2-people'
) ON CONFLICT (search_hash) DO NOTHING;

-- ============================================================
-- Done
-- ============================================================
