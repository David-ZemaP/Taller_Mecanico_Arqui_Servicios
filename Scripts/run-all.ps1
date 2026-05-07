# Script para ejecutar todos los servicios del Taller Mecánico
# Uso: .\run-all.ps1

param(
    [switch]$SkipAdminCreation
)

$ErrorActionPreference = "Continue"
$ProjectRoot = Split-Path -Parent $PSScriptRoot

Write-Host "=== Taller Mecánico - Iniciando servicios ===" -ForegroundColor Cyan

# Cargar variables desde .env al entorno actual para que dotnet run las herede.
$envFile = Join-Path $ProjectRoot ".env"
if (Test-Path $envFile) {
    Write-Host "Cargando variables desde .env..." -ForegroundColor DarkCyan
    Get-Content $envFile | ForEach-Object {
        $line = $_.Trim()
        if ([string]::IsNullOrWhiteSpace($line) -or $line.StartsWith("#")) {
            return
        }

        $parts = $line -split "=", 2
        if ($parts.Length -eq 2) {
            $key = $parts[0].Trim()
            $value = $parts[1].Trim()
            [System.Environment]::SetEnvironmentVariable($key, $value, "Process")
        }
    }
}

# Configuración de puertos
$Ports = @{
    "UsersService" = 5297
    "OrdenTrabajoService" = 5229
    "Frontend" = 5146
}

# Verificar que los puertos no estén en uso
foreach ($service in $Ports.Keys) {
    $port = $Ports[$service]
    $process = Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue
    if ($process) {
        Write-Host "ADVERTENCIA: Puerto $port ($service) ya está en uso" -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "Puertos:" -ForegroundColor White
Write-Host "  UsersService:         http://localhost:$($Ports['UsersService'])" -ForegroundColor Gray
Write-Host "  OrdenTrabajoService: http://localhost:$($Ports['OrdenTrabajoService'])" -ForegroundColor Gray
Write-Host "  Frontend:             http://localhost:$($Ports['Frontend'])" -ForegroundColor Gray
Write-Host ""

# Rutas de proyectos ejecutables
$FrontendPath = Join-Path $ProjectRoot "Frontend"
$UsersServicePath = Join-Path $ProjectRoot "UsersService\App"
$OrdenTrabajoPath = Join-Path $ProjectRoot "OrdenTrabajoService"

Write-Host "Iniciando UsersService..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$UsersServicePath'; dotnet run" -WindowStyle Normal

Write-Host "Iniciando OrdenTrabajoService..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$OrdenTrabajoPath'; dotnet run" -WindowStyle Normal

Write-Host "Iniciando Frontend..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$FrontendPath'; dotnet run" -WindowStyle Normal

Write-Host ""
Write-Host "=== Esperando que los servicios arranquen (5 segundos) ===" -ForegroundColor Cyan
Start-Sleep -Seconds 5

# Crear usuario administrador si no existe
if (-not $SkipAdminCreation) {
    Write-Host ""
    Write-Host "Creando usuario administrador..." -ForegroundColor Green
    $CreateAdminPath = Join-Path $PSScriptRoot "CreateAdminUser.csproj"
    dotnet run --project $CreateAdminPath 2>&1
}

Write-Host ""
Write-Host "=== Todos los servicios iniciados ===" -ForegroundColor Cyan
Write-Host "  Frontend:       http://localhost:$($Ports['Frontend'])" -ForegroundColor White
Write-Host "  UsersService:   http://localhost:$($Ports['UsersService'])" -ForegroundColor White
Write-Host ""
Write-Host "Credenciales Admin:" -ForegroundColor White
Write-Host "  Email:    administrador.principal@taller.com" -ForegroundColor Gray
Write-Host "  Password: ap100000" -ForegroundColor Gray
Write-Host ""
Write-Host "Presiona Ctrl+C en cada ventana para detener los servicios" -ForegroundColor Yellow
