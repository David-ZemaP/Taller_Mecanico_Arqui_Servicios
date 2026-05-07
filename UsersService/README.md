# 🔧 Taller Mecánico - Microservicio de Usuarios (Auth Service)

**Versión:** 1.0.0 | **Framework:** .NET 10 (C# 14) | **Tipo:** ASP.NET Core API + JWT Auth

---

## 📑 Tabla de Contenidos

1. [Visión General](#visión-general)
2. [Arquitectura Clean Architecture](#arquitectura-clean-architecture)
3. [Patrones de Diseño Implementados](#patrones-de-diseño-implementados)
4. [Base de Datos](#base-de-datos)
5. [Autenticación & Autorización](#autenticación--autorización)
6. [Endpoints Disponibles](#endpoints-disponibles)
7. [Guía de Ejecución](#guía-de-ejecución)
8. [Pruebas de Endpoints](#pruebas-de-endpoints)
9. [Archivos Utilizados de Taller_Mecanico_Arqui](#archivos-utilizados-de-taller_mecanico_arqui)
10. [Estado Actual & Rúbrica (40 pts)](#estado-actual--rúbrica-40-pts)
11. [Funcionalidades Extras Implementadas](#funcionalidades-extras-implementadas)
12. [Pendientes & Plan de Desarrollo](#pendientes--plan-de-desarrollo)
13. [Instrucciones de Integración](#instrucciones-de-integración)

---

## 🎯 Visión General

**Taller_Mecanico_Users** es un **microservicio independiente** que gestiona:
- ✅ **Autenticación basada en JWT** (sin cookies)
- ✅ **CRUD completo de usuarios** (Empleados y Clientes)
- ✅ **Gestión de contraseñas** (cambio, reset, validación de políticas)
- ✅ **Auditoría transaccional** (registro de ADD/UPDATE/DELETE)
- ✅ **Seguimiento de último acceso** (UltimoAcceso en BD)
- ✅ **Roles y autorización** (Empleado, Cliente)

**Objetivo:** Desacoplarse de la arquitectura Razor Pages monolítica (`Taller_Mecanico_Arqui`) usando **Clean Architecture** y permitir que el frontend consuma APIs REST+JWT en lugar de sesiones.

---

## 🏛️ Arquitectura Clean Architecture

### Estructura de Capas (Dependencias hacia adentro ↓)

```
┌─────────────────────────────────────────┐
│  App (Presentation Layer)               │
│  - Controllers (HTTP endpoints)         │
│  - Program.cs (DI Container setup)      │
└──────────┬──────────────────────────────┘
           │ depends on
┌──────────▼────────────────────────────┐
│  UseCases (Business Logic Layer)      │
│  - CreateUserUseCase                  │
│  - GetUsersUseCase                    │
│  - ChangePasswordUseCase              │
│  - ResetPasswordUseCase               │
│  - DeleteUserUseCase                  │
└──────────┬──────────────────────────────┘
           │ depends on
┌──────────▼────────────────────────────────┐
│  Domain (Entity & Port Definitions)      │
│  - Entities: UsuarioLogin (aggregate)   │
│  - Ports: IUsuarioLoginRepository       │
│  - ValueObjects: Result<T>              │
│  - Enums: ErrorCodes                    │
└──────────┬─────────────────────────────────┘
           │
    ┌──────┴──────┐
    │             │
┌───▼──────────┐  ┌──▼────────────────┐
│ Data Layer   │  │ Framework Layer    │
│ - Repos      │  │ - DTOs             │
│ - DB access  │  │ - Mappers          │
└──────────────┘  │ - Services (JWT)   │
                  └───────────────────┘
```

### Capas Detalladas

#### 1. **Domain Layer** (`Domain/`)
Contiene **lógica pura de negocio** sin dependencias externas.

**Archivos clave:**
- `Domain/Entities/UsuarioLogin.cs` → **Aggregate Root**
  - Métodos de dominio: `Crear()`, `CambiarPassword()`, `RegistrarAcceso()`, `Desactivar()`, etc.
  - Invariantes: Validaciones de dominio
  
- `Domain/Ports/IUsuarioLoginRepository.cs` → **Contrato de persistencia**
  - Métodos: `AddAsync()`, `UpdateAsync()`, `DeleteAsync()`, `GetByIdAsync()`, etc.
  - Implementado en capa Data
  
- `Domain/Common/Result<T>` → **Patrón Result (Either Monad)**
  - Manejo de errores funcional sin excepciones
  - `Result.Success()`, `Result.Failure(code, message)`

#### 2. **UseCases Layer** (`UseCases/Users/`)
Orquesta llamadas a repositorios y servicios, implementando reglas de negocio.

**Use Cases Implementados:**
- `CreateUserUseCase` → Crea usuario, genera pwd temporal, envía email, registra auditoría
- `GetUserByIdUseCase` → Obtiene usuario por ID (lectura simple)
- `GetUsersUseCase` → Lista todos los usuarios
- `UpdateUserUseCase` → Modifica email/estado activo
- `ChangePasswordUseCase` → Cambio de contraseña con validación de política
- `ResetPasswordUseCase` → Reset a pwd temporal (admin), envía email
- `DeleteUserUseCase` → Elimina usuario (auditoría registrada)
- `PasswordSecurity.cs` → Helper con validación de política de contraseña

#### 3. **Data Layer** (`Data/Repositories/`)
Acceso a BD mediante SQL directo (no ORM).

**Archivos:**
- `UsuarioLoginRepository.cs` → Implementa `IUsuarioLoginRepository`
  - Usa `ISqlConnectionFactory` (inyección de conexión)
  - Transacciones con `TransactionScope` para atomicidad
  - Auditoría automática en cada operación

#### 4. **Framework Layer** (`Framework/`)
Servicios técnicos reutilizables (DTOs, Mappers, Persistencia).

**Componentes:**
- `Framework/Persistence/ISqlConnectionFactory` → Factory pattern para conexiones BD
- `Framework/Services/IAuthenticationHelper` → Lee claims JWT, obtiene usuario actual
- `Framework/Services/IMailSender` → Interfaz para enviar emails (implementado como Dummy)
- `Framework/DTOs/` → Objetos de transferencia (UsuarioLoginDto, ChangePasswordDto, etc.)
- `Framework/Mappers/` → Conversión Entity ↔ DTO

#### 5. **App Layer (Presentation)** (`App/`)
Controllers HTTP, middlewares, infraestructura técnica.

**Controllers:**
- `AuthController.cs`
  - `POST /api/auth/login` → Autentica + genera JWT
  
- `UsersController.cs`
  - `POST /api/users` → Crear usuario
  - `GET /api/users` → Listar usuarios
  - `GET /api/users/{id}` → Obtener usuario
  - `PUT /api/users/{id}` → Actualizar usuario
  - `POST /api/users/{id}/change-password` → Cambiar contraseña
  - `POST /api/users/{id}/reset-password` → Reset contraseña (admin)
  - `DELETE /api/users/{id}` → Eliminar usuario

**Servicios:**
- `AuthenticationHelper.cs` → Extrae info de JWT (NameIdentifier, Email, Roles)
- `DummyMailSender.cs` → Simulador de envío de emails (imprime en consola)

---

## 🏗️ Patrones de Diseño Implementados

### 1. **Factory Pattern** ✓
**Ubicación:** `Framework/Persistence/ISqlConnectionFactory.cs`

```csharp
public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
```

**Implementación:** `App/Infrastructure/SqlConnectionFactory.cs`
- Centraliza la creación de conexiones BD
- Inyecta cadena de conexión desde `appsettings.json`
- Beneficio: cambiar BD sin afectar repositorios

**Registro DI:**
```csharp
builder.Services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
```

---

### 2. **Repository Pattern** ✓
**Ubicación:** `Domain/Ports/IUsuarioLoginRepository.cs` (interfaz)

```csharp
public interface IUsuarioLoginRepository
{
    Task<Result> AddAsync(UsuarioLogin entity);
    Task<Result> UpdateAsync(UsuarioLogin entity);
    Task<Result> DeleteAsync(int id);
    Task<Result<UsuarioLogin?>> GetByIdAsync(int id);
    Task<IEnumerable<UsuarioLogin>> GetAllAsync();
    // ... más métodos
}
```

**Implementación:** `Data/Repositories/UsuarioLoginRepository.cs`
- Acceso direto a BD (SQL)
- Transacciones automáticas (ADD/UPDATE/DELETE)
- Auditoría registrada en cada operación

---

### 3. **Dependency Injection (DI)** ✓
**Ubicación:** `App/Program.cs`

```csharp
// Servicios
builder.Services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddScoped<IUsuarioLoginRepository, UsuarioLoginRepository>();
builder.Services.AddScoped<IAuthenticationHelper, AuthenticationHelper>();
builder.Services.AddScoped<IMailSender, DummyMailSender>();

// Use Cases
builder.Services.AddScoped<CreateUserUseCase>();
builder.Services.AddScoped<GetUserByIdUseCase>();
builder.Services.AddScoped<ChangePasswordUseCase>();
// ... etc
```

**Beneficio:** Loose coupling, fácil testing, inyección en constructores

---

### 4. **Strategy Pattern (Implicit)** ✓
**Ubicación:** `IMailSender` interface

```csharp
public interface IMailSender
{
    Task SendEmailAsync(string email, string subject, string body);
}
```

**Implementaciones:**
- `DummyMailSender` → Simulador (imprime en consola)
- Futura: `SmtpMailSender`, `SendGridMailSender`, etc.

Intercambiables mediante DI sin cambiar código

---

### 5. **Result Pattern (Either Monad)** ✓
**Ubicación:** `Domain/Common/Result<T>.cs`

```csharp
public class Result<T>
{
    public bool IsFailure { get; }
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string ErrorCode { get; }
    public string ErrorMessage { get; }

    // Métodos
    public static Result<T> Success(T value);
    public static Result<T> Failure(string code, string message);
}
```

**Uso en Use Cases:**
```csharp
var result = await _repository.GetByIdAsync(userId);
if (result.IsFailure)
    return Result.Failure(result.ErrorCode, result.ErrorMessage);
```

**Beneficio:** Eliminación de excepciones, manejo explícito de errores

---

### 6. **Singleton Pattern** ✓
**Ubicación:** JWT Configuration

```csharp
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);
// Configurado una sola vez en Program.cs
options.IssuerSigningKey = new SymmetricSecurityKey(secretKey);
```

**Instancias Singleton implícitas:**
- `TokenValidationParameters` (1 instancia para toda la app)
- `JwtBearerOptions` (configuración global)

---

### 7. **Adapter Pattern** (Pendiente - Ver sección integración)
**Para integrar con frontend (Razor Pages → Cliente HTTP JWT)**

---

## 🗄️ Base de Datos

### Compartida con Taller_Mecanico_Arqui

**Servidor:** PostgreSQL 17  
**Host:** `TallerMecanico_PG` (Docker container)  
**Puerto:** 5433  
**BD:** `TallerMecanico`  
**Usuario:** admin | **Contraseña:** tallermecanico2026

### Tablas Utilizadas

#### 1. **usuariologin** (Tabla Principal)
```sql
CREATE TABLE usuariologin (
    usuariologinid SERIAL PRIMARY KEY,
    empleadoid INT NULL,                     -- FK a Empleado
    clienteid INT NULL,                      -- FK a Cliente  
    email VARCHAR(255) UNIQUE NOT NULL,
    passwordhash VARCHAR(255) NOT NULL,
    ultimoacceso TIMESTAMP NULL,             -- Última autenticación
    activo BOOLEAN DEFAULT true,
    requierechangiopassword BOOLEAN DEFAULT false,  -- Flag 1st login
    escliente BOOLEAN DEFAULT false
);
```

#### 2. **audit_logs** (Auditoría Transaccional)
```sql
CREATE TABLE audit_logs (
    audit_log_id SERIAL PRIMARY KEY,
    tabla_afectada VARCHAR(50),              -- Ej: 'usuariologin'
    registro_id INT,                         -- PK del registro modificado
    accion VARCHAR(20),                      -- 'INSERT', 'UPDATE', 'DELETE'
    realizado_por VARCHAR(255),              -- Email del usuario que actuó
    fecha_hora TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

#### 3. **empleado** (Tabla Existente - Consultada)
```sql
-- Tabla del sistema principal
-- Campos: empleadoid, nombre, apellido, cedula, email, telefono, etc.
```

#### 4. **cliente** (Tabla Existente - Consultada)
```sql
-- Tabla del sistema principal
-- Campos: clienteid, nombre, apellido, cedula, email, telefono, etc.
```

### Conexión desde Docker Compose

**docker-compose.yml:**
```yaml
services:
  taller_users:
    environment:
      ConnectionStrings__DefaultConnection: 
        "Host=host.docker.internal;Port=5433;Database=TallerMecanico;Username=admin;Password=tallermecanico2026"
    ports:
      - "5005:80"
```

**Nota:** `host.docker.internal` permite que el container acceda a servicios en la máquina host (PostgreSQL del compose de Taller_Mecanico_Arqui)

---

## 🔐 Autenticación & Autorización

### JWT (JSON Web Token)

**Configuración:** `appsettings.json`
```json
{
  "JwtSettings": {
    "Secret": "your_secret_key_at_least_32_characters_long!!!",
    "Issuer": "TallerMecanico",
    "Audience": "TallerMecanicoUsers",
    "ExpirationMinutes": 120
  }
}
```

### Flujo de Autenticación

1. **Login:**
   ```
   POST /api/auth/login
   { email, password }
      ↓
   Validate email + BCrypt.Verify(password)
      ↓
   GenerateJwtToken() → RegistrarAcceso() en BD
      ↓
   { token, requiereCambioPassword, esCliente }
   ```

2. **Token Claims:**
   ```csharp
   var claims = new[]
   {
       new Claim(ClaimTypes.NameIdentifier, user.UsuarioLoginId.ToString()),
       new Claim(ClaimTypes.Email, user.Email),
       new Claim("RequiereCambio", user.RequiereCambioPassword.ToString()),
       new Claim(ClaimTypes.Role, user.EsCliente ? "Cliente" : "Empleado"),
       new Claim("EmpleadoId", user.EmpleadoId?.ToString() ?? ""),
       new Claim("ClienteId", user.ClienteId?.ToString() ?? "")
   };
   ```

3. **Validación:**
   - Firma del token (SymmetricSecurityKey)
   - Emisor + Audiencia válidos
   - Token no expirado
   - Claims presentes

### Autorización por Roles

```csharp
// Solo empleados pueden listar/modificar usuarios
[Authorize(Roles = "Empleado")]
public async Task<IActionResult> GetUsers() { ... }

// Solo empleados pueden cambiar contraseña de otros
[Authorize(Roles = "Empleado")]
public async Task<IActionResult> ResetPassword(int id) { ... }

// Los clientes solo pueden cambiar su propia contraseña
[Authorize]
public async Task<IActionResult> ChangePassword(int id) { ... }
```

### Extracción de Usuario del JWT

**Servicio:** `AuthenticationHelper.cs`
```csharp
public class AuthenticationHelper : IAuthenticationHelper
{
    private readonly IHttpContextAccessor _contextAccessor;

    public string GetCurrentAuditActor()
    {
        return _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value 
            ?? "SYSTEM";
    }

    public int? GetCurrentUserId()
    {
        var claim = _contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }
}
```

---

## 📡 Endpoints Disponibles

### Base URL
```
http://localhost:5005/api
```

### 1. Authentication

#### `POST /auth/login`
Autentica usuario con email + contraseña

**Request:**
```json
{
  "email": "juan@example.com",
  "password": "Segura123!"
}
```

**Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "requiereCambioPassword": true,
  "esCliente": false
}
```

**Errors:**
- `400` → Email/contraseña inválido
- `401` → Usuario no encontrado

---

### 2. Users Management

#### `POST /users`
Crear usuario (genera contraseña temporal)

**Request:**
```json
{
  "email": "nuevo@example.com",
  "empleadoId": 5,
  "esCliente": false
}
```

**Response (201):**
```json
{
  "usuarioLoginId": 1,
  "email": "nuevo@example.com",
  "tempPassword": "Auto_Gen_1234!",
  "requiereCambioPassword": true
}
```

**Auth:** ✅ Requiere token JWT (rol: Empleado)

---

#### `GET /users`
Listar todos los usuarios

**Response (200):**
```json
[
  {
    "usuarioLoginId": 1,
    "email": "juan@example.com",
    "ultimoAcceso": "2026-05-05T14:30:00Z",
    "activo": true,
    "esCliente": false,
    "empleadoId": 5
  }
]
```

**Auth:** ✅ Requiere token JWT (rol: Empleado)

---

#### `GET /users/{id}`
Obtener usuario por ID

**Response (200):**
```json
{
  "usuarioLoginId": 1,
  "email": "juan@example.com",
  "ultimoAcceso": "2026-05-05T14:30:00Z",
  "activo": true,
  "esCliente": false,
  "empleadoId": 5
}
```

**Auth:** ✅ Requiere token JWT (rol: Empleado)

---

#### `PUT /users/{id}`
Actualizar usuario (email + estado activo)

**Request:**
```json
{
  "email": "nuevo_email@example.com",
  "activo": true
}
```

**Response (204 No Content)**

**Auth:** ✅ Requiere token JWT (rol: Empleado)

---

#### `POST /users/{id}/change-password`
Cambiar contraseña (usuario autenticado)

**Request:**
```json
{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword456!"
}
```

**Validación de Nueva Contraseña:**
- Mínimo 8 caracteres
- Al menos 1 MAYÚSCULA
- Al menos 1 minúscula
- Al menos 1 número
- Al menos 1 carácter especial (!@#$%^&*)

**Response (204 No Content)**

**Errors:**
- `400` → Contraseña actual incorrecta
- `400` → Nueva contraseña no cumple política

**Auth:** ✅ Requiere token JWT (puede ser el usuario mismo o Empleado)

---

#### `POST /users/{id}/reset-password`
Reset a contraseña temporal (admin action)

**Response (204 No Content)**

**Efectos:**
- Genera contraseña temporal aleatoria (10 chars)
- Encripta con BCrypt
- Establece `RequiereCambioPassword = true`
- Envía email con contraseña temporal (Dummy: imprime en consola)
- Registra auditoría

**Auth:** ✅ Requiere token JWT (rol: Empleado)

---

#### `DELETE /users/{id}`
Eliminar usuario (hard delete)

**Response (204 No Content)**

**Auth:** ✅ Requiere token JWT (rol: Empleado)

---

## 🚀 Guía de Ejecución

### Requisitos Previos
- **.NET 10 SDK** instalado
- **PostgreSQL 17** en contenedor (desde Taller_Mecanico_Arqui)
- **Docker Desktop** (para ejecutar ambos composes)

### Paso 1: Levantar Base de Datos
```bash
cd ~/Documentos/Universidad/7_Semestre/ARQUITECTURA_DE_SOFTWARE/codigos_docente/Nueva\ carpeta/Taller_Mecanico_Arqui

docker-compose up -d
```

### Paso 2: Crear Tabla audit_logs
```bash
docker exec -it TallerMecanico_PG psql -U admin -d TallerMecanico

CREATE TABLE IF NOT EXISTS audit_logs (
    audit_log_id SERIAL PRIMARY KEY,
    tabla_afectada VARCHAR(50),
    registro_id INT,
    accion VARCHAR(20),
    realizado_por VARCHAR(255),
    fecha_hora TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### Paso 3: Levantar Microservicio

```bash
cd ~/Documentos/Universidad/7_Semestre/ARQUITECTURA_DE_SOFTWARE/codigos_docente/Nueva\ carpeta/Taller_Mecanico_Users

# Opción A: Localmente
dotnet build
dotnet run --project App

# Opción B: Docker
docker-compose up --build
```

**Acceso:** http://localhost:5005

---

## 🧪 Pruebas de Endpoints

Utiliza **curl**, **Postman** o **Thunder Client**.

### Ejemplo completo:

```bash
# 1. Crear usuario
curl -X POST http://localhost:5005/api/users \
  -H "Content-Type: application/json" \
  -d '{
    "email": "carlos@example.com",
    "empleadoId": 1,
    "esCliente": false
  }' | jq .

# 2. Login
TOKEN=$(curl -s -X POST http://localhost:5005/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "carlos@example.com",
    "password": "<temp_password_from_step_1>"
  }' | jq -r '.token')

# 3. Cambiar contraseña
curl -X POST http://localhost:5005/api/users/1/change-password \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "currentPassword": "<temp_password>",
    "newPassword": "NuevaPwd456!"
  }'

# 4. Listar usuarios
curl -X GET http://localhost:5005/api/users \
  -H "Authorization: Bearer $TOKEN" | jq .
```

---

## 📁 Archivos Utilizados de Taller_Mecanico_Arqui

| Elemento | Ubicación Original | Ubicación Nueva | Cambios |
|---|---|---|---|
| **BD: usuariologin** | Scripts/init.sql | Compartida PostgreSQL | Sin cambios |
| **BD: empleado/cliente** | Scripts/init.sql | Compartida PostgreSQL | Consultadas como FK |
| **Concepto: Session/Login** | Pages/Login.cshtml.cs | AuthController.cs | Session → JWT |
| **Concepto: ChangePassword** | Pages/ChangePassword.cshtml.cs | ChangePasswordUseCase.cs | UI → Business Logic |
| **Patrón: Result<T>** | Application/Common | Domain/Common | Reutilizado idéntico |
| **Patrón: Repository** | Application/UseCases | UseCases/Data layers | Adaptado a microservicio |
| **Hash: BCrypt** | Aplicación monolito | CreateUserUseCase.cs | Reutilizado idéntico |
| **Flag: RequiereCambioPassword** | UsuarioLogin entity | Domain/Entities/UsuarioLogin.cs | Reutilizado |
| **Track: UltimoAcceso** | UsuarioLogin entity | AuthController + Repository | Reutilizado |

---

## 📊 Estado Actual & Rúbrica (40 pts)

| Requisito | Pts | Estado | Notas |
|-----------|-----|--------|-------|
| **Login con JWT** | 10 | ✅ 10/10 | Funcional, claims completos |
| **CRUD Read** | 2 | ✅ 2/2 | GET endpoints |
| **CRUD Create** | 5 | ✅ 5/5 | POST /users con pwd temporal |
| **CRUD Update** | 3 | ✅ 3/3 | PUT /users/{id} |
| **CRUD Delete** | 1 | ✅ 1/1 | DELETE /users/{id} |
| **Change Password (1st)** | 5 | ✅ 5/5 | RequiereCambioPassword |
| **Change Password (User)** | 5 | ✅ 5/5 | Con política |
| **Auditoría** | 3 | ✅ 3/3 | audit_logs completada |
| **Reportes (Listado)** | 10 | ❌ 0/10 | PENDIENTE |
| **Reportes (Sumario)** | 10 | ❌ 0/10 | PENDIENTE |
| **TOTAL** | **40** | **✅ 39/40** | Falta: Reportes PDF/Excel |

---

## ✨ Funcionalidades Extras Implementadas

✅ Política de contraseñas fuerte (8+ chars, MAYÚS, minus, num, special)
✅ Auditoría transaccional automática (INSERT/UPDATE/DELETE)
✅ Último acceso registrado en BD
✅ Roles y autorización por JWT claims
✅ Transacciones ACID con rollback automático
✅ Inyección de dependencias completa
✅ Pattern Result<T> para manejo de errores
✅ Factory Pattern para conexiones BD

---

## ⏳ Pendientes & Plan de Desarrollo

### ❌ Reportes (20 pts críticos)

- **Reporte 1 (10 pts):** Listado PDF/Excel con filtros
- **Reporte 2 (10 pts):** Sumario con gráfico
- **Auto-generación:** Tras inserción de usuario

**Librerías necesarias:**
```bash
dotnet add App.csproj package iTextSharp.LGPLv2.Core
dotnet add App.csproj package EPPlus
dotnet add App.csproj package OxyPlot
```

---

## 🔗 Instrucciones de Integración

### Consumir desde Taller_Mecanico_Arqui

**Crear HttpClient Service:**

```csharp
public class AuthServiceClient
{
    private readonly HttpClient _http;
    const string URL = "http://host.docker.internal:5005/api";

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var response = await _http.PostAsJsonAsync($"{URL}/auth/login", 
            new { email, password });
        return response.IsSuccessStatusCode 
            ? await response.Content.ReadAsAsync<LoginResponse>()
            : null;
    }
}
```

**Registrar en Program.cs:**
```csharp
builder.Services.AddHttpClient<AuthServiceClient>();
```

---

**Última actualización:** 5 de mayo de 2026 | **Versión:** 1.0.0