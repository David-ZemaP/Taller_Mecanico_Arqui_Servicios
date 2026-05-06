# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

University project for Software Architecture. A mechanical workshop management system built as three independent .NET 10 services plus a legacy .NET 8 test project. The services share a PostgreSQL database (`TallerMecanico`, port 5433) exposed from a companion repository (`Taller_Mecanico_Arqui`).

## Services

There is **no solution file**; each service is a standalone `.csproj` and must be built/run individually by path.

| Project | Type | Dev URL (HTTP / HTTPS) | Description |
|---|---|---|---|
| `UsersService/` | Web API (net10) | `5297` / `7268` (Docker: `5005`) | Auth + user management |
| `OrdenTrabajoService/` | Web API (net10) | `5229` / `7274` | Work order management |
| `WebService/` | Razor Pages (net10) | `5146` / `7030` | Frontend UI |
| `tests/TallerMecanico.Tests/` | xUnit (net8) | — | Tests targeting a legacy `src/` layer that no longer exists in this repo (project references are broken) |

## Common Commands

```bash
# Run a service (use the project folder, not a .sln)
dotnet run --project UsersService
dotnet run --project OrdenTrabajoService
dotnet run --project WebService

# Build a specific service
dotnet build UsersService

# Tests — currently broken: TallerMecanico.Tests references ../../src/ projects that aren't in this repo
dotnet test tests/TallerMecanico.Tests

# Docker (UsersService only — requires the Postgres compose from the companion repo to be running first)
cd UsersService
docker compose up --build
```

> Note: `UsersService/App/Dockerfile` references `Taller_Mecanico_Users.sln`, which does not exist in this repo. The Dockerfile is stale and will fail until updated.

## Architecture

### UsersService — Clean Architecture (single project, layered by folder)

The most developed service. It is a **single `.csproj`** whose source is organized into Clean Architecture layers as folders. Namespaces use the prefix `Taller_Mecanico_Users.<Folder>`.

```
UsersService/
├── App/            # Entry point: Controllers/, Middleware/, Infrastructure/ (SqlConnectionFactory), Services/ (AuthHelper)
├── Domain/         # Entities, Ports (interfaces), ValueObjects, Common/Result<T>, Common/ErrorCodes, Enums
├── Data/           # Repository implementations (raw Npgsql SQL — no ORM)
├── UseCases/       # One class per operation (CreateUserUseCase, ChangePasswordUseCase, ...), returns Result<T>
└── Framework/      # DTOs, shared service interfaces (IAuthenticationHelper, IMailSender, ISqlConnectionFactory)
```

DI wiring lives in `Program.cs` and registers each use case explicitly by full namespace. Use cases never throw for business failures — they return `Result`/`Result<T>`. JWT auth is configured under `JwtSettings` in `appsettings.json`. Password hashing uses BCrypt (`UseCases/Users/PasswordSecurity.cs`). `IMailSender` is currently bound to `DummyMailSender` (no real mail).

### OrdenTrabajoService — Facade + Use Cases (single project, namespaced `Taller_Mecanico_Arqui.*`)

Also a single `.csproj`. Namespaces use `Taller_Mecanico_Arqui.<Layer>` even though the folder names are `OrdenTrabajo.Aplication` etc. Note the misspelling **`Aplication`** (one `p`) in the folder name — the namespace is `Application` (two `p`s).

```
OrdenTrabajoService/
├── Controllers/                # HTTP entry point
├── OrdenTrabajo.Aplication/    # folder is "Aplication" (sic); namespace is Taller_Mecanico_Arqui.Application
│   ├── UseCases/               # Granular operations (Create, GetAll, GetById, Update, SetAnulacion)
│   └── Facades/                # Orchestrators that combine use cases (OrdenTrabajoCreate, OrdenTrabajoAnular, UpdateProductStocks)
├── OrdenTrabajo.Domain/        # Entities, Interfaces, Enums
└── OrdenTrabajo.Infrastructure/
    ├── FactoryCreators/        # Abstract Factory for repository creation
    └── FactoryProducts/        # Concrete repositories
```

Controllers call **Facades**, not use cases directly. Facades own DTO ↔ entity mapping and orchestrate multiple use cases (e.g. `OrdenTrabajoCreate.RegistrarProcesoPrincipalAsync` calls `CreateOrdenTrabajoUseCase` then `UpdateProductStocks`).

### WebService — Razor Pages frontend

Calls UsersService and OrdenTrabajoService via HTTP. Uses `Adapters/` to map external API responses to internal DTOs (`DTOs/`). Session/auth state managed via cookie-based login through UsersService.

## Database

- PostgreSQL, accessed via raw `Npgsql` (no EF Core)
- Connection string: `Host=host.docker.internal;Port=5433;Database=TallerMecanico;Username=admin;Password=tallermecanico2026`
- The database lives in the companion repo `Taller_Mecanico_Arqui` — that compose must be running first

## Key Patterns

- **Result<T>**: All use cases and repositories return `Result` or `Result<T>` instead of throwing exceptions. Check `IsFailure` before accessing `.Value`. Error codes come from `Domain/Common/ErrorCodes.cs` (e.g. `ErrorCodes.ValidationInvalidValue`, `ErrorCodes.DbError`) — reuse them; don't invent ad-hoc strings.
- **No ORM**: All SQL is handwritten using `Npgsql`. `ISqlConnectionFactory` creates `NpgsqlConnection` instances on demand. There is no `DbContext`, no migrations — schema lives in the companion repo's database.
- **DI is explicit and namespace-qualified**: `Program.cs` registers each use case and port with fully-qualified type names. When adding a new use case, register it the same way.
- **DTO locations differ per service**: UsersService DTOs are in `Framework/DTOs/`. OrdenTrabajoService DTOs are in `OrdenTrabajo.Aplication/DTOs/`.
- **Nullable enabled**: All projects have `<Nullable>enable</Nullable>`. Use null-forgiving (`!`) only when provably non-null.
- **Spanish domain language**: entities, properties, and enums use Spanish (`Cliente`, `Vehiculo`, `OrdenTrabajo`, `EstadoTrabajo.Pendiente`). Match this when extending the domain.

## Cross-service communication

`WebService` is the only consumer of the API services. It calls them over HTTP via `HttpClient` from page handlers (`Pages/**/*.cshtml.cs`) and maps responses through `Adapters/`. The two API services do not call each other.
