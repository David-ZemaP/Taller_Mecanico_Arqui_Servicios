# Taller Mecánico - Script de inicio rápido
# Uso: .\run-all.ps1

param(
    [switch]$NoDocker   # Usar si ya tienes PostgreSQL corriendo en otro lado
)

$ErrorActionPreference = "Stop"
$projectRoot = $PSScriptRoot

Write-Host ""
Write-Host "  ╔═══════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "  ║       TALLER MECHANICO - STARTUP            ║" -ForegroundColor Cyan
Write-Host "  ╚═══════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Verificar que dotnet esté disponible
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "[ERROR] dotnet CLI no está instalado o no está en PATH" -ForegroundColor Red
    Write-Host "Descarga: https://dotnet.microsoft.com/download" -ForegroundColor Gray
    exit 1
}

# Verificar versión de .NET
$dotnetVersion = dotnet --version
Write-Host "[OK] .NET $dotnetVersion instalado" -ForegroundColor Green

# Verificar Docker
if (Get-Command docker -ErrorAction SilentlyContinue) {
    $dockerRunning = docker info 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Docker corriendo" -ForegroundColor Green
    } else {
        Write-Host "[!] Docker instalado pero no corriendo" -ForegroundColor Yellow
    }
} else {
    Write-Host "[!] Docker no encontrado (opcional)" -ForegroundColor Yellow
}

# Verificar/carpeta Scripts existe
if (-not (Test-Path "$projectRoot\Scripts")) {
    Write-Host "[ERROR] No se encontró la carpeta Scripts/" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "  Servicios:" -ForegroundColor Cyan
Write-Host "  ├─ Frontend:         http://localhost:5146" -ForegroundColor White
Write-Host "  ├─ UsersService:    http://localhost:51029" -ForegroundColor White
Write-Host "  ├─ OrdenService:    http://localhost:5229" -ForegroundColor White
Write-Host "  └─ pgAdmin:         http://localhost:5050" -ForegroundColor White
Write-Host ""
Write-Host "  Login: administrador.principal@taller.com / ap100000" -ForegroundColor White
Write-Host ""

# Verificar que los proyectos existan
$projects = @(
    @{Name="UsersService"; Path="$projectRoot\UsersService\App"},
    @{Name="OrdenService"; Path="$projectRoot\OrdenTrabajoService"},
    @{Name="WebService"; Path="$projectRoot\WebService"}
)

foreach ($p in $projects) {
    if (-not (Test-Path "$($p.Path)\$($p.Name.Split('-')[0]).csproj")) {
        $csproj = Get-ChildItem "$($p.Path)\*.csproj" | Select-Object -First 1
        if (-not $csproj) {
            Write-Host "[ERROR] No se encontró .csproj en $($p.Path)" -ForegroundColor Red
            exit 1
        }
    }
}

Write-Host "Iniciando servicios...`n" -ForegroundColor Cyan

# Función para iniciar un proyecto en una nueva ventana
function Start-ServiceWindow {
    param([string]$Name, [string]$Path, [string]$Port)
    
    $workingDir = if (Test-Path "$Path\App") { "$Path\App" } else { $Path }
    
    Write-Host "  -> Iniciando $Name en puerto $Port..." -ForegroundColor Yellow
    
    Start-Process powershell -ArgumentList @(
        "-NoExit", 
        "-Command",
        "Write-Host ''; Write-Host '  [$Name]' -ForegroundColor Cyan; Write-Host '  URL: http://localhost:$Port'; Write-Host '  Presiona Ctrl+C para detener'; Write-Host '';",
        "Set-Location '$workingDir';",
        "dotnet run;",
        "Read-Host 'Presiona Enter para salir'"
    ) -WindowStyle Normal
    
    Start-Sleep -Milliseconds 500
}

# Iniciar cada servicio en su propia ventana
Start-ServiceWindow -Name "UsersService" -Path "$projectRoot\UsersService" -Port "51029"
Start-ServiceWindow -Name "OrdenService" -Path "$projectRoot\OrdenTrabajoService" -Port "5229"
Start-ServiceWindow -Name "WebService" -Path "$projectRoot\WebService" -Port "5146"

Write-Host ""
Write-Host "  ╔═══════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "  ║  Servicios iniciados en nuevas ventanas!     ║" -ForegroundColor Green
Write-Host "  ╚═══════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "  IMPORTANTE: Mantén estas ventanas abiertas." -ForegroundColor Yellow
Write-Host "  Puedes minimizar las ventanas de los servicios (backend)." -ForegroundColor Gray
Write-Host "  Usa la ventana de WebService/Frontend como tu interfaz principal." -ForegroundColor Gray
Write-Host ""