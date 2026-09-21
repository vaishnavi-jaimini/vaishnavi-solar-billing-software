# Vaishnavi Solar — Billing Software

A GST-compliant billing, inventory, and quotation system for Vaishnavi Solar, a solar equipment retailer.

## Stack

- **Backend**: ASP.NET Core 8 Web API + Entity Framework Core, SQL Server LocalDB
- **Frontend**: Angular 18 (standalone components) + Angular Material

## Project Structure

```
backend/VaishnaviSolar.Api/   ASP.NET Core Web API
frontend/vaishnavi-solar-app/ Angular app
```

## Running the app

### 1. Backend (API)

```
cd backend/VaishnaviSolar.Api
dotnet run
```

Runs at `http://localhost:5215`. Swagger UI at `http://localhost:5215/swagger`.

The database (`VaishnaviSolarDb` on `(localdb)\MSSQLLocalDB`) is created and migrated automatically on startup, seeded with company defaults and a standard set of solar items (panels, inverters, batteries, structure, cables, BOS, services).

### 2. Frontend (Angular)

```
cd frontend/vaishnavi-solar-app
npm install   # first time only
ng serve
```

Runs at `http://localhost:4200`. The frontend expects the API at `http://localhost:5215/api` (see `src/environments/environment.ts`).

Run both at the same time for the app to work.

## First-time setup

1. Open **Settings** and fill in your business details (GSTIN, address, bank details) — these appear on every printed invoice.
2. Add real stock quantities from **Items → adjust stock icon** (seeded items start at 0 stock).
3. Add customers, then create quotations or invoices directly.

## Features

- Customers, Items (with solar-specific categories/units), stock ledger
- Quotations → convert to Invoice
- Proforma Invoices (own numbering, no stock/payment impact) → convert to Invoice
- GST tax invoices with CGST/SGST/IGST breakdown based on customer state vs company state, printable invoice with company logo, declaration, and bank details
- Payments against invoices (partial payments supported)
- Reports: Sales, GST Summary (by HSN/SAC), Outstanding Dues, Stock
- Dashboard with monthly sales, outstanding dues, low-stock count, and a sales trend chart

## Deploying (Render.com, free tier)

This repo includes a [`render.yaml`](./render.yaml) blueprint that provisions all three pieces in one go:
a free PostgreSQL database, the backend API (built from [`backend/Dockerfile`](./backend/Dockerfile)), and
the Angular app as a static site.

1. Push this repo to GitHub (already done if you're reading this on GitHub).
2. Go to the [Render Dashboard](https://dashboard.render.com/), sign in with GitHub (no credit card required
   for the free tier), and choose **New → Blueprint**.
3. Select this repository. Render reads `render.yaml` and creates all three services automatically.
4. Wait for the backend (`vaishnavi-solar-api`) and frontend (`vaishnavi-solar-app`) builds to finish —
   the backend builds a Docker image, so it takes a few minutes on the first deploy.
5. Open the frontend service's URL (something like `https://vaishnavi-solar-app.onrender.com`, or whatever
   Render assigned if that exact name was taken) — that's your live app.

**Locally**, the app keeps using SQL Server LocalDB as before — nothing changes for local development.
In production, the backend detects Render's `DATABASE_URL` env var and switches to PostgreSQL automatically
(see `Program.cs`), building its schema fresh on first boot (no PostgreSQL migrations are checked in; SQL
Server migrations under `Migrations/` are unaffected).

**Free-tier caveats to know about:**
- The free web service spins down after ~15 minutes of inactivity — the first request after that can take
  20–30 seconds to wake back up.
- The free PostgreSQL database expires 90 days after creation (Render will email a warning); recreate it
  from the blueprint to keep the demo alive longer, or upgrade to a paid plan for production use.
