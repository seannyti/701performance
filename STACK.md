# M Performance Power — Stack Reference

## Projects

| Project | Port (local) | Description |
|---------|-------------|-------------|
| `mperformancepower-public` | 5173 | Customer-facing Vue 3 site |
| `mperformancepower-admin` | 5174 | Admin panel Vue 3 app |
| `mperformancepower.Api` | 5000 | ASP.NET Core 8 REST API |

## Tech Stack

**Frontend — Public** (`mperformancepower-public/`)
- Vue 3 + Vite + TypeScript
- SCSS, Pinia, Axios, SignalR, DOMPurify

**Frontend — Admin** (`mperformancepower-admin/`)
- Vue 3 + Vite + TypeScript
- PrimeVue 4 (Aura theme), PrimeIcons, VueUse, SignalR

**Backend** (`mperformancepower.Api/`)
- ASP.NET Core 8
- EF Core 8 + MySQL (Pomelo driver)
- JWT auth, BCrypt, MailKit, ImageSharp, SignalR

**Infrastructure**
- MySQL 8 (Linode)
- nginx reverse proxy
- Docker Compose (3 app containers + db + nginx)

## Local Development

```bash
# Start everything via Docker
docker compose -f docker-compose.yml -f docker-compose.local.yml up

# Or run individually
cd mperformancepower.Api && dotnet run
cd mperformancepower-public && npm run dev
cd mperformancepower-admin && npm run dev
```

## Default Admin Credentials
- Email: `admin@mperformancepower.com`
- Password: `Admin1234!`
- **Change this immediately after first login.**

## API Routes

| Method | Route | Auth | Purpose |
|--------|-------|------|---------|
| POST | /api/auth/login | — | Get JWT |
| GET | /api/vehicles | — | Paginated list |
| GET | /api/vehicles/{id} | — | Single vehicle |
| POST | /api/vehicles | Admin | Create |
| PUT | /api/vehicles/{id} | Admin | Update |
| DELETE | /api/vehicles/{id} | Admin | Delete |
| POST | /api/vehicles/{id}/images | Admin | Upload images |
| DELETE | /api/images/{id} | Admin | Delete image |
| PUT | /api/images/reorder | Admin | Reorder images |
| POST | /api/inquiries | — | Submit inquiry |
| GET | /api/inquiries | Admin | List inquiries |
| PUT | /api/inquiries/{id}/read | Admin | Mark read |

## SignalR
Hub: `/api/hubs/notifications`
Event: `NewInquiry` → broadcast to `Admins` group on new inquiry submission
