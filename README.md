# Powersports Showcase - Full-Stack Application

A modern full-stack web application showcasing powersports vehicles and gear, built with .NET 8 Web API backend and Vue.js 3 frontend.

================= PROJECT STRUCTURE =================

```
PowersportsShowcase/
├── backend/
│   └── PowersportsApi/          # .NET 8 Web API
│       ├── Models/              # Data models
│       ├── Services/            # Business logic services
│       ├── Middleware/          # Security, rate limiting, validation
│       ├── Migrations/          # EF Core database migrations
│       ├── Program.cs           # Main application entry point
│       └── PowersportsApi.csproj
├── frontend/                    # Vue.js 3 + TypeScript (Main Site)
│   ├── src/
│   │   ├── components/          # Reusable components (Header, Footer, ProductCard, etc.)
│   │   ├── composables/         # useSettings, useTheme, useParallax, etc.
│   │   ├── views/               # Page components (Home, About, Contact, Products, etc.)
│   │   ├── services/            # API services
│   │   ├── stores/              # Pinia auth store
│   │   ├── types/               # TypeScript interfaces
│   │   ├── utils/               # api-config.ts (VITE_API_URL helper)
│   │   └── router/              # Vue Router configuration
│   ├── .env.local               # Local dev settings (gitignored)
│   ├── .env.production          # Production env (committed)
│   └── vite.config.ts
├── admin/                       # Vue.js 3 + TypeScript (Admin Dashboard)
│   ├── src/
│   │   ├── components/          # AdminLayout, MediaPicker, BrandListManager, TeamMemberManager, FeatureListManager
│   │   ├── composables/         # useLoadingState, useToast
│   │   ├── views/               # Dashboard, Catalog, Orders, Users, Settings, MediaLibrary, Backup, Inquiries, Categories
│   │   ├── stores/              # Pinia auth store
│   │   ├── types/               # TypeScript interfaces
│   │   ├── utils/               # apiClient.ts, api-config.ts
│   │   └── router/              # Admin routing with auth guards
│   ├── .env.local               # Local dev settings (gitignored)
│   ├── .env.production          # Production env (committed)
│   └── vite.config.ts
└── packages/
    └── shared-types/            # Shared TypeScript types (Product, User, AuthResponse, etc.)
```

================= TECHNOLOGY STACK =================

### Backend (.NET 8 Web API)
- **Framework**: .NET 8 with minimal APIs
- **Database**: Entity Framework Core with MySQL (production) or SQL Server (dev)
- **Auth**: JWT Bearer tokens with refresh token support
- **Middleware**: Rate limiting, request validation, security headers
- **Documentation**: Swagger/OpenAPI
- **File Storage**: Local disk (`wwwroot/uploads/`) with thumbnail generation

### Frontend & Admin (Vue.js 3 + TypeScript)
- **Framework**: Vue.js 3 with Composition API
- **State Management**: Pinia
- **Routing**: Vue Router 4 with auth guards
- **HTTP**: Axios (frontend) + centralized apiClient (admin)
- **Build Tool**: Vite
- **Styling**: Custom CSS using CSS custom properties (theme-driven)

================= ENVIRONMENT CONFIGURATION =================

### Development (local)
Create `.env.local` in both `frontend/` and `admin/` (these are gitignored):
```env
VITE_API_URL=http://localhost:5226
```

### Production
`frontend/.env.production` and `admin/.env.production` (committed):
```env
VITE_API_URL=http://23.239.26.52:5226
```

### Backend
- `appsettings.json` — default (dev) settings
- `appsettings.Development.json` — dev overrides
- `appsettings.Production.json` — production MySQL + CORS for Linode server

### API URL Helper (`src/utils/api-config.ts`)
Both frontend and admin have a shared utility:
```typescript
const BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5226';
export const API_URL = `${BASE_URL}/api/v1`;
export const getMediaUrl = (path: string): string => {
  if (!path) return '';
  return path.startsWith('http') ? path : `${BASE_URL}${path}`;
};
```
All views/components import from here — **no hardcoded localhost URLs** remain.

================= GETTING STARTED =================

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- MySQL (production) or SQL Server Express (dev)

### Quick Start

1. **Backend**
   ```bash
   cd backend/PowersportsApi
   dotnet restore
   dotnet ef database update
   dotnet run
   # API: http://localhost:5226
   # Swagger: http://localhost:5226/swagger
   ```

2. **Frontend**
   ```bash
   cd frontend
   npm install
   npm run dev
   # http://localhost:5173
   ```

3. **Admin**
   ```bash
   cd admin
   npm install
   npm run dev
   # http://localhost:5174
   ```

Or use the startup scripts:
```powershell
.\start.ps1        # Start all three
start.bat          # Windows batch alternative
```

================= API ENDPOINTS =================

**Public:**
- `GET /api/v1/products` — All products
- `GET /api/v1/products/{id}` — Product by ID
- `GET /api/v1/products/featured` — Featured products
- `GET /api/v1/categories` — All categories
- `GET /api/v1/settings` — Public site settings
- `GET /health` — Health check

**Auth:**
- `POST /api/v1/auth/register` — Registration
- `POST /api/v1/auth/login` — Login (returns JWT + refresh token)
- `POST /api/v1/auth/refresh` — Refresh access token
- `POST /api/v1/auth/logout` — Logout

**Admin (JWT required):**
- **Products**: Full CRUD `/api/v1/admin/products`
- **Categories**: Full CRUD `/api/v1/admin/categories`
- **Users**: Manage accounts `/api/v1/admin/users` (SuperAdmin only)
- **Orders**: Create/manage `/api/v1/admin/orders`
- **Inquiries**: Contact submissions `/api/v1/admin/contact-submissions`
- **Settings**: Dynamic config `/api/v1/admin/settings`
- **Media**: Upload/manage `/api/v1/admin/media`
- **Media Sections**: Organize media `/api/v1/admin/media/sections`
- **Backup**: DB backup/restore `/api/v1/admin/backup`

================= MEDIA LIBRARY SYSTEM =================

### Architecture
All media is centrally stored in `wwwroot/uploads/media/` with section-based subfolders. Each file is linked to entities via junction tables (`ProductImages`, `CategoryImages`).

**Section types:**
- **System Sections**: Auto-created (Products, Settings)
- **Category Sections**: Auto-created when a new category is added
- **Custom Sections**: Manually created (Promotions, Events, etc.)

### Upload Flow
```
POST /api/v1/admin/media/upload
  └─ file, sectionId, altText, caption, tags
  └─ Stored as: /uploads/media/SectionName/filename_guid.jpg
  └─ Thumbnail auto-generated at 300px
```

### Cascade Deletion
Deleting a category automatically:
1. Finds its linked media section
2. Deletes all MediaFile records
3. Removes physical files + thumbnails from disk
4. Removes section folder
5. Deletes MediaSection and Category records

### Database Schema

**MediaSection:** Id, Name, Description, DisplayOrder, IsSystem, IsActive, CategoryId, CreatedAt  
**MediaFile:** Id, FileName, StoredFileName, FilePath, ThumbnailPath, MimeType, FileSize, MediaType, Width, Height, AltText, Caption, Tags, SectionId, UploadedByUserId, UploadedAt  
**Junction tables:** ProductImages, CategoryImages

### File Storage
```
wwwroot/uploads/media/
├── ATVs/              # Auto-created for category
├── Dirtbikes/
├── Products/          # System section
├── Promotions/        # Custom section
└── General/           # Default
```

### Security
- Extension + MIME type + magic number validation
- Sanitized filenames (path traversal prevention)
- Role-based access (Admin+ only)
- Configurable max file size

**Config (`appsettings.json`):**
```json
"FileUploadSettings": {
  "MaxFileSizeMB": 10,
  "MaxImageWidth": 2000,
  "ThumbnailSize": 300,
  "ImageQuality": 85
}
```

================= THEME SYSTEM =================

### Overview
The admin Settings panel controls 70+ theme variables that apply to the frontend in real-time via CSS custom properties.

### How It Works
1. Admin saves theme settings to database
2. `useTheme.ts` composable fetches settings from `/api/v1/settings`
3. Settings are applied as CSS variables to `:root`
4. Auto-refreshes every 30 seconds

### Theme Categories
| Category | Settings |
|----------|----------|
| Colors | Primary, Secondary, Accent, Success, Warning, Danger, backgrounds, text |
| Typography | Font families, sizes (base, H1–H3), weights, line height, letter spacing |
| Buttons | Radius, padding, font weight, transform, style preset (Solid/Outlined/3D/Ghost) |
| Cards | Radius, padding, shadow |
| Layout | Container max-width, padding, section spacing, element gaps |
| Effects | Transition duration/timing, hover lift/scale/shadow |
| Header/Footer | Background, text colors, heights, sticky, shadow |
| Gradients | Start/end colors, direction, opacity, backdrop blur |
| Visual FX | Corner styles, image hover effects, background patterns, text effects |
| Advanced | Dark mode, smooth scrolling, parallax, glassmorphism, custom CSS injection |

### Example CSS Variable Usage
```css
/* Theme-aware (correct) */
.btn { background: var(--color-primary, #6366f1); border-radius: var(--button-radius, 8px); }

/* Hardcoded (avoid) */
.btn { background: #6366f1; border-radius: 8px; }
```

================= LOADING STATES SYSTEM =================

### Overview
All 9 admin views use the `useLoadingState` composable for consistent UX — full-page overlay on initial load, per-button spinners on actions, double-click prevention.

### Usage
```typescript
import { useLoadingState } from '@/composables/useLoadingState'

const { isLoading, executeWithLoading, isActionLoading } = useLoadingState()

// Initial data load
const loadData = async () => {
  await executeWithLoading(async () => {
    const data = await apiGet('/items')
    items.value = data
  })
}

// Action with item-specific tracking
const deleteItem = async (id: number) => {
  await executeWithLoading(async () => {
    await apiDelete(`/items/${id}`)
    await loadData()
  }, id)  // Pass ID to track which button is loading
}
```

### Template Pattern
```vue
<div class="card loading-container">
  <div v-if="isLoading" class="loading-overlay">
    <div class="spinner"></div>
    <p>Loading...</p>
  </div>
  <div v-else>
    <button @click="deleteItem(item.id)" :disabled="isActionLoading(item.id)" class="btn btn-danger">
      <span v-if="isActionLoading(item.id)" class="btn-spinner"></span>
      Delete
    </button>
  </div>
</div>
```

### API Reference
| Property | Type | Description |
|----------|------|-------------|
| `isLoading` | `Ref<boolean>` | Global page-level loading flag |
| `executeWithLoading(fn, id?)` | Function | Wraps async function with loading state |
| `isActionLoading(id)` | Function | True if specific item/action is loading |
| `hasActiveAction()` | Function | True if any action is in progress |
| `resetLoading()` | Function | Reset all states |

### CSS Classes
- `.spinner` — 48px spinner
- `.spinner-sm` — 24px
- `.spinner-lg` — 64px
- `.btn-spinner` — 12px inline button spinner
- `.loading-overlay` — Full-card overlay with fade-in (150ms delay to prevent flicker)
- `.skeleton`, `.skeleton-text`, `.skeleton-card` — Skeleton loaders

### Views with Loading States
All 9 admin views are fully implemented: Calendar, Dashboard, Orders, Inquiries, Users, MediaLibrary, Settings, Backup, Catalog, Categories

================= AUTHENTICATION & SESSION MANAGEMENT =================

### Auth Flow
1. User logs in → backend returns JWT access token (480min) + refresh token (7 days)
2. Token stored in `localStorage` as `admin_token`
3. `apiClient.ts` auto-attaches `Authorization: Bearer <token>` to every request
4. On 401 response → user logged out, redirected to login with "Session expired" message

### apiClient Utility (`admin/src/utils/apiClient.ts`)
Centralized API client used by all admin views:
```typescript
import { apiGet, apiPost, apiPut, apiDelete } from '@/utils/apiClient'

// Clean usage — no manual auth headers needed
const data = await apiGet('/admin/products')
const result = await apiPost('/admin/orders', orderPayload)
await apiPut(`/admin/orders/${id}`, updatePayload)
await apiDelete(`/admin/media/${id}`)
```

### JWT Settings (backend `appsettings.json`)
```json
{
  "AccessTokenExpiryMinutes": "480",
  "RefreshTokenExpiryDays": "7"
}
```
**Production recommendation:** Reduce to 60–120 minutes for better security.

================= PRODUCTION SETUP =================

### Server Details
- **Host**: Linode VPS — `23.239.26.52`
- **Database**: MySQL on Akamai Cloud — `a428036-akamai-prod-4078154-default.g2a.akamaidb.net:15921`
- **DB Name**: `defaultdb`, **User**: `akmadmin`

### Pre-Launch Checklist
- [x] Hardcoded localhost URLs replaced with `VITE_API_URL` env variable
- [x] `admin/.env.production` and `frontend/.env.production` configured
- [x] `appsettings.Production.json` with MySQL connection string
- [x] CORS updated for `23.239.26.52` in `appsettings.json`
- [ ] **Generate strong JWT secret** (64+ char random string) in `appsettings.Production.json`
- [ ] Run migrations on production MySQL: `dotnet ef database update`
- [ ] Create production admin account, set role to SuperAdmin
- [ ] Delete all test products, test users, test orders/inquiries
- [ ] Build frontend: `npm run build` in both `frontend/` and `admin/`
- [ ] Publish backend: `dotnet publish -c Release`
- [ ] Test full flow on production (login, CRUD, media upload, contact form)
- [ ] Configure HTTPS / SSL certificate (Let's Encrypt recommended)

### Generate JWT Secret (PowerShell)
```powershell
-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 64 | ForEach-Object {[char]$_})
```

### Build Commands
```powershell
# Frontend
cd frontend ; npm run build   # Output → frontend/dist

# Admin
cd admin ; npm run build      # Output → admin/dist

# Backend
cd backend/PowersportsApi
dotnet publish -c Release     # Output → bin/Release/net8.0/publish
```

### Data Strategy for Launch
**Recommended: Fresh Start**
1. Run migrations on production DB (auto-seeds 5 categories + 150+ settings)
2. Create real admin account via API, manually set role to SuperAdmin
3. Add real products through admin panel
4. Customize settings (theme, contact info, logo, etc.) through admin panel

**Clean up test data (if migrating existing DB):**
```sql
DELETE FROM Products WHERE Name LIKE '%test%';
DELETE FROM Users WHERE Email LIKE '%test%';
DELETE FROM Orders WHERE CreatedAt < '2026-03-01';
DELETE FROM ContactSubmissions WHERE CreatedAt < '2026-03-01';
```

### Environment Variables (Server-Side, More Secure)
```powershell
# Windows Server
setx JWT_SECRET "your-64-char-secret"
setx DB_CONNECTION_STRING "Server=...;Database=...;"

# Linux
export JWT_SECRET="your-64-char-secret"
export ASPNETCORE_ENVIRONMENT="Production"
```

### When a Domain Is Added
Update these files with the real domain:
- `admin/.env.production` → `VITE_API_URL=https://api.yourdomain.com`
- `frontend/.env.production` → same
- `appsettings.Production.json` → `AllowedHosts`, `CorsOrigins`
- Add SSL certificate for HTTPS

================= SHARED TYPES PACKAGE =================

Located at `packages/shared-types/index.ts` — shared TypeScript definitions used by both frontend and admin.

**Included types:** `Product`, `ProductCategory`, `Category`, `User`, `UserRole`, `AuthResponse`, `ApiResponse<T>`, `ApiError`, `ContactForm`, `LoginRequest`, `RegisterRequest`

```typescript
import { Product, User, AuthResponse, UserRole } from '@powersports/shared-types';
```

```bash
cd packages/shared-types
npm run typecheck
```

================= CODEBASE AUDIT NOTES (March 14, 2026) =================

### Completed Fixes
- ✅ All 84 hardcoded `localhost:5226` URLs replaced with `import.meta.env.VITE_API_URL`
- ✅ `api-config.ts` utility created in both `frontend/src/utils/` and `admin/src/utils/`
- ✅ `apiClient.ts` centralized auth handler with 401 auto-logout
- ✅ All 9 admin views have loading states via `useLoadingState` composable
- ✅ `.env.local` / `.env.production` environment files configured
- ✅ Production database (MySQL/Akamai) connection string configured
- ✅ CORS updated for production server IP

### Known Incomplete Features (Post-Launch)
- ⚠️ **2FA** — Setting exists in admin UI, backend/frontend logic not implemented
- ⚠️ **Guest Checkout** — Setting exists, no checkout flow implemented
- ⚠️ **Payment Gateway** — Orders support manual payment tracking; Stripe/PayPal not integrated
- ⚠️ **Email SMTP** — Verification and contact form emails need real SMTP config + testing
- ⚠️ **Old Products.vue** — Deprecated; `Catalog.vue` is the full implementation

### Production Readiness
- **Hardcoded URLs**: ✅ Fixed
- **Environment config**: ✅ Configured
- **JWT Secret**: ⚠️ Must generate before deploy
- **Database migrations**: ⚠️ Must run on production MySQL
- **Test data cleanup**: ⚠️ Delete before launch
- **HTTPS/SSL**: ⚠️ Configure after deployment

### Hidden Gotchas
1. Admin dashboard link from frontend uses token passthrough — verify URL in `Header.vue`
2. Image fallbacks use Unsplash URLs (fine for now, not ideal long-term)
3. Backend seed data has `site_url = http://localhost:3000` — update in settings after launch
4. `admin/src/views/Products.vue` — shows "coming soon" stub; real product management is in `Catalog.vue`

================= FUTURE ENHANCEMENTS =================

### Short-Term (Post-Launch)
- [ ] Generate and configure production JWT secret
- [ ] Configure SMTP for real email delivery
- [ ] Set up automated database backups
- [ ] Add global error boundary component
- [ ] Integrate error monitoring (Sentry)
- [ ] Add `/health` endpoint for uptime monitoring
- [ ] Reduce JWT token expiry to 120 minutes

### Medium-Term
- [ ] Stripe/PayPal payment gateway integration
- [ ] Two-factor authentication (2FA)
- [ ] Guest checkout flow
- [ ] Shopping cart functionality
- [ ] Product detail pages with image galleries
- [ ] Advanced search with filters
- [ ] User reviews and ratings
- [ ] Order history and tracking for customers
- [ ] Wishlist functionality

### Long-Term
- [ ] CDN for media/images
- [ ] SEO optimization (meta tags, sitemap, structured data)
- [ ] Multi-language support
- [ ] Mobile app
- [ ] Analytics dashboard (user behavior tracking)
- [ ] Performance optimization (code splitting, lazy loading)

================= BROWSER SUPPORT =================

- Chrome/Edge 90+
- Firefox 88+
- Safari 14+
- iOS Safari, Chrome Mobile

---

**Built with .NET 8 and Vue.js 3**


```
PowersportsShowcase/
├── backend/
│   └── PowersportsApi/          # .NET 8 Web API
│       ├── Models/              # Product models
│       ├── Services/            # Business logic services
│       ├── Program.cs           # Main application entry point
│       └── PowersportsApi.csproj
├── frontend/                    # Vue.js 3 + TypeScript (Main Site)
│   ├── src/
│   │   ├── components/          # Reusable components
│   │   ├── views/               # Page components
│   │   ├── services/            # API services
│   │   ├── types/               # TypeScript interfaces
│   │   └── router/              # Vue Router configuration
│   ├── index.html
│   ├── package.json
│   └── vite.config.ts
└── admin/                       # Vue.js 3 + TypeScript (Admin Dashboard)
    ├── src/
    │   ├── components/          # Admin components
    │   ├── views/               # Admin pages
    │   ├── stores/              # Pinia stores
    │   ├── types/               # TypeScript interfaces
    │   └── router/              # Admin routing
    ├── index.html
    ├── package.json
    └── vite.config.ts
```

## 🛠️ Technology Stack

### Backend (.NET 8 Web API)
- **Framework**: .NET 8 with minimal APIs
- **Database**: Entity Framework Core with SQL Server or In-Memory fallback
- **Architecture**: Clean, service-oriented design
- **Documentation**: Swagger/OpenAPI integration
- **CORS**: Configured for frontend integration

### Frontend (Vue.js 3 + TypeScript)
- **Framework**: Vue.js 3 with Composition API
- **Language**: TypeScript for type safety
- **Routing**: Vue Router 4
- **HTTP Client**: Axios for API communication
- **Build Tool**: Vite for fast development
- **Styling**: Custom CSS with responsive design

### Admin Dashboard (Vue.js 3 + TypeScript)
- **Framework**: Vue.js 3 with Composition API and Pinia
- **Authentication**: Role-based access control (Admin/SuperAdmin)
- **Auto-Login**: Unified authentication flow from main site
- **Management**: Products, users, and dynamic site content
- **Permissions**: Different access levels for Admin vs SuperAdmin users

## ✨ Features

### Main Site
- 🏍️ **Product Showcase** - Browse powersports vehicles and gear by category
- 👤 **User Authentication** - Secure registration and login system
- 🔐 **Role-Based Access** - User/Admin/SuperAdmin role system
- 📱 **Responsive Design** - Optimized for desktop and mobile devices
- 🛒 **Product Categories** - ATV, Dirtbike, UTV, Snowmobile, and Gear
- 📞 **Contact Forms** - Get in touch functionality

### Admin Dashboard
- 📊 **Dashboard Analytics** - Overview of users, products, and activity
- 🛵 **Product Management** - Add, edit, and remove products with image upload
- 👥 **User Management** - Manage user accounts and roles (SuperAdmin only)
- 📁 **Media Library** - Centralized media management with section-based organization
- 🖼️ **Photo Uploader** - Drag-and-drop image uploads with metadata support
- ⚙️ **Dynamic Settings** - Edit site content without code changes
- 🔗 **Unified Login** - Seamless access from main site for admin users
- 🛡️ **Security** - Route guards and API endpoint protection
- 🗑️ **Cascade Deletion** - Automatic cleanup of related media when deleting entities

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (version 18 or later)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express edition works fine)
- [Git](https://git-scm.com/)

### Quick Start

#### Option 1: Automated Setup (Recommended)
1. **Run the setup script**
   ```powershell
   # PowerShell
   .\setup.ps1
   
   # Or skip database setup to use in-memory only
   .\setup.ps1 -SkipDatabase
   ```

2. **Start both services**
   ```batch
   # Windows Batch
   start.bat
   
   # Or PowerShell with options
   .\start.ps1                 # Uses SQL Server
   .\start.ps1 -UseInMemory    # Uses in-memory database
   ```

3. **Access the application**
   - Main Site: http://localhost:5173
   - Admin Dashboard: http://localhost:5174
   - Backend API: http://localhost:5000
   - API Docs: http://localhost:5000/swagger

#### Option 2: Manual Setup

1. **Clone the repository** (if applicable)
   ```bash
   git clone <repository-url>
   cd PowersportsShowcase
   ```

2. **Backend Setup**
   ```bash
   cd backend/PowersportsApi
   dotnet restore
   dotnet build
   
   # Optional: Update database (if using SQL Server)
   dotnet ef database update
   ```

3. **Frontend Setup**
   ```bash
   cd ../../frontend
   npm install
   ```

### Database Configuration

The application supports two database modes:

#### SQL Server (Default)
- **Server**: PATRICK\SQLEXPRESS (configurable in appsettings.json)
- **Database**: PowersportsShowcase (auto-created)
- **Authentication**: Integrated Security (Windows Authentication)
- **Fallback**: Automatically uses in-memory data if SQL Server is unavailable

#### In-Memory Database
- **Usage**: Fallback when SQL Server is not available
- **Data**: Uses hardcoded mock data
- **Benefits**: No database setup required, works immediately

### Running the Application

#### Using Startup Scripts (Recommended)
**Windows Batch File:**
```batch
start.bat
```

**PowerShell Script:**
```powershell
# Use SQL Server database
.\start.ps1

# Use in-memory database only
.\start.ps1 -UseInMemory

# Show help
.\start.ps1 -Help
```

#### Manual Startup
1. **Start the Backend API**
   ```bash
   cd backend/PowersportsApi
   dotnet run
   ```
   - API will be available at: `https://localhost:7081`
   - Swagger documentation: `https://localhost:7081/swagger`

2. **Start the Frontend** (in a new terminal)
   ```bash
   cd frontend
   npm run dev
   ```
   - Frontend will be available at: `http://localhost:3000`

## 🌟 Features

### Current Implementation
- ✅ **Product Showcase**: Display ATVs, dirt bikes, UTVs, snowmobiles, and gear
- ✅ **User Authentication**: JWT-based authentication with role-based access control
- ✅ **Role Management**: User, Admin, and SuperAdmin permission levels
- ✅ **Media Library**: Centralized media management with section-based organization
- ✅ **Image Upload**: Drag-and-drop uploads with automatic thumbnail generation
- ✅ **Cascade Deletion**: Automatic cleanup of media files when deleting entities
- ✅ **Admin Dashboard**: Complete management interface for products, users, and media
- ✅ **Responsive Design**: Mobile-first, responsive UI across all pages
- ✅ **Navigation**: Clean routing between Home, Products, About, Contact, and Admin
- ✅ **API Integration**: RESTful API with full CRUD capabilities
- ✅ **Category Filtering**: Filter products by category with dynamic sections
- ✅ **Contact Form**: Functional contact form with validation
- ✅ **Modern UI**: Professional, sleek design with smooth transitions
- ✅ **Database Integration**: Entity Framework Core with SQL Server and in-memory fallback

### API Endpoints

**Public Endpoints:**
- `GET /api/v1/products` - Get all products
- `GET /api/v1/products/{id}` - Get product by ID
- `GET /api/v1/products/category/{category}` - Get products by category
- `GET /api/v1/products/featured` - Get featured products for home page
- `GET /api/v1/categories` - Get all categories
- `GET /api/v1/health` - Health check endpoint

**Authentication:**
- `POST /api/v1/auth/register` - User registration
- `POST /api/v1/auth/login` - User login
- `POST /api/v1/auth/refresh` - Refresh access token
- `POST /api/v1/auth/logout` - User logout

**Admin Endpoints (Authentication Required):**
- **Products**: Full CRUD operations
- **Categories**: Create, update, delete with auto-section management
- **Users**: User management (SuperAdmin only)
- **Media Library**: File uploads, metadata editing, section management
- **Settings**: Dynamic site configuration

See **Media Library System** section above for detailed media endpoints.

### Frontend Features
- **Home Page**: Hero section with featured products
- **Products Page**: Full product catalog with category filtering
- **About Page**: Company information and team details
- **Contact Page**: Contact form and business information
- **Responsive Mobile Design**: Optimized for all device sizes

## 🧪 Testing & Development

### Backend Testing
```bash
cd backend/PowersportsApi
dotnet test                    # Run unit tests (when implemented)
dotnet run --watch            # Run with hot reload
```

### Frontend Testing
```bash
cd frontend
npm run build                 # Build for production
npm run preview              # Preview production build
npm run lint                 # Run linting (when configured)
```

### API Testing with Swagger
1. Start the backend API
2. Navigate to `https://localhost:7081/swagger`
3. Test all available endpoints

## 📁 Media Library System

The application features a comprehensive media library system with section-based organization and cascade deletion capabilities.

### Architecture Overview

**Unified Media Storage:**
- All media files are centrally managed in `wwwroot/uploads/media/`
- Files are organized into section-based subfolders (e.g., `Categories/`, `Products/`, `Promotions/`)
- Each file is linked to entities through junction tables (`ProductImages`, `CategoryImages`)
- Metadata support includes alt text, captions, tags, dimensions, and file size

**Section-Based Organization:**
- **System Sections**: Auto-created for core entities (Products, Settings)
- **Category Sections**: Auto-created when a new category is added
- **Custom Sections**: Manually created for flexible content organization (Promotions, Events, etc.)
- Each section maintains its own isolated subfolder on disk

### Key Features

#### 🗂️ Automatic Section Management
```csharp
// When creating a category, a media section is auto-created
POST /api/v1/categories
{
  "name": "ATVs",
  "description": "All-terrain vehicles"
}
// Automatically creates: MediaSection "ATVs" + wwwroot/uploads/media/ATVs/
```

#### 📤 Unified Upload System
```csharp
// Upload file to specific section
POST /api/v1/admin/media/upload
- file: [binary]
- sectionId: 3
- altText: "Product image"
- caption: "New ATV model"
- tags: "atv, featured, 2026"

// Files stored as: /uploads/media/ATVs/filename_guid.jpg
```

#### 🔗 Entity Linking
```csharp
// Upload and link in one step
POST /api/v1/photos/upload
- file: [binary]
- entityType: "product" | "category" | "setting"
- entityId: 5
- altText: "..."
- caption: "..."

// Automatically:
// 1. Uploads to media library
// 2. Creates MediaFile record
// 3. Links via ProductImages/CategoryImages table
```

#### 🗑️ Cascade Deletion
The system implements intelligent cascade deletion to maintain data integrity:

**When deleting a category:**
1. Finds linked media section by `CategoryId`
2. Deletes all MediaFile records in the section
3. Removes physical files (main images + thumbnails) from disk
4. Deletes the section's folder (`wwwroot/uploads/media/CategoryName/`)
5. Removes MediaSection database record
6. Finally deletes the Category itself

**Implementation:**
```csharp
// Reusable service method
public async Task<(bool Success, int DeletedFileCount, string? ErrorMessage)> 
    DeleteEntityMediaSectionAsync(int entityId, string entityType)

// Usage in endpoints:
var (success, deletedFileCount, errorMessage) = 
    await fileService.DeleteEntityMediaSectionAsync(categoryId, "Category");
```

**Extensibility:**
The cascade deletion system is built to support future entities. Simply add a new case to the switch statement:

```csharp
case "teammember":
    mediaSection = await _context.MediaSections
        .Include(s => s.MediaFiles)
        .FirstOrDefaultAsync(s => s.TeamMemberId == entityId);
    break;
```

### Database Schema

**MediaSection Table:**
| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| Name | string | Section name (e.g., "ATVs", "Promotions") |
| Description | string? | Optional description |
| DisplayOrder | int | Sort order in UI |
| IsSystem | bool | Prevents deletion of core sections |
| IsActive | bool | Enable/disable section |
| CategoryId | int? | Links to Category (if auto-created) |
| CreatedAt | DateTime | Creation timestamp |
| UpdatedAt | DateTime? | Last modification |

**MediaFile Table:**
| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| FileName | string | Original uploaded filename |
| StoredFileName | string | Unique filename on disk |
| FilePath | string | Relative path (e.g., /uploads/media/ATVs/file.jpg) |
| ThumbnailPath | string? | Thumbnail path for images |
| MimeType | string | File content type |
| FileSize | long | Size in bytes |
| MediaType | enum | Image, Video, Document, Other |
| Width | int? | Image width in pixels |
| Height | int? | Image height in pixels |
| AltText | string? | Accessibility description |
| Caption | string? | Display caption |
| Tags | string? | Comma-separated tags |
| SectionId | int? | Parent section |
| UploadedByUserId | int | User who uploaded |
| UploadedAt | DateTime | Upload timestamp |

**Junction Tables:**
- **ProductImages**: Links MediaFile → Product (many-to-many)
- **CategoryImages**: Links MediaFile → Category (one-to-one)

### API Endpoints

**Media Management:**
```
GET    /api/v1/admin/media                    - List all media files (with filtering)
GET    /api/v1/admin/media/{id}               - Get media file by ID
POST   /api/v1/admin/media/upload             - Upload file to media library
PUT    /api/v1/admin/media/{id}               - Update media metadata
DELETE /api/v1/admin/media/{id}               - Delete media file

GET    /api/v1/admin/media/sections           - List all sections
POST   /api/v1/admin/media/sections           - Create custom section
DELETE /api/v1/admin/media/sections/{id}      - Delete section + all files
```

**Unified Photo Upload:**
```
POST   /api/v1/photos/upload                  - Upload and link to entity
GET    /api/v1/photos/{entityType}/{entityId} - Get entity's photos
DELETE /api/v1/photos/{entityType}/{entityId}/{mediaFileId} - Unlink photo
```

### Frontend Integration

**Admin Media Library View:**
- Collapsible section cards with gradient headers
- Per-section upload buttons
- File grid with thumbnails
- Metadata editing panel
- Create/delete section modals
- Drag-and-drop upload support

**PhotoUploader Component:**
- Reusable across Products, Categories, Settings
- Automatic media library integration
- Entity linking on upload
- Visual file management interface

### Best Practices

1. **Use Sections for Organization**: Create logical sections for different content types
2. **Add Metadata**: Include alt text for accessibility and SEO
3. **Reuse Media**: Link the same MediaFile to multiple entities instead of duplicating
4. **Clean Naming**: Section names become folder names (alphanumeric + underscores)
5. **Cascade Awareness**: Deleting a category removes its entire media section

### File Storage Structure
```
wwwroot/
└── uploads/
    ├── media/                    # Unified media library
    │   ├── ATVs/                # Category: ATVs
    │   │   ├── atv123_abc.jpg
    │   │   └── atv123_abc_thumb.jpg
    │   ├── Dirtbikes/           # Category: Dirtbikes
    │   ├── Products/            # System: General products
    │   ├── Promotions/          # Custom: Marketing content
    │   └── General/             # Default: Uncategorized
    └── settings/                # Site settings & logos
```

### Configuration

**File Upload Settings (appsettings.json):**
```json
{
  "FileUploadSettings": {
    "MaxFileSizeMB": 10,
    "MaxImageWidth": 2000,
    "ThumbnailSize": 300,
    "ImageQuality": 85,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".webp"],
    "AllowedMimeTypes": ["image/jpeg", "image/png", "image/webp"]
  }
}
```

### Security Features

- **File Validation**: Extension and MIME type checking with magic number verification
- **Sanitized Filenames**: Path traversal prevention and special character removal
- **Role-Based Access**: Admin+ only for uploads, modifications, and deletions
- **Size Limits**: Configurable maximum file size per upload
- **Authentication Required**: All media endpoints require valid JWT token

## 🎯 Project Context & Goals

This application demonstrates:
- **Full-stack development** with modern .NET and Vue.js
- **RESTful API design** with proper HTTP semantics
- **Component-based frontend architecture**
- **Responsive web design** principles
- **Type-safe development** with TypeScript
- **Modern development tooling** (Vite, minimal APIs)

### Target Audience
- **Adventure enthusiasts** looking for powersports equipment
- **Dealers and retailers** seeking inventory display solutions
- **Developers** learning full-stack web development

## 📱 Browser Support
- Chrome/Edge 90+
- Firefox 88+
- Safari 14+
- Mobile browsers (iOS Safari, Chrome Mobile)

## 🔧 Configuration

### Environment Variables
**Frontend (.env):**
```env
VITE_API_URL=https://localhost:7081
VITE_DEV_MODE=true
```

### Database Configuration
**Backend (appsettings.json):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=PATRICK\\SQLEXPRESS;Database=PowersportsShowcase;Integrated Security=true;TrustServerCertificate=true;",
    "InMemoryConnection": "InMemory"
  },
  "DatabaseProvider": "SqlServer",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Customizing Database Connection
To use a different SQL Server instance, update the `DefaultConnection` in `appsettings.json`:

```json
"DefaultConnection": "Server=YOUR_SERVER\\YOUR_INSTANCE;Database=PowersportsShowcase;Integrated Security=true;TrustServerCertificate=true;"
```

Or use SQL Server authentication:
```json
"DefaultConnection": "Server=YOUR_SERVER;Database=PowersportsShowcase;User Id=username;Password=password;TrustServerCertificate=true;"
```

To force in-memory mode, change `DatabaseProvider`:
```json
"DatabaseProvider": "InMemory"
```

## 🚧 Future Enhancements
- [ ] Shopping cart functionality
- [ ] Product detail pages with image galleries
- [ ] Advanced search functionality with filters
- [ ] Email integration for contact form
- [ ] Payment processing integration
- [ ] Advanced inventory management with stock alerts
- [ ] User reviews and ratings system
- [ ] Product comparison feature
- [ ] Wishlist functionality
- [ ] Order history and tracking
- [ ] SEO optimization and meta tags
- [ ] Multi-language support
- [ ] Advanced analytics dashboard

## 🤝 Development Guidelines

### Code Style
- **Backend**: Follow .NET coding conventions
- **Frontend**: Use TypeScript and Vue 3 Composition API
- **Commits**: Use conventional commit messages
- **Components**: Keep components focused and reusable

### Adding New Features
1. **Backend**: Add models, services, and endpoints
2. **Frontend**: Create types, services, and components
3. **Testing**: Add appropriate tests
4. **Documentation**: Update README and API docs

## 📞 Support & Contact

For questions about this project:
- **Demo Contact**: Use the contact form in the application
- **Developer Support**: Create an issue in the repository
- **Business Inquiries**: info@powersportsshowcase.com

---

**Built with ❤️ using .NET 8 and Vue.js 3**