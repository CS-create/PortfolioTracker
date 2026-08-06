# PortfolioTracker

A full-stack stock portfolio tracker built with C# and ASP.NET Core, using Clean Architecture principles. Users can register, create portfolios, search for real stocks, record buy/sell transactions, and see live-calculated market value and gain/loss based on real-time prices.

Built as a personal project to demonstrate backend architecture, authentication, external API integration, and containerized deployment.

## Features

- **JWT authentication** — register/login/logout, with every portfolio endpoint scoped to the authenticated user's own data
- **Live stock search** — search by company name or ticker via the Alpha Vantage API
- **Portfolio management** — create portfolios, add holdings, record buy/sell transactions
- **Automatic financial calculations** — average cost basis, market value, and unrealized gain/loss computed from transaction history and live prices
- **Price caching** — a background service periodically refreshes and caches prices to avoid excessive external API calls
- **Web frontend** — session-based ASP.NET Core MVC app; the JWT is held server-side and never exposed to the browser
- **24 automated unit tests** covering domain calculations, service logic, and authorization rules
- **Dockerized** — the full stack (API, Web, PostgreSQL) runs with a single `docker-compose up`
- **CI** — GitHub Actions builds and runs the full test suite on every push

## Architecture

The backend follows Clean Architecture, split across four projects with dependencies pointing inward:

```
API  →  Application  →  Domain
Infrastructure  →  Application  (via interfaces)
```

- **Domain** — core entities (`Portfolio`, `Holding`, `Transaction`, `User`, `PriceSnapshot`) and business logic (e.g. cost-basis calculation), with no external dependencies
- **Application** — service interfaces, DTOs, and orchestration logic (e.g. `PortfolioService`, `AuthService`)
- **Infrastructure** — EF Core + PostgreSQL implementation, JWT generation, and the Alpha Vantage API client
- **API** — ASP.NET Core Web API exposing REST endpoints, documented with Swagger
- **Web** — ASP.NET Core MVC frontend that calls the API server-to-server

```
PortfolioTracker/
├── Domain/            # Entities and core business rules
├── Application/        # Interfaces, DTOs, services
├── Infrastructure/      # EF Core, repositories, external API clients
├── API/                 # REST API (JWT-protected)
├── Web/                 # MVC frontend (session-based auth)
├── Tests/                # xUnit + Moq unit tests
└── docker-compose.yml
```

## Tech Stack

| Layer | Technology |
|---|---|
| Language | C# / .NET 8 |
| API | ASP.NET Core Web API |
| Frontend | ASP.NET Core MVC (Razor) |
| Database | PostgreSQL + Entity Framework Core |
| Auth | JWT Bearer tokens |
| Testing | xUnit, Moq |
| External API | Alpha Vantage (stock search & pricing) |
| Containerization | Docker, Docker Compose |
| CI | GitHub Actions |

## Running locally with Docker (recommended)

**Prerequisites:** Docker Desktop, an [Alpha Vantage API key](https://www.alphavantage.co/support/#api-key) (free)

1. Clone the repo and create a `.env` file in the project root:
   ```
   POSTGRES_PASSWORD=your-password-here
   JWT_KEY=a-random-32-plus-character-string
   ALPHA_VANTAGE_API_KEY=your-key-here
   ```

2. Start everything:
   ```bash
   docker-compose up --build
   ```

3. On first run only, apply the database migrations (the containers don't run migrations automatically on startup):
   ```bash
   dotnet ef database update --project Infrastructure --startup-project API
   ```

4. Open the app:
   - Web frontend: http://localhost:5171
   - API + Swagger: http://localhost:5130/swagger

## Running locally without Docker

**Prerequisites:** .NET 8 SDK, PostgreSQL running locally

1. Update the connection string and secrets in `API/appsettings.json`, or set them via `dotnet user-secrets`
2. Apply migrations:
   ```bash
   dotnet ef database update --project Infrastructure --startup-project API
   ```
3. Run both projects in separate terminals:
   ```bash
   dotnet run --project API
   dotnet run --project Web
   ```

## Running the tests

```bash
dotnet test
```

24 tests covering:
- Domain-level financial calculations (`Holding.GetTotalQuantity`, `Holding.GetAverageCostBasis`)
- `PortfolioService` — CRUD operations and ownership/authorization checks
- `AuthService` — registration, password hashing, login, and rejection of invalid credentials

## Known limitations

This is a portfolio/demo project, not a production system. A few deliberate simplifications:
- No refresh tokens — JWTs expire after 60 minutes and require re-login
- Alpha Vantage's free tier has a low daily rate limit, which can affect live price fetching during heavy testing
- Database migrations must be applied manually after the first `docker-compose up` (not automated into container startup)
- No rate limiting or bot protection on the API

## Author

Oliver Fasdal
[GitHub](https://github.com/CS-create) · [LinkedIn](https://www.linkedin.com/in/oliver-fasdal-964bbb130)
