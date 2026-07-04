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
        "destination": "Rome, Italy",
        "total_days": 5,
        "flight_widget_params": {
            "destination_iata": "FCO",
            "suggested_months": ["2026-09"]
        },
        "itinerary": [
            {
                "day": 1,
                "theme": "Arrivo e Roma Antica",
                "slots": {
                    "morning": {
                        "option_a": {
                            "title": "Colosseo & Foro Romano",
                            "description": "Visita guidata tra i monumenti dell'antica Roma.",
                            "latitude": 41.8902,
                            "longitude": 12.4922,
                            "gyg_search_term": "Colosseum guided tour Rome"
                        },
                        "option_b": {
                            "title": "Passeggiata al Circo Massimo",
                            "description": "Cammina tra le rovine dell'antico stadio romano.",
                            "latitude": 41.8860,
                            "longitude": 12.4854,
                            "gyg_search_term": "Circus Maximus Rome walking tour"
                        }
                    },
                    "afternoon": {
                        "option_a": {
                            "title": "Pranzo a Monti",
                            "description": "Prova la cucina romana in un locale tradizionale.",
                            "latitude": 41.8955,
                            "longitude": 12.4868,
                            "gyg_search_term": "Monti Rome food tour"
                        },
                        "option_b": {
                            "title": "Visita al Palatino",
                            "description": "Scopri i palazzi imperiali e la vista sulla città.",
                            "latitude": 41.8894,
                            "longitude": 12.4881,
                            "gyg_search_term": "Palatine Hill Rome tickets"
                        }
                    },
                    "evening": {
                        "option_a": {
                            "title": "Cena a Trastevere",
                            "description": "Cena in un quartiere pittoresco con atmosfera romana.",
                            "latitude": 41.8899,
                            "longitude": 12.4723,
                            "gyg_search_term": "Trastevere dinner experience"
                        },
                        "option_b": {
                            "title": "Aperitivo in piazza Navona",
                            "description": "Goditi un aperitivo tra le statue barocche.",
                            "latitude": 41.8986,
                            "longitude": 12.4731,
                            "gyg_search_term": "Piazza Navona aperitivo"
                        }
                    }
                }
            },
            {
                "day": 2,
                "theme": "Vaticano e Arte Sacra",
                "slots": {
                    "morning": {
                        "option_a": {
                            "title": "Musei Vaticani & Cappella Sistina",
                            "description": "Ammira i capolavori rinascimentali.",
                            "latitude": 41.9065,
                            "longitude": 12.4536,
                            "gyg_search_term": "Vatican Museums guided tour"
                        },
                        "option_b": {
                            "title": "Basilica di San Pietro",
                            "description": "Visita la basilica e sali sulla cupola.",
                            "latitude": 41.9022,
                            "longitude": 12.4539,
                            "gyg_search_term": "St Peter's Basilica dome tour"
                        }
                    },
                    "afternoon": {
                        "option_a": {
                            "title": "Pranzo vicino al Vaticano",
                            "description": "Pranzo tipico in un locale elegante.",
                            "latitude": 41.9051,
                            "longitude": 12.4553,
                            "gyg_search_term": "Vatican area lunch"
                        },
                        "option_b": {
                            "title": "Passeggiata a Piazza del Popolo",
                            "description": "Passeggia tra piazze storiche e negozi.",
                            "latitude": 41.9101,
                            "longitude": 12.4769,
                            "gyg_search_term": "Rome Piazza del Popolo walking tour"
                        }
                    },
                    "evening": {
                        "option_a": {
                            "title": "Tramonto al Gianicolo",
                            "description": "Ammira la città dal colle panoramico.",
                            "latitude": 41.8896,
                            "longitude": 12.4665,
                            "gyg_search_term": "Gianicolo sunset tour Rome"
                        },
                        "option_b": {
                            "title": "Cena a Prati",
                            "description": "Cena in uno dei quartieri più eleganti di Roma.",
                            "latitude": 41.9104,
                            "longitude": 12.4616,
                            "gyg_search_term": "Prati Rome dinner"
                        }
                    }
                }
            }
        ]
    }'::jsonb,
    '{
        "hotel_search": "https://www.booking.com/searchresults.html?ss=Rome&aid=",
        "activities_search": "https://www.getyourguide.com/rome-l711/",
        "flight_search": "https://www.aviasales.com/"
    }'::jsonb,
    'sha256_rome_5d_1500_2p_20260901',
    TRUE,
    'rome-italy-5-days-2-people'
) ON CONFLICT (search_hash) DO UPDATE SET
    destination = EXCLUDED.destination,
    total_days = EXCLUDED.total_days,
    budget_max = EXCLUDED.budget_max,
    num_people = EXCLUDED.num_people,
    start_date = EXCLUDED.start_date,
    end_date = EXCLUDED.end_date,
    itinerary_data = EXCLUDED.itinerary_data,
    affiliate_data = EXCLUDED.affiliate_data,
    is_public = EXCLUDED.is_public,
    slug = EXCLUDED.slug,
    updated_at = NOW();

-- ============================================================
-- Done
-- ============================================================
