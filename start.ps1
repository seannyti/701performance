# PowerShell script to start both frontend and backend
param(
    [switch]$UseInMemory,
    [switch]$Help
)

if ($Help) {
    Write-Host "Powersports Showcase Startup Script" -ForegroundColor Green
    Write-Host "Usage: .\start.ps1 [-UseInMemory] [-Help]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Options:"
    Write-Host "  -UseInMemory    Use in-memory database instead of SQL Server"
    Write-Host "  -Help          Show this help message"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\start.ps1                # Use SQL Server (PATRICK\SQLEXPRESS)"
    Write-Host "  .\start.ps1 -UseInMemory   # Use in-memory database"
    exit 0
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting Powersports Showcase Application" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if we're in the right directory
if (-not (Test-Path "backend") -or -not (Test-Path "frontend")) {
    Write-Host "Error: backend or frontend folder not found!" -ForegroundColor Red
    Write-Host "Make sure you're running this script from the PowersportsShowcase directory." -ForegroundColor Yellow
    exit 1
}

# Kill any existing dotnet and node processes
Write-Host "Checking for running services..." -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Gray
try {
    Get-Process | Where-Object {$_.ProcessName -eq "dotnet" -or $_.ProcessName -eq "node"} | Stop-Process -Force -ErrorAction SilentlyContinue
    Write-Host "Existing services stopped (if any)" -ForegroundColor Green
} catch {
    Write-Host "No existing services to stop" -ForegroundColor Gray
}
Write-Host ""
Start-Sleep -Seconds 2

# Set database provider based on parameter
$databaseProvider = if ($UseInMemory) { "InMemory" } else { "SqlServer" }

if ($UseInMemory) {
    Write-Host "🔧 Using in-memory database" -ForegroundColor Yellow
} else {
    Write-Host "🔧 Using SQL Server database (PATRICK\SQLEXPRESS)" -ForegroundColor Green
}

Write-Host ""

# Function to start backend
function Start-Backend {
    Write-Host "Starting Backend API..." -ForegroundColor Green
    Write-Host "----------------------------------------" -ForegroundColor Gray
    
    Set-Location "backend\PowersportsApi"
    
    # Update appsettings.json if using in-memory
    if ($UseInMemory) {
        $appsettings = Get-Content "appsettings.json" | ConvertFrom-Json
        $appsettings.DatabaseProvider = "InMemory"
        $appsettings | ConvertTo-Json -Depth 10 | Set-Content "appsettings.json"
    }
    
    Start-Process -FilePath "cmd" -ArgumentList "/c", "dotnet run && pause" -WindowStyle Normal
    Set-Location "..\.."
}

# Function to start frontend
function Start-Frontend {
    Write-Host "Starting Frontend..." -ForegroundColor Green
    Write-Host "----------------------------------------" -ForegroundColor Gray
    
    Set-Location "frontend"
    Start-Process -FilePath "cmd" -ArgumentList "/c", "npm run dev && pause" -WindowStyle Normal
    Set-Location ".."
}

# Function to start admin dashboard
function Start-Admin {
    Write-Host "Starting Admin Dashboard..." -ForegroundColor Green
    Write-Host "----------------------------------------" -ForegroundColor Gray
    
    Set-Location "admin"
    Start-Process -FilePath "cmd" -ArgumentList "/c", "npm run dev && pause" -WindowStyle Normal
    Set-Location ".."
}

try {
    # Start backend
    Start-Backend
    
    # Wait for backend to start
    Write-Host "Waiting 10 seconds for backend to start..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10
    
    # Start frontend
    Start-Frontend
    
    # Start admin dashboard
    Start-Admin
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "All services are starting!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Backend API:      http://localhost:5226" -ForegroundColor White
    Write-Host "Frontend:         http://localhost:3000" -ForegroundColor White
    Write-Host "Admin Dashboard:  http://localhost:5174" -ForegroundColor White
    Write-Host "Swagger:          http://localhost:5226/swagger" -ForegroundColor White
    Write-Host ""
    Write-Host "Database:         $databaseProvider" -ForegroundColor $(if ($UseInMemory) { "Yellow" } else { "Green" })
    Write-Host ""
    Write-Host "All services are now running in separate windows." -ForegroundColor Green
    Write-Host "Close this window to continue (services will keep running)" -ForegroundColor Gray
    Write-Host ""
    
    Read-Host -Prompt "Press Enter to close this window"
}
catch {
    Write-Host "Error starting application: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}