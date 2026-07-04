# ✈️ Travel Planner — Itinerari di Viaggio Istantanei

> Genera itinerari di viaggio completi e personalizzati in pochi secondi, con monetizzazione affiliate incorporata.

Travel Planner è un'app full-stack che consente di creare piani di viaggio dettagliati. Gli utenti inseriscono una destinazione, le date, il budget e il numero di viaggiatori — e la piattaforma restituisce un itinerario ricco di attività, costi stimati e link affiliati per hotel, tour e voli.

---

## 🏗️ Tech Stack

| Layer        | Technology                       |
| ------------ | -------------------------------- |
| **Frontend** | Next.js 14 · React · TypeScript  |
| **Backend**  | .NET 8 · C# · ASP.NET Core      |
| **Engine** | Python 3.12 · FastAPI · Gemini   |
| **Database** | PostgreSQL 16 · JSONB            |
| **Infra**    | Docker Compose · Dev Containers  |

---

## 📋 Prerequisites

| Tool                    | Version   |
| ----------------------- | --------- |
| **Docker Desktop**      | ≥ 4.x    |
| **VS Code**             | Latest    |
| **Dev Containers ext.** | Latest    |

> _All language runtimes (Node, .NET, Python) are provided inside the containers — no local installs required._

---

## 🚀 Quick Start

### Option A — VS Code Dev Container (Recommended)

```bash
# 1. Clone the repo
git clone https://github.com/your-username/AI-TRAVELS.git
cd AI-TRAVELS

# 2. Create your local env file
cp .env.example .env
# Edit .env and set your GEMINI_API_KEY if you want real Gemini generation
# To use demo itinerary data without Gemini, set USE_DEMO_DATA=true

# 3. Open in VS Code → Reopen in Container
code .
# Press F1 → "Dev Containers: Reopen in Container"
```

### Option B — Docker Compose (CLI)

```bash
# 1. Clone & configure
git clone https://github.com/your-username/AI-TRAVELS.git
cd AI-TRAVELS
cp .env.example .env

# 2. Spin up all services
docker compose up -d

# 3. Access the services
#    Frontend  → http://localhost:3000
#    Backend   → http://localhost:5000
#    AI Engine → http://localhost:8000
#    Database  → localhost:5433
```

---

## 🌐 Ports

| Port   | Service           | Description                     |
| ------ | ----------------- | ------------------------------- |
| `3000` | Frontend          | Next.js dev server              |
| `5000` | Backend API       | .NET 8 HTTP                     |
| `5001` | Backend API       | .NET 8 HTTPS                    |
| `8000` | AI Engine         | FastAPI (Gemini integration)    |
| `5433` | PostgreSQL        | Database (mapped from 5432)     |

---

## 📁 Project Structure

```
AI-TRAVELS/
├── .devcontainer/
│   └── devcontainer.json       # Dev Container configuration
├── db/
│   └── init.sql                # Database schema & seed data
├── frontend/                   # Next.js application (coming soon)
├── backend-api/                # .NET 8 Web API (coming soon)
├── ai-engine/                  # Python FastAPI service (coming soon)
├── docker-compose.yml          # Multi-service orchestration
├── .env.example                # Environment variable template
├── .gitignore
└── README.md
```

---

## 🛠️ Development Commands

### Docker Compose

```bash
# Start all services
docker compose up -d

# Stop all services
docker compose down

# Rebuild & start
docker compose up -d --build

# View logs
docker compose logs -f [service-name]

# Reset database (destroy volume)
docker compose down -v
docker compose up -d
```

### Frontend (inside container)

```bash
cd /workspace/frontend
npm install
npm run dev
```

### Backend API (inside container)

```bash
cd /workspace/backend-api
dotnet restore
dotnet run
```

### AI Engine (inside container)

```bash
cd /workspace/ai-engine
pip install -r requirements.txt
uvicorn main:app --host 0.0.0.0 --port 8000 --reload
```

### Database

```bash
# Connect via psql (inside db container)
docker compose exec db psql -U aitravels -d aitravels

# Connect from host
psql -h localhost -p 5433 -U aitravels -d aitravels
```

---

## 🔑 Environment Variables

| Variable                     | Description                        | Default             |
| ---------------------------- | ---------------------------------- | ------------------- |
| `POSTGRES_USER`              | Database user                      | `aitravels`         |
| `POSTGRES_PASSWORD`          | Database password                  | `devpassword`       |
| `POSTGRES_DB`                | Database name                      | `aitravels`         |
| `GEMINI_API_KEY`             | Google Gemini API key              | —                   |
| `GEMINI_MODEL`               | Gemini model name                  | `gemini-2.0-flash`  |
| `AFFILIATE_BOOKING_AID`     | Booking.com affiliate ID           | —                   |
| `AFFILIATE_GYG_PARTNER_ID`  | GetYourGuide partner ID            | —                   |
| `AFFILIATE_AVIASALES_MARKER`| Aviasales marker                   | —                   |
| `APP_ENV`                    | Application environment            | `development`       |
| `LOG_LEVEL`                  | Logging verbosity                  | `debug`             |

---

## 📄 License

This project is proprietary. All rights reserved.
