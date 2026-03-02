@echo off
title Powersports Showcase Startup
echo ========================================
echo Starting Powersports Showcase Application
echo ========================================
echo.

REM Check if we're in the right directory
if not exist "backend" (
    echo Error: backend folder not found!
    echo Make sure you're running this script from the PowersportsShowcase directory.
    pause
    exit /b 1
)
if not exist "frontend" (
    echo Error: frontend folder not found!
    echo Make sure you're running this script from the PowersportsShowcase directory.
    pause
    exit /b 1
)

echo Checking for running services...
echo ----------------------------------------
REM Kill any existing dotnet and node processes
powershell -Command "Get-Process | Where-Object {$_.ProcessName -eq 'dotnet' -or $_.ProcessName -eq 'node'} | Stop-Process -Force" 2>nul
echo Existing services stopped (if any)
echo.
timeout /t 2 /nobreak >nul

echo Starting Backend API...
echo ----------------------------------------
start "Powersports API" cmd /c "cd backend\PowersportsApi && dotnet run && pause"

echo Waiting 10 seconds for backend to start...
timeout /t 10 /nobreak

echo Starting Frontend...
echo ----------------------------------------
start "Powersports Frontend" cmd /c "cd frontend && npm run dev && pause"

echo Starting Admin Dashboard...
echo ----------------------------------------
start "Powersports Admin" cmd /c "cd admin && npm run dev && pause"

echo.
echo ========================================
echo All services are starting!
echo ========================================
echo Backend API:      http://localhost:5226
echo Frontend:         http://localhost:3000
echo Admin Dashboard:  http://localhost:5174
echo Swagger:          http://localhost:5226/swagger
echo.
echo All services are now running in separate windows.
echo Close this window to continue (services will keep running)
echo.
pause