# Script para ejecutar los 3 servicios del Taller Mecánico
# Ejecución en paralelo

param(
    [switch]$Watch,
    [switch]$Build,
    [switch]$Kill
)

$ErrorActionPreference = "Continue"
$RootDir = "C:\Users\skt1f\Documents\Cato\2026\2026-I\ARQUI\Taller_Mecanico_Arqui_Services"

function Write-Step($msg) {
    Write-Host "`n==> $msg" -ForegroundColor Cyan
}

# Kill existing processes on ports
if ($Kill) {
    Write-Step "Killing processes on ports 5146, 5229, 51029..."
    Get-NetTCPConnection -LocalPort 5146, 5229, 51029 -ErrorAction SilentlyContinue | 
        ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }
    Start-Sleep -Milliseconds 500
}

$watchFlag = if ($Watch) { "--watch" } else { "" }

# Build if requested
if ($Build) {
    Write-Step "Building all services..."
    dotnet build "$RootDir\WebService\WebService.csproj"
    dotnet build "$RootDir\OrdenTrabajoService\OrdenTrabajoService.csproj"
    dotnet build "$RootDir\UsersService\App\App.csproj"
}

# Run all three services
Write-Step "Starting services..."

Write-Host "[WebService]          http://localhost:5146" -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$RootDir\WebService'; dotnet run $watchFlag --project WebService.csproj"

Write-Host "[OrdenTrabajoService]  http://localhost:5229" -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$RootDir\OrdenTrabajoService'; dotnet run $watchFlag --project OrdenTrabajoService.csproj"

Write-Host "[UsersService]       http://localhost:51029" -ForegroundColor Magenta
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$RootDir\UsersService\App'; dotnet run $watchFlag --project App.csproj"

Write-Host "`n==> All services started!" -ForegroundColor Green