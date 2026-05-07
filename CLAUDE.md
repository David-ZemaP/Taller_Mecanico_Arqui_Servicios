# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Taller Mecánico** is an academic microservices project for a Software Architecture course. It implements a mechanical workshop management system using three .NET 10 services communicating via REST + JWT.

## Running the Project

### Prerequisites
- .NET 10 SDK
- Docker Desktop (for PostgreSQL)

### Start the database
```powershell
docker compose up -d
```
PostgreSQL runs on port 5432, database `taller_mecanico`, user/password `postgres`/`postgres`. The `Scripts/init.sql` file is auto-executed on first startup to create schema and seed data.

### Start all services (Windows)
```powershell
.\Scripts\run-all.ps1
```
This opens 3 separate PowerShell windows. Alternatively, run each service manually:
```powershell
dotnet run --project UsersService/App          # http://localhost:5297
dotnet run --project OrdenTrabajoService       # http://localhost:5229
dotnet run --project Frontend                  # http://localhost:5146
```

### Default admin credentials
- Email: `administrador.principal@taller.com`
- Password: `ap100000`

### Build & test
```powershell
dotnet build
dotnet test tests/TallerMecanico.Tests
```

### BCrypt hash utility (for generating test passwords)
```powershell
dotnet run --project hashgen/
```

### pgAdmin (database GUI)
- URL: http://localhost:5050
- Login: `admin@taller.com` / `admin123`

## Architecture

Three services plus a shared PostgreSQL instance:

```
Frontend (5146)          → Razor Pages UI, HTTP adapters to both services
OrdenTrabajoService (5229) → Core business: clients, vehicles, work orders, inventory
UsersService (5297)      → Authentication, user management, JWT issuance
hashgen/                 → CLI utility for BCrypt hash generation
```

### UsersService — Clean Architecture (strict layering)
```
App/           → Controllers (AuthController, UsersController), Program.cs
UseCases/      → One class per operation (CreateUserUseCase, ChangePasswordUseCase, etc.)
Domain/        → UsuarioLogin aggregate, IUsuarioLoginRepository port, Result<T> pattern
Data/          → Repository impl (raw SQL via Npgsql + TransactionScope for audit logs)
Framework/     → ISqlConnectionFactory, IAuthenticationHelper, DTOs, Mappers
```
- Uses `Result<T>` (Either monad) instead of exceptions for error propagation.
- Every write operation records to `audit_logs` table inside the same transaction.
- Issues JWT tokens with claims: `NameIdentifier`, `Email`, `Role` ("Cliente"/"Empleado"), `EmpleadoId`, `ClienteId`.

### OrdenTrabajoService — Layered Architecture
```
Controllers/                   → 7 REST controllers
OrdenTrabajo.Aplication/       → Facades (complex ops), Use Cases, DTOs
OrdenTrabajo.Domain/           → Entities, repository ports (IRepository<T>), domain services
OrdenTrabajo.Infrastructure/   → Repository impls (raw SQL), HttpClient to UsersService
```
- Validates incoming JWT tokens using the shared secret from `appsettings.json`.
- Calls UsersService via `HttpClient` for user lookups when needed.

### Frontend — Razor Pages + Adapter Pattern
```
Pages/      → .cshtml + .cshtml.cs pairs, organized by domain (Clientes, Empleados, ordentrabajo, etc.)
Adapters/   → IOrdenTrabajoAdapter, IUsersServiceAdapter, IClienteAdapter, etc. — HTTP clients to services
```
- Authenticates via UsersService, stores JWT in session.
- All service calls pass `Authorization: Bearer <token>` header.
- `RequiereCambioPassword` flag forces password change on first login.

## Key Configuration

**JWT secret** (shared between UsersService and OrdenTrabajoService):
```
"Secret": "SuperSecretaClaveDePruebaParaTallerMecanico2026!!!"
"Issuer": "TallerMecanicoUsersApi"
"Audience": "TallerMecanicoClients"
"ExpirationInMinutes": 120
```

**Service URLs** (Frontend `appsettings.json`):
- OrdenTrabajoService: `http://localhost:5229`
- UsersService: `http://localhost:5297`

All other routes (`ClientesServiceUrl`, `EmpleadosServiceUrl`, etc.) also point to port 5229 since they are sub-resources of OrdenTrabajoService.

## Database Schema Notes

- All repositories use **raw SQL via Npgsql** — no ORM.
- Soft deletes via `IsDeleted` column on `cliente` and `empleado` tables.
- `usuariologin` has FK references to both `empleado.empleadoid` and `cliente.clienteid` (one user is either an employee or a client, not both).
- `audit_logs` records every INSERT/UPDATE/DELETE with table name, record ID, action, actor email, and timestamp.

## Password Policy (enforced in UsersService)
Minimum 8 characters, at least one uppercase, one lowercase, one digit, one special character (`!@#$%^&*`).
