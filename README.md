# Powersports Showcase - Full-Stack Application

A modern full-stack web application showcasing powersports vehicles and gear, built with .NET 8 Web API backend and Vue.js 3 frontend.

## 🏗️ Project Structure

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
- ⚙️ **Dynamic Settings** - Edit site content without code changes
- 🔗 **Unified Login** - Seamless access from main site for admin users
- 🛡️ **Security** - Route guards and API endpoint protection

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
- ✅ **Responsive Design**: Mobile-first, responsive UI
- ✅ **Navigation**: Clean routing between Home, Products, About, and Contact pages
- ✅ **API Integration**: RESTful API with full CRUD capabilities
- ✅ **Category Filtering**: Filter products by category
- ✅ **Contact Form**: Functional contact form with validation
- ✅ **Modern UI**: Professional, sleek design with smooth transitions

### API Endpoints
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `GET /api/products/category/{category}` - Get products by category
- `GET /api/products/featured` - Get featured products for home page
- `GET /api/health` - Health check endpoint

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
- [ ] User authentication and authorization
- [ ] Shopping cart functionality
- [ ] Real database integration (Entity Framework Core)
- [ ] Product detail pages
- [ ] Search functionality
- [ ] Image upload for products
- [ ] Email integration for contact form
- [ ] Payment processing
- [ ] Inventory management
- [ ] User reviews and ratings

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