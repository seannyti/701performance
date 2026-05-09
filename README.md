# PerformancePower

Public marketing site, staff portal, and API for a powersports dealer. Visitors browse inventory and submit contact-form inquiries; staff manage inventory and site settings from the portal.

## Stack

| Layer | Technology |
|---|---|
| API | .NET 10 / ASP.NET Core |
| ORM | EF Core 9 + Pomelo MySQL |
| Auth | JWT Bearer + BCrypt + TOTP MFA |
| Database | MySQL 8 |
| Public site | Vue 3 + Vite (TypeScript) |
| Staff portal | Vue 3 + PrimeVue + Pinia |
| Reverse proxy | Nginx |
| Containers | Docker Compose |

## Running

### Prerequisites

- Docker Desktop
- A `.env` file at the repo root (copy `.env.example` and fill in values)

### Start everything (local dev — HTTP only)

```bash
docker compose up --build -d
```

### Production deploy (HTTPS)

On a server with Let's Encrypt certs at `/etc/letsencrypt/live/<domain>/`:

```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d
```

Override the cert path for a different domain via `CERT_DIR=/etc/letsencrypt/live/yourdomain.com` in `.env`.

| Service | URL |
|---|---|
| Public site | https://yourdomain.com |
| Staff portal | https://portal.yourdomain.com |
| API | https://api.yourdomain.com |
| Swagger UI | https://api.yourdomain.com/swagger (Development only) |

The API applies EF Core migrations and seeds an admin account on first startup. **Check the API container logs for the one-time admin password:**

```bash
docker logs performancepower-api-1 2>&1 | grep "ADMIN SEEDED"
```

### Required environment variables

See `.env.example`. The API also reads `Admin__SeedPassword` if you want to set the initial admin password explicitly instead of using the auto-generated one.

### Run tests

```bash
# API integration tests (xUnit + SQLite in-memory)
cd src/api.tests && dotnet test

# Portal unit tests
cd src/portal && npm test

# Public unit tests
cd src/public && npm test
```

## Project Structure

```
src/
├── api/                        # ASP.NET Core API
│   ├── Controllers/            # Auth, Inventory, Settings, Contact
│   ├── Data/                   # EF Core DbContext
│   ├── DTOs/                   # Request/response records
│   ├── Helpers/                # JWT, claims extensions
│   ├── Middleware/             # Global exception handler
│   ├── Migrations/             # EF Core migrations
│   ├── Models/                 # Auth, Vehicle, Settings entities
│   ├── Services/               # AuthService, EmailService
│   └── Validators/             # FluentValidation validators
├── api.tests/                  # xUnit integration tests
├── portal/                     # Staff portal (Vue 3 + PrimeVue)
│   └── src/
│       ├── components/
│       ├── services/           # Typed API service clients
│       ├── stores/             # Pinia stores (auth, settings)
│       └── views/              # Inventory CRUD + Site settings
└── public/                     # Public marketing site (Vue 3)
    └── src/
        ├── components/
        ├── services/
        ├── stores/
        └── views/              # Home, Inventory, Vehicle detail, Finance, About, Contact
```

## API Documentation

Swagger UI is available at `/swagger` in Development. All write endpoints require an admin or superadmin JWT — click **Authorize** in the Swagger UI and paste your access token.

## Settings

Site settings (contact info, hours, SMTP, theme, SEO, hero, finance lenders, TOS, privacy policy) are managed from the staff portal at `/settings`. The SMTP password is excluded from the public `GET /api/settings` endpoint and only returned to authenticated requests via `GET /api/settings/secure`.

## Security Notes

- Refresh tokens are stored in HttpOnly cookies and rotated on every use
- The auth login endpoint is rate-limited to 10 requests/minute per IP
- Admin seed password is randomly generated and logged once at startup — change it immediately
- TOTP MFA is opt-in per staff user (`/security/mfa` in the portal)
