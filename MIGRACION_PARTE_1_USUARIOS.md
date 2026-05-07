# 📋 ANÁLISIS TÉCNICO - MIGRACIÓN A ARQUITECTURA DE SERVICIOS
## PARTE 1: SERVICIO DE USUARIOS (Clean Architecture)

**Proyecto:** Taller Mecánico - Arquitectura de Software  
**Fecha:** Mayo 6, 2026  
**Versión:** 1.0  
**Alcance:** Análisis completo del dominio, lógica y requisitos del Servicio de Usuarios

---

## 📑 TABLA DE CONTENIDOS

1. [Modelado de Dominio - Usuarios](#1-modelado-de-dominio---usuarios)
2. [Entidades y Value Objects](#2-entidades-y-value-objects)
3. [Enumeraciones](#3-enumeraciones)
4. [Lógica de Negocio - Autenticación](#4-lógica-de-negocio---autenticación)
5. [Casos de Uso de Usuarios](#5-casos-de-uso-de-usuarios)
6. [DTOs del Dominio de Usuarios](#6-dtos-del-dominio-de-usuarios)
7. [Repositorios e Interfaces](#7-repositorios-e-interfaces)
8. [Esquema de Base de Datos](#8-esquema-de-base-de-datos)
9. [Consultas SQL Críticas](#9-consultas-sql-críticas)
10. [Servicios Externos (Adapters)](#10-servicios-externos-adapters)
11. [Páginas Razor - UI de Usuarios](#11-páginas-razor---ui-de-usuarios)
12. [Validaciones y Reglas de Negocio](#12-validaciones-y-reglas-de-negocio)
13. [Campos de Auditoría](#13-campos-de-auditoría)
14. [Mapeo de Patrones de Diseño](#14-mapeo-de-patrones-de-diseño)
15. [Especificaciones para Reconstrucción](#15-especificaciones-para-reconstrucción)

---

## 1. MODELADO DE DOMINIO - USUARIOS

### 1.1 Jerarquía de Entidades de Personas

```
┌─────────────────────────────────────────────────────────────┐
│                  PERSONA (Clase Base Abstracta)              │
│─────────────────────────────────────────────────────────────│
│ PK: PersonaId (IDENTITY)                                    │
│ - NombreCompleto (ValueObject)                              │
│ - DocumentoIdentidad (ValueObject)                          │
│ - Telefono (int)                                            │
│ - Email (string?)                                           │
│ - FechaActualizacion (DateTime?)                            │
│ - IsDeleted (bool, soft delete)                             │
│                                                              │
│ Métodos: Create(), Reconstituir()                           │
└─────────────────────────────────────────────────────────────┘
         ▲                          ▲
         │                          │
    ┌────┴────┐              ┌──────┴──────┐
    │          │              │             │
┌───┴───┐  ┌──┴──┐      ┌─────┴────┐  ┌───┴───┐
│CLIENTE│  │EM..│      │ADMIN     │  │MECAN..│
└───────┘  └─────┘      └──────────┘  └───────┘
    │         │              │            │
    └─────────┴──────────────┴────────────┘
       Todas heredan de PERSONA
```

### 1.2 Entidad: CLIENTE

**Descripción:** Persona que registra vehículos y solicita servicios al taller.

```csharp
public class Cliente : Persona
{
    // PK & Relaciones
    public int ClienteId { get; set; }
    public int? UsuarioLoginId { get; set; }
    
    // Datos específicos
    public DateTime FechaRegistro { get; set; }
    public TipoCliente TipoCliente { get; set; } 
    // Enum: Regular, Frecuente, Corporativo
    
    // Navegación
    public virtual UsuarioLogin? UsuarioLogin { get; set; }
    public virtual ICollection<Vehiculo> Vehiculos { get; set; }
    
    // Factory Methods
    public static Result<Cliente> Crear(
        NombreCompleto nombreCompleto, 
        DocumentoIdentidad documentoIdentidad,
        int telefono, string? email, 
        TipoCliente tipoCliente)
    
    public static Result<Cliente> Reconstituir(
        int clienteId, ... todos los parámetros)
}
```

**Restricciones BD:**
- `DocumentoIdentidad` UNIQUE (Numero + Complemento)
- Soft delete mediante `IsDeleted`
- Índice en (`DocumentoIdentidad`, `IsDeleted`) para búsquedas

**Matriz de Relaciones:**
```
Cliente 1-----(N) Vehiculo
        └─────(1) UsuarioLogin (nullable)
```

---

### 1.3 Entidad: EMPLEADO

**Descripción:** Persona con relación laboral con el taller.

```csharp
public class Empleado : Persona
{
    // PK & Relaciones
    public int EmpleadoId { get; set; }
    public int? UsuarioLoginId { get; set; }
    
    // Datos laborales
    public DateTime FechaContratacion { get; set; }
    public EstadoLaboral EstadoLaboral { get; set; }
    // Enum: Activo, Inactivo, Licencia, Despedido
    
    // Navegación
    public virtual UsuarioLogin? UsuarioLogin { get; set; }
    public virtual ICollection<OrdenTrabajoMecanico>? OrdenesAsignadas { get; set; }
}
```

**Subtipos:**

#### 3.1.1 MECANICO (hereda de EMPLEADO)
```csharp
public class Mecanico : Empleado
{
    public string Especialidad { get; set; }
    // Ejemplo: "Motor", "Transmisión", "Frenos"
    
    public decimal SalarioPorHora { get; set; }
    
    // Relación
    public virtual ICollection<OrdenTrabajoMecanico> 
        OrdenesAsignadas { get; set; }
}
```

#### 3.1.2 ADMINISTRADOR (hereda de EMPLEADO)
```csharp
public class Administrador : Empleado
{
    public decimal SalarioMensual { get; set; }
    
    public NivelAcceso NivelAcceso { get; set; }
    // Enum: Parcial, Completo, Gerente, Cliente
    
    // Permiso para crear/modificar otros admins
    public bool PuedeGestionarAdmins { get; set; }
}
```

**Restricciones BD:**
- `DocumentoIdentidad` UNIQUE por empleado
- `UsuarioLoginId` puede ser NULL
- Índice en `EstadoLaboral` para filtros

---

### 1.4 Entidad: USUARIO_LOGIN

**Descripción:** Credenciales de acceso al sistema. Nexo entre personas y autenticación.

```csharp
public class UsuarioLogin
{
    // PK & FKs
    public int UsuarioLoginId { get; set; }
    public int? EmpleadoId { get; set; }
    public int? ClienteId { get; set; }
    
    // Credenciales
    public string Email { get; set; } // UNIQUE
    public string PasswordHash { get; set; } // BCrypt
    
    // Control de acceso
    public bool Activo { get; set; } 
    public bool RequiereCambioPassword { get; set; }
    
    // Auditoría
    public DateTime? UltimoAcceso { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
    
    // Discriminador
    public bool EsCliente { get; set; }
    
    // Navegación
    public virtual Empleado? Empleado { get; set; }
    public virtual Cliente? Cliente { get; set; }
}
```

**Reglas de Negocio:**
- `Email` es UNIQUE (identificador único de login)
- Solo UNO de `EmpleadoId` o `ClienteId` puede ser NOT NULL
- `PasswordHash` siempre en BCrypt (nunca texto plano)
- `RequiereCambioPassword` = TRUE → fuerza cambio en próximo login
- `UltimoAcceso` se actualiza en cada autenticación exitosa

**Restricciones BD:**
```sql
CONSTRAINT chk_email_format CHECK (Email ~* '^[A-Za-z0-9._%-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}$')
CONSTRAINT chk_usuario_type CHECK (
    (EmpleadoId IS NOT NULL AND ClienteId IS NULL) OR
    (EmpleadoId IS NULL AND ClienteId IS NOT NULL)
)
```

---

## 2. ENTIDADES Y VALUE OBJECTS

### 2.1 VALUE OBJECT: NombreCompleto

**Propósito:** Encapsular y validar estructura de nombres.

```csharp
public record NombreCompleto
{
    const int LONGITUD_MAXIMA = 20;
    
    public string Nombres { get; init; }              // max 20 chars
    public string PrimerApellido { get; init; }      // max 20 chars
    public string? SegundoApellido { get; init; }    // max 20 chars, opcional
    
    /// <summary>
    /// Crea una nueva instancia validada
    /// </summary>
    public static Result<NombreCompleto> Crear(
        string nombres, 
        string primerApellido, 
        string? segundoApellido = null)
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(nombres))
            return Result<NombreCompleto>.Failure(
                "VALIDATION_REQUIRED", 
                "Nombres son requeridos");
        
        if (nombres.Length > LONGITUD_MAXIMA)
            return Result<NombreCompleto>.Failure(
                "VALIDATION_INVALID_VALUE",
                $"Nombres no deben exceder {LONGITUD_MAXIMA} caracteres");
        
        if (string.IsNullOrWhiteSpace(primerApellido))
            return Result<NombreCompleto>.Failure(
                "VALIDATION_REQUIRED",
                "Primer apellido es requerido");
        
        if (primerApellido.Length > LONGITUD_MAXIMA)
            return Result<NombreCompleto>.Failure(
                "VALIDATION_INVALID_VALUE",
                "Primer apellido excede máximo");
        
        if (!string.IsNullOrWhiteSpace(segundoApellido) && 
            segundoApellido.Length > LONGITUD_MAXIMA)
            return Result<NombreCompleto>.Failure(
                "VALIDATION_INVALID_VALUE",
                "Segundo apellido excede máximo");
        
        return Result<NombreCompleto>.Success(
            new NombreCompleto
            {
                Nombres = nombres.Trim(),
                PrimerApellido = primerApellido.Trim(),
                SegundoApellido = segundoApellido?.Trim()
            }
        );
    }
    
    public override string ToString()
    {
        return segundoApellido == null 
            ? $"{primerApellido}, {nombres}"
            : $"{primerApellido} {segundoApellido}, {nombres}";
    }
}
```

**Invariantes:**
- Nunca puede ser null
- Nombres y PrimerApellido obligatorios
- Máximo 20 caracteres cada componente
- Sin caracteres especiales (solo letras y espacios)

---

### 2.2 VALUE OBJECT: DocumentoIdentidad

**Propósito:** Validar y encapsular documento de identificación (Cédula, Pasaporte, etc.)

```csharp
public record DocumentoIdentidad
{
    const int MIN_NUMERO = 100000;
    const int MAX_NUMERO = 99999999;
    
    public int Numero { get; init; }              // 6-8 dígitos
    public string? Complemento { get; init; }     // Ej: "1G", "12K"
    
    /// <summary>
    /// Crea instancia validada de DocumentoIdentidad
    /// Formato: "12345678" o "12345678-1G"
    /// </summary>
    public static Result<DocumentoIdentidad> Crear(string numeroCompleto)
    {
        // Validaciones
        if (string.IsNullOrWhiteSpace(numeroCompleto))
            return Result<DocumentoIdentidad>.Failure(
                "VALIDATION_REQUIRED",
                "Documento de identidad es requerido");
        
        var partes = numeroCompleto.Split('-');
        if (partes.Length > 2)
            return Result<DocumentoIdentidad>.Failure(
                "VALIDATION_INVALID_VALUE",
                "Formato de documento inválido");
        
        if (!int.TryParse(partes[0], out int numero) ||
            numero < MIN_NUMERO || numero > MAX_NUMERO)
            return Result<DocumentoIdentidad>.Failure(
                "VALIDATION_INVALID_VALUE",
                "Número debe ser entre 6 y 8 dígitos");
        
        string? complemento = null;
        if (partes.Length == 2)
        {
            complemento = partes[1].ToUpper();
            
            // Validación: formato "1G" (dígito + mayúscula)
            if (complemento.Length != 2 ||
                !char.IsDigit(complemento[0]) ||
                !char.IsLetter(complemento[1]))
                return Result<DocumentoIdentidad>.Failure(
                    "VALIDATION_INVALID_VALUE",
                    "Complemento debe ser digit+letter (ej: 1G)");
        }
        
        return Result<DocumentoIdentidad>.Success(
            new DocumentoIdentidad
            {
                Numero = numero,
                Complemento = complemento
            }
        );
    }
    
    public override string ToString()
    {
        return complemento == null 
            ? numero.ToString("D8")
            : $"{numero:D8}-{complemento}";
    }
}
```

**Variantes por País:**
- **Paraguay:** Cédula (8 dígitos) con complemento opcional (1G, 2K, etc.)
- Extensible para Pasaporte, Licencia de Conducir

**Restricción de Base de Datos:**
```sql
ALTER TABLE Persona
ADD CONSTRAINT uk_documento_identidad 
    UNIQUE (Numero, Complemento)
    WHERE IsDeleted = 0;
```

---

## 3. ENUMERACIONES

### 3.1 NivelAcceso
Define permisos en el sistema para Administradores.

```csharp
public enum NivelAcceso
{
    /// <summary>
    /// Acceso limitado: Solo puede ver sus datos
    /// - Ver su perfil
    /// - Ver mec nicandidatos asignados (para mecánicos)
    /// - No puede crear/editar otros registros
    /// </summary>
    Parcial = 1,
    
    /// <summary>
    /// Acceso completo a funcionalidad estándar:
    /// - CRUD de clientes, empleados, vehículos
    /// - CRUD de órdenes de trabajo
    /// - Ver reportes básicos
    /// - No puede gestionar admins
    /// </summary>
    Completo = 2,
    
    /// <summary>
    /// Acceso gerencial:
    /// - Todo lo de Completo
    /// - Crear/editar otros administradores
    /// - Ver reportes avanzados
    /// - Auditoría completa
    /// </summary>
    Gerente = 3,
    
    /// <summary>
    /// Acceso cliente: Solo para usuarios Cliente
    /// - Ver sus vehículos
    /// - Ver sus órdenes de trabajo
    /// - Listar servicios/productos disponibles
    /// </summary>
    Cliente = 4
}
```

### 3.2 EstadoLaboral
Estado actual del empleado en la organización.

```csharp
public enum EstadoLaboral
{
    Activo = 1,      // Trabajando actualmente
    Inactivo = 2,    // No está en la nómina
    Licencia = 3,    // Permiso temporal (maternidad, sabático)
    Despedido = 4    // Terminación de relación laboral
}
```

**Comportamiento en Sistema:**
- Solo empleados `Activo` pueden recibir nuevas asignaciones
- No se muestran en búsquedas si están `Inactivo` o `Despedido`
- Pueden mantener órdenes históricas asignadas

### 3.3 TipoCliente
Clasificación para segmentación de clientes.

```csharp
public enum TipoCliente
{
    Regular = 1,      // Cliente ocasional
    Frecuente = 2,    // Cliente que regresa seguido
    Corporativo = 3   // Empresa/flota de vehículos
}
```

**Uso para:**
- Descuentos diferenciados
- Reportes de rentabilidad por segmento
- Restricciones de crédito

---

## 4. LÓGICA DE NEGOCIO - AUTENTICACIÓN

### 4.1 Protocolo de Creación de Usuario (Cliente)

**Flujo Completo:**

```
┌─────────────────────────────────────────────────────────────┐
│   Formulario de Registro CLIENTE                             │
│─────────────────────────────────────────────────────────────│
│ Campos:                                                      │
│ • Nombres ................................ [TextBox]        │
│ • PrimerApellido .......................... [TextBox]        │
│ • SegundoApellido (opcional) ............. [TextBox]        │
│ • Documento de Identidad ................. [TextBox]        │
│ • Teléfono ............................... [TextBox]        │
│ • Email .................................. [TextBox]        │
│ • Tipo de Cliente ........................ [Dropdown]       │
│   └─ Options: Regular, Frecuente, Corporativo             │
│ [Botón: Registrar]                                         │
└─────────────────────────────────────────────────────────────┘

                         ▼

┌─────────────────────────────────────────────────────────────┐
│   VALIDACIONES CLIENT-SIDE (Razor)                          │
│─────────────────────────────────────────────────────────────│
│ ✓ Campos obligatorios no vacíos                             │
│ ✓ Email con formato válido                                 │
│ ✓ Documento: 6-8 dígitos + complemento opcional            │
│ ✓ Teléfono: números solamente                              │
└─────────────────────────────────────────────────────────────┘

                         ▼

┌─────────────────────────────────────────────────────────────┐
│    CreateClienteUseCase (Application layer)                |
│─────────────────────────────────────────────────────────────│
│                                                              │
│ 1. Valida ValueObjects:                                    │
│    • NombreCompleto.Crear(nombres, apellidos)             │
│    • DocumentoIdentidad.Crear(doc + complemento)          │
│                                                              │
│ 2. Verifica no-duplicidad:                                 │
│    • IClienteRepository.ExistsByCiAsync(doc)              │
│    • IUsuarioLoginRepository.ExistsByEmailAsync(email)    │
│                                                              │
│ 3. Genera credenciales temporales:                         │
│    • Crea GUID para token reset password                  │
│    • Genera contraseña temporal: xxxxXXXX123! (12 chars)   │
│       └─ Contiene mayúsculas, minúsculas, números, símbol │
│                                                              │
│ 4. Hashea contraseña:                                      │
│    • BCrypt(plainPassword, salt=12)                        │
│    • Hash almacenado en PasswordHash                       │
│                                                              │
│ 5. Transacción de BD (ATOMICIDAD):                         │
│    BEGIN TRANSACTION                                        │
│    ├─ INSERT INTO Cliente (...)                            │
│    ├─ INSERT INTO UsuarioLogin (...)                       │
│    │  └─ Email, PasswordHash, RequiereCambioPassword=true │
│    └─ COMMIT                                                │
│                                                              │
│ 6. Envía credenciales vía Email:                           │
│    • ICredentialEmailSender.SendCredentialsAsync()         │
│    • Email contiene nombre, email, contraseña temporal    │
│    • Solicita cambio en primer login                      │
│                                                              │
│ 7. Retorna Result<ClienteDto>                             │
└─────────────────────────────────────────────────────────────┘

                         ▼

              ✅ Cliente Creado Exitosamente
         ✉️  Email enviado con credenciales temporales
```

**Validaciones Detalladas:**

| Validación | Tipo | Regla | Código Error |
|---|---|---|---|
| Email obligatorio | ValueRule | `string.IsNullOrWhiteSpace(email) == false` | `VALIDATION_REQUIRED` |
| Email formato | ValueRule | Regex: `^[A-Za-z0-9._%-]+@...` | `VALIDATION_INVALID_VALUE` |
| Email único | DBRule | `COUNT(*) FROM UsuarioLogin WHERE Email = @email` | `USUARIO_EMAIL_DUPLICADO` |
| CI obligatoria | ValueRule | No puede ser null | `VALIDATION_REQUIRED` |
| CI duplicada | DBRule | `COUNT(*) WHERE Numero+Complemento` | `CLIENTE_CI_DUPLICADO` |
| Nombres máx 20 | ValueRule | `length <= 20` | `VALIDATION_INVALID_VALUE` |
| Teléfono válido | ValueRule | `9 dígitos` | `VALIDATION_INVALID_VALUE` |

---

### 4.2 Protocolo de Creación de Usuario (Empleado/Admin)

**Flujo Alternativo para Empleados:**

```
Formulario EMPLEADO
├─ Nombres, Apellidos, Documento, Teléfono (igual que cliente)
├─ Fecha Contratación [DatePicker]
├─ Estado Laboral [Dropdown: Activo/Inactivo/Licencia/Despedido]
├─ Email [TextBox] ◄───────────┐
└─ Tipo Empleado [Dropdown]    │
   ├─ MECANICO                 │
   │  └─ Especialidad [TextBox]  │
   │  └─ Salario/Hora [Decimal]  │
   │                             │
   └─ ADMINISTRADOR ────────────►
      └─ Email OBLIGATORIO
      └─ Salario Mensual [Decimal]
      └─ Nivel Acceso [Dropdown]
         ├─ Parcial
         ├─ Completo
         ├─ Gerente
         └─ Cliente ◄─── Solo si es empleado designado a cliente
      └─ Puede Gestionar Admins [Checkbox]

Validación Extra para ADMIN:
├─ si NivelAcceso = Gerente:
│  └─ Requiere validación por admin existente (PuedeGestionarAdmins = true)
├─ si NivelAcceso != Cliente:
│  └─ Email es OBLIGATORIO
└─ si TipoEmpleado = Mecanico:
   └─ Email es OPCIONAL
```

**Reglas de Negocio:**

```csharp
// REGLA 1: Solo Gerentes pueden crear Administradores
Result requierenPermiso = 
    ValidationHelper.RequireCanCreateAdmin(
        currentUserLevel: usuarioActual.NivelAcceso,
        requestedLevel: empleadoNuevo.NivelAcceso
    );

// REGLA 2: Email obligatorio para Admins no-Cliente
Result requiereEmail = 
    ValidationHelper.ValidateAdminEmail(
        tipoEmpleado: TipoEmpleado.Administrador,
        email: model.Email,
        nivelAcceso: requestedLevel
    );
// Si NivelAcceso != Cliente -> email debe ser NOT NULL
// Si NivelAcceso == Cliente -> email puede ser opcional

// REGLA 3: Cambio de password forzado
usuarioLogin.RequiereCambioPassword = true;
// Middleware ReloquirePasswordChangeMiddleware 
// redirige a /ChangePassword antes de permitir otra acción
```

---

### 4.3 Autenticación (Login)

**Flujo de Autenticación:**

```
LoginForm → Email + Password
           ▼
    ┌─────────────────────────────────────────┐
    │ Búsqueda en BD                          │
    │─────────────────────────────────────────│
    │ SELECT * FROM UsuarioLogin              │
    │ WHERE Email = @email AND Activo = true  │
    └─────────────────────────────────────────┘
           ▼
    ┌─────────────────────────────────────────┐
    │ NO ENCONTRADO:                          │
    │ ├─ Retorna error genérico               │
    │ └─ Log de intento fallido               │
    │                                          │
    │ ENCONTRADO:                             │
    │─────────────────────────────────────────│
    │ BCrypt.Verify(password, PasswordHash)   │
    │  ├─ FALSE → Error + Log intento        │
    │  └─ TRUE → Continúa con autenticación  │
    └─────────────────────────────────────────┘
           ▼
    ┌─────────────────────────────────────────┐
    │ Cargar Contexto de Usuario              │
    │─────────────────────────────────────────│
    │ if (EsCliente):                         │
    │   ├─ GET Cliente WHERE ClienteId       │
    │   ├─ GET TipoCliente                   │
    │   └─ Rol = "Cliente"                   │
    │                                          │
    │ else:                                   │
    │   ├─ GET Empleado WHERE EmpleadoId     │
    │   ├─ GET NivelAcceso                   │
    │   ├─ GET TipoEmpleado                  │
    │   └─ Rol = "Empleado"                  │
    └─────────────────────────────────────────┘
           ▼
    ┌─────────────────────────────────────────┐
    │ Crear ClaimsPrincipal                   │
    │─────────────────────────────────────────│
    │ Claims:                                 │
    │  ├─ NameIdentifier = ClienteId|EmpleId │
    │  ├─ Email = email                      │
    │  ├─ Role = Cliente|Empleado            │
    │  ├─ NivelAcceso = Parcial|Completo|... │
    │  └─ RequiereCambio = bool              │
    └─────────────────────────────────────────┘
           ▼
    ┌─────────────────────────────────────────┐
    │ Cookie Authentication                  │
    │─────────────────────────────────────────│
    │ Crear cookie:                           │
    │  ├─ Name: ".AspNetCore.Identity"       │
    │  ├─ Expires: Now + 8 horas             │
    │  ├─ IsEssential: true                  │
    │  ├─ HttpOnly: true (XSS protection)    │
    │  ├─ Secure: true (HTTPS only)          │
    │  └─ SameSite: Strict (CSRF protection) │
    └─────────────────────────────────────────┘
           ▼
    ┌─────────────────────────────────────────┐
    │ UPDATE UsuarioLogin                     │
    │─────────────────────────────────────────│
    │ SET UltimoAcceso = NOW()                │
    │ WHERE UsuarioLoginId = @id              │
    └─────────────────────────────────────────┘
           ▼
       ✅ AUTENTICADO
       Redirect to /Index o ReturnUrl
```

**Claims Details:**

```csharp
var claims = new List<Claim>
{
    // Identificación única
    new Claim(ClaimTypes.NameIdentifier, empleadoId.ToString()),
    
    // Email (para contacto/auditoría)
    new Claim(ClaimTypes.Email, usuarioLogin.Email),
    
    // Rol genérico
    new Claim(ClaimTypes.Role, esCliente ? "Cliente" : "Empleado"),
    
    // Rol específico en aplicación
    new Claim("NivelAcceso", nivelAcceso.ToString()),
    
    // ID de entidad (para saber quién es)
    new Claim("ClienteId", clienteId?.ToString() ?? "0"),
    new Claim("EmpleadoId", empleadoId?.ToString() ?? "0"),
    
    // Control de cambio de password
    new Claim("RequiereCambio", requiereCambio.ToString())
};
```

---

### 4.4 Manejo de Sesiones y Cookies

**Configuración de Cookie (Program.cs):**

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        // Rutas de autenticación
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccesoDenegado";
        
        // Duración
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true; // Extiende si activo
        
        // Seguridad
        options.Cookie.Name = ".TallerMecanico.Auth";
        options.Cookie.HttpOnly = true;      // No accesible via JS
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.IsEssential = true;   // No requiere consentimiento
        
        // Eventos
        options.Events = new CookieAuthenticationEvents
        {
            OnSigningIn = context =>
            {
                // Log de autenticación
                return Task.CompletedTask;
            },
            OnSignedOut = context =>
            {
                // Limpieza de sesión
                return Task.CompletedTask;
            },
            OnValidatePrincipal = context =>
            {
                // Validación de token en cada solicitud
                return Task.CompletedTask;
            }
        };
    });
```

**Cierre de Sesión (Logout):**

```csharp
public async Task OnGetAsync()
{
    // Invalidar todas las cookies
    await HttpContext.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme
    );
    
    // Log de logout
    _logger.LogInformation(
        "Usuario {Email} logout a las {Time}",
        User.FindFirst(ClaimTypes.Email)?.Value,
        DateTime.UtcNow
    );
    
    // Limpiar cache local si existe
    
    // Redirect a home
    return RedirectToPage("/Index");
}
```

---

## 5. CASOS DE USO DE USUARIOS

### 5.1 CreateClienteUseCase

**Responsabilidad:** Crear un nuevo cliente con usuario login asociado.

```csharp
public class CreateClienteUseCase
{
    private readonly IClienteRepository _clienteRepo;
    private readonly IUsuarioLoginRepository _usuarioRepo;
    private readonly ICredentialEmailSender _emailSender;
    private readonly IClienteValidation _validation;
    
    public async Task<Result<ClienteDto>> ExecuteAsync(
        CreateClienteDto request)
    {
        // 1. Validar ValueObjects
        var nombreResult = NombreCompleto.Crear(
            request.Nombres,
            request.PrimerApellido,
            request.SegundoApellido
        );
        if (nombreResult.IsFailure) return Result<ClienteDto>.Failure(
            nombreResult.ErrorCode, nombreResult.ErrorMessage);
        
        var docResult = DocumentoIdentidad.Crear(
            $"{request.NumeroDocumento}-{request.ComplementoDocumento}"
        );
        if (docResult.IsFailure) return Result<ClienteDto>.Failure(
            docResult.ErrorCode, docResult.ErrorMessage);
        
        // 2. Validar unicidad en BD
        bool ciDuplicate = 
            await _clienteRepo.ExistsByCiAsync(
                docResult.Value.Numero, 
                docResult.Value.Complemento
            );
        if (ciDuplicate) 
            return Result<ClienteDto>.Failure(
                "CLIENTE_CI_DUPLICADO",
                $"Cliente con documento {docResult.Value} ya existe");
        
        // 3. Validar email único
        bool emailDuplicate = 
            await _usuarioRepo.ExistsByEmailAsync(request.Email);
        if (emailDuplicate)
            return Result<ClienteDto>.Failure(
                "USUARIO_EMAIL_DUPLICADO",
                $"Email {request.Email} ya está registrado");
        
        // 4. Crear entidad Cliente
        var clienteResult = Cliente.Crear(
            nombreCompleto: nombreResult.Value,
            documentoIdentidad: docResult.Value,
            telefono: request.Telefono,
            email: request.Email,
            tipoCliente: request.TipoCliente
        );
        if (clienteResult.IsFailure) return Result<ClienteDto>.Failure(
            clienteResult.ErrorCode, clienteResult.ErrorMessage);
        
        var cliente = clienteResult.Value;
        
        // 5. Generar credenciales temporales
        string passwordTemporal = GenerarPassword();
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(
            passwordTemporal, 
            workFactor: 12
        );
        
        // 6. Crear UsuarioLogin (dentro de transacción)
        var usuarioLogin = new UsuarioLogin
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            Activo = true,
            RequiereCambioPassword = true,
            FechaCreacion = DateTime.UtcNow,
            EsCliente = true
        };
        
        try
        {
            // 7. Guardar en BD (transacción)
            cliente = await _clienteRepo.AddAsync(cliente);
            usuarioLogin.ClienteId = cliente.ClienteId;
            usuarioLogin = await _usuarioRepo.AddAsync(usuarioLogin);
            
            cliente.UsuarioLoginId = usuarioLogin.UsuarioLoginId;
            await _clienteRepo.UpdateAsync(cliente);
            
            // 8. Enviar email con credenciales
            await _emailSender.SendCredentialsAsync(
                toEmail: usuarioLogin.Email,
                employeeName: cliente.NombreCompleto.ToString(),
                plainPassword: passwordTemporal
            );
            
            return Result<ClienteDto>.Success(
                _mapper.Map<ClienteDto>(cliente)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando cliente");
            return Result<ClienteDto>.Failure(
                "DB_INSERT_FAILED",
                "No se pudo crear el cliente"
            );
        }
    }
    
    private string GenerarPassword()
    {
        // Formato: xxxxXXXX123! (mayús, minús, números, símbolo)
        var random = new Random();
        string lower = "abcdefghijklmnopqrstuvwxyz";
        string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string digits = "0123456789";
        string symbols = "!@#$%";
        
        var pwd = new StringBuilder();
        pwd.Append(lower[random.Next(lower.Length)]);
        pwd.Append(lower[random.Next(lower.Length)]);
        pwd.Append(lower[random.Next(lower.Length)]);
        pwd.Append(lower[random.Next(lower.Length)]);
        pwd.Append(upper[random.Next(upper.Length)]);
        pwd.Append(upper[random.Next(upper.Length)]);
        pwd.Append(upper[random.Next(upper.Length)]);
        pwd.Append(upper[random.Next(upper.Length)]);
        pwd.Append(digits[random.Next(digits.Length)]);
        pwd.Append(digits[random.Next(digits.Length)]);
        pwd.Append(digits[random.Next(digits.Length)]);
        pwd.Append(symbols[random.Next(symbols.Length)]);
        
        return pwd.ToString();
    }
}
```

### 5.2 GetAllClientesUseCase

```csharp
public class GetAllClientesUseCase
{
    private readonly IClienteRepository _repo;
    
    public async Task<Result<List<ClienteListDto>>> ExecuteAsync(
        Filter? filter = null)
    {
        try
        {
            var clientes = await _repo.GetAllAsync();
            
            if (!string.IsNullOrWhiteSpace(filter?.SearchTerm))
            {
                clientes = clientes
                    .Where(c => c.NombreCompleto.ToString()
                        .Contains(filter.SearchTerm, 
                            StringComparison.OrdinalIgnoreCase)
                        || c.Email.Contains(filter.SearchTerm,
                            StringComparison.OrdinalIgnoreCase)
                    )
                    .ToList();
            }
            
            return Result<List<ClienteListDto>>.Success(
                _mapper.Map<List<ClienteListDto>>(clientes)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo clientes");
            return Result<List<ClienteListDto>>.Failure(
                "DB_ERROR",
                "No se pudieron obtener los clientes"
            );
        }
    }
}
```

---

## 6. DTOs DEL DOMINIO DE USUARIOS

### 6.1 DTOs de Cliente

```csharp
/// <summary>
/// DTO para crear nuevo cliente
/// </summary>
public record CreateClienteDto
{
    [Required(ErrorMessage = "Nombres requeridos")]
    [StringLength(20)]
    public string Nombres { get; init; }
    
    [Required(ErrorMessage = "Primer apellido requerido")]
    [StringLength(20)]
    public string PrimerApellido { get; init; }
    
    [StringLength(20)]
    public string? SegundoApellido { get; init; }
    
    [Required]
    [Range(100000, 99999999)]
    public int NumeroDocumento { get; init; }
    
    public string? ComplementoDocumento { get; init; } // Ej: "1G"
    
    [Required]
    [Phone]
    public int Telefono { get; init; }
    
    [Required]
    [EmailAddress]
    public string Email { get; init; }
    
    [Required]
    public TipoCliente TipoCliente { get; init; }
}

/// <summary>
/// DTO para listados de clientes
/// </summary>
public record ClienteListDto
{
    public int ClienteId { get; init; }
    public string NombreCompleto { get; init; }
    public string DocumentoIdentidad { get; init; }
    public string Email { get; init; }
    public TipoCliente TipoCliente { get; init; }
    public int CantidadVehiculos { get; init; }
    public bool UsuarioActivo { get; init; }
}

/// <summary>
/// DTO para detalle de cliente
/// </summary>
public record ClienteDetalleDto
{
    public int ClienteId { get; init; }
    public string Nombres { get; init; }
    public string PrimerApellido { get; init; }
    public string? SegundoApellido { get; init; }
    public string DocumentoIdentidad { get; init; }
    public string Telefono { get; init; }
    public string Email { get; init; }
    public TipoCliente TipoCliente { get; init; }
    public DateTime FechaRegistro { get; init; }
    public List<VehiculoDto> Vehiculos { get; init; }
}
```

### 6.2 DTOs de Empleado

```csharp
/// <summary>
/// DTO para crear empleado (Mecánico o Admin)
/// </summary>
public record CreateEmpleadoDto
{
    // Datos de Persona
    [Required]
    public string Nombres { get; init; }
    
    [Required]
    public string PrimerApellido { get; init; }
    
    public string? SegundoApellido { get; init; }
    
    [Required]
    public int NumeroDocumento { get; init; }
    
    public string? ComplementoDocumento { get; init; }
    
    [Required]
    [Phone]
    public int Telefono { get; init; }
    
    // Datos de Empleado
    [Required]
    [DataType(DataType.Date)]
    public DateTime FechaContratacion { get; init; }
    
    [Required]
    public EstadoLaboral EstadoLaboral { get; init; }
    
    [Required]
    public TipoEmpleado TipoEmpleado { get; init; }
    
    // Para MECANICO
    public string? Especialidad { get; init; }
    public decimal? SalarioPorHora { get; init; }
    
    // Para ADMINISTRADOR
    public string? Email { get; init; }
    public decimal? SalarioMensual { get; init; }
    public NivelAcceso? NivelAcceso { get; init; }
    public bool? PuedeGestionarAdmins { get; init; }
}

/// <summary>
/// DTO para listar empleados
/// </summary>
public record EmpleadoListDto
{
    public int EmpleadoId { get; init; }
    public string NombreCompleto { get; init; }
    public string DocumentoIdentidad { get; init; }
    public string TipoEmpleado { get; init; } // Mecanico|Administrador
    public EstadoLaboral EstadoLaboral { get; init; }
    public string? Email { get; init; }
    public NivelAcceso? NivelAcceso { get; init; }
}
```

### 6.3 DTOs de UsuarioLogin

```csharp
/// <summary>
/// DTO para cambio de contraseña
/// </summary>
public record ChangePasswordDto
{
    [Required]
    [DataType(DataType.Password)]
    public string PasswordActual { get; init; }
    
    [Required]
    [StringLength(100, MinimumLength = 8)]
    [DataType(DataType.Password)]
    public string PasswordNueva { get; init; }
    
    [DataType(DataType.Password)]
    [Compare("PasswordNueva", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmPassword { get; init; }
}

/// <summary>
/// DTO para reset de contraseña (olvidé)
/// </summary>
public record ResetPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; init; }
}

/// <summary>
/// DTO para confirmación de reset
/// </summary>
public record ConfirmResetDto
{
    [Required]
    public string Token { get; init; }
    
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string NuevaPassword { get; init; }
    
    [Compare("NuevaPassword")]
    public string ConfirmPassword { get; init; }
}
```

---

## 7. REPOSITORIOS E INTERFACES

### 7.1 IClienteRepository

```csharp
public interface IClienteRepository : IRepository<Cliente>
{
    /// <summary>
    /// Verifica si existe cliente con documento específico
    /// </summary>
    Task<bool> ExistsByCiAsync(int numero, string? complemento);
    
    /// <summary>
    /// Obtiene cliente con todas sus órdenes de trabajo
    /// </summary>
    Task<Cliente?> GetByIdWithOrdenesAsync(int clienteId);
    
    /// <summary>
    /// Asocia usuario login a cliente
    /// </summary>
    Task<bool> UpdateUsuarioLoginIdAsync(
        int clienteId, int usuarioLoginId);
}
```

### 7.2 IUsuarioLoginRepository

```csharp
public interface IUsuarioLoginRepository : IRepository<UsuarioLogin>
{
    /// <summary>
    /// Obtiene usuario por email
    /// </summary>
    Task<UsuarioLogin?> GetByEmailAsync(string email);
    
    /// <summary>
    /// Verifica si email existe
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email);
    
    /// <summary>
    /// Obtiene usuarios activos
    /// </summary>
    Task<List<UsuarioLogin>> GetActivosAsync();
    
    /// <summary>
    /// Marca como que requiere cambio de password
    /// </summary>
    Task<bool> RequirePasswordChangeAsync(int usuarioLoginId);
    
    /// <summary>
    /// Actualiza último acceso
    /// </summary>
    Task<bool> UpdateUltimoAccesoAsync(
        int usuarioLoginId, DateTime tiempo);
}
```

### 7.3 IEmpleadoRepository

```csharp
public interface IEmpleadoRepository : IRepository<Empleado>
{
    /// <summary>
    /// Obtiene empleados activos
    /// </summary>
    Task<List<Empleado>> GetActivosAsync();
    
    /// <summary>
    /// Obtiene profesionales específicos por estado
    /// </summary>
    Task<List<Empleado>> GetByEstadoAsync(EstadoLaboral estado);
    
    /// <summary>
    /// Obtiene mecánicos de especialidad
    /// </summary>
    Task<List<Mecanico>> GetMecanicosByEspecialidadAsync(
        string especialidad);
}
```

---

## 8. ESQUEMA DE BASE DE DATOS

### 8.1 Tablas de Personas y Login

```sql
-- ============================================
-- TABLA: PERSONA (Base para herencia)
-- ============================================
CREATE TABLE Persona (
    PersonaId BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    
    -- Nombre
    Nombres VARCHAR(20) NOT NULL,
    PrimerApellido VARCHAR(20) NOT NULL,
    SegundoApellido VARCHAR(20),
    
    -- Documento
    Numero INTEGER NOT NULL,
    Complemento VARCHAR(2),
    
    -- Contacto
    Telefono INTEGER NOT NULL,
    Email VARCHAR(255),
    
    -- Auditoría
    CreadoPor VARCHAR(255),
    FechaCreacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ActualizadoPor VARCHAR(255),
    FechaActualizacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    EliminadoPor VARCHAR(255),
    FechaEliminacion TIMESTAMP,
    
    -- Control
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    
    -- Constraints
    CONSTRAINT uk_numero_complemento UNIQUE (Numero, Complemento) 
        WHERE IsDeleted = FALSE,
    CONSTRAINT chk_email_format CHECK (
        Email IS NULL OR 
        Email ~* '^[A-Za-z0-9._%-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}$'
    )
);

CREATE INDEX idx_persona_ci ON Persona(Numero, Complemento) 
    WHERE IsDeleted = FALSE;
CREATE INDEX idx_persona_deleted ON Persona(IsDeleted);


-- ============================================
-- TABLA: CLIENTE (Hereda de Persona)
-- ============================================
CREATE TABLE Cliente (
    ClienteId BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    PersonaId BIGINT NOT NULL UNIQUE,
    UsuarioLoginId BIGINT,
    
    FechaRegistro TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    TipoCliente SMALLINT NOT NULL, -- 1=Regular, 2=Frecuente, 3=Corporativo
    
    CONSTRAINT fk_cliente_persona FOREIGN KEY (PersonaId)
        REFERENCES Persona(PersonaId) ON DELETE CASCADE,
    CONSTRAINT fk_cliente_usuario FOREIGN KEY (UsuarioLoginId)
        REFERENCES UsuarioLogin(UsuarioLoginId) ON DELETE SET NULL
);

CREATE INDEX idx_cliente_tipo ON Cliente(TipoCliente);
CREATE INDEX idx_cliente_usuario ON Cliente(UsuarioLoginId);


-- ============================================
-- TABLA: EMPLEADO (Hereda de Persona)
-- ============================================
CREATE TABLE Empleado (
    EmpleadoId BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    PersonaId BIGINT NOT NULL UNIQUE,
    UsuarioLoginId BIGINT,
    
    FechaContratacion TIMESTAMP NOT NULL,
    EstadoLaboral SMALLINT NOT NULL, -- 1=Activo, 2=Inactivo, 3=Licencia, 4=Despedido
    
    CONSTRAINT fk_empleado_persona FOREIGN KEY (PersonaId)
        REFERENCES Persona(PersonaId) ON DELETE CASCADE,
    CONSTRAINT fk_empleado_usuario FOREIGN KEY (UsuarioLoginId)
        REFERENCES UsuarioLogin(UsuarioLoginId) ON DELETE SET NULL
);

CREATE INDEX idx_empleado_estado ON Empleado(EstadoLaboral);
CREATE INDEX idx_empleado_usuario ON Empleado(UsuarioLoginId);


-- ============================================
-- TABLA: MECANICO (Hereda de Empleado)
-- ============================================
CREATE TABLE Mecanico (
    MecanicoId BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    EmpleadoId BIGINT NOT NULL UNIQUE,
    
    Especialidad VARCHAR(100) NOT NULL,
    SalarioPorHora NUMERIC(10, 2) NOT NULL,
    
    CONSTRAINT fk_mecanico_empleado FOREIGN KEY (EmpleadoId)
        REFERENCES Empleado(EmpleadoId) ON DELETE CASCADE
);

CREATE INDEX idx_mecanico_especialidad ON Mecanico(Especialidad);


-- ============================================
-- TABLA: ADMINISTRADOR (Hereda de Empleado)
-- ============================================
CREATE TABLE Administrador (
    AdministradorId BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    EmpleadoId BIGINT NOT NULL UNIQUE,
    
    SalarioMensual NUMERIC(10, 2) NOT NULL,
    NivelAcceso SMALLINT NOT NULL, -- 1=Parcial, 2=Completo, 3=Gerente, 4=Cliente
    PuedeGestionarAdmins BOOLEAN NOT NULL DEFAULT FALSE,
    
    CONSTRAINT fk_admin_empleado FOREIGN KEY (EmpleadoId)
        REFERENCES Empleado(EmpleadoId) ON DELETE CASCADE,
    CONSTRAINT chk_nivel_acceso CHECK (NivelAcceso IN (1, 2, 3, 4))
);

CREATE INDEX idx_admin_nivel ON Administrador(NivelAcceso);


-- ============================================
-- TABLA: USUARIO_LOGIN (Credenciales)
-- ============================================
CREATE TABLE UsuarioLogin (
    UsuarioLoginId BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    EmpleadoId BIGINT,
    ClienteId BIGINT,
    
    Email VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL, -- BCrypt
    
    Activo BOOLEAN NOT NULL DEFAULT TRUE,
    RequiereCambioPassword BOOLEAN NOT NULL DEFAULT FALSE,
    
    -- Auditoría
    FechaCreacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaActualizacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UltimoAcceso TIMESTAMP,
    
    -- Control
    EsCliente BOOLEAN NOT NULL DEFAULT FALSE,
    
    CONSTRAINT fk_usuario_empleado FOREIGN KEY (EmpleadoId)
        REFERENCES Empleado(EmpleadoId) ON DELETE CASCADE,
    CONSTRAINT fk_usuario_cliente FOREIGN KEY (ClienteId)
        REFERENCES Cliente(ClienteId) ON DELETE CASCADE,
    CONSTRAINT chk_usuario_type CHECK (
        (EmpleadoId IS NOT NULL AND ClienteId IS NULL) OR
        (EmpleadoId IS NULL AND ClienteId IS NOT NULL)
    ),
    CONSTRAINT chk_email_format CHECK (
        Email ~* '^[A-Za-z0-9._%-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}$'
    )
);

CREATE UNIQUE INDEX uk_usuario_email ON UsuarioLogin(Email);
CREATE INDEX idx_usuario_activo ON UsuarioLogin(Activo);
CREATE INDEX idx_usuario_empleado ON UsuarioLogin(EmpleadoId);
CREATE INDEX idx_usuario_cliente ON UsuarioLogin(ClienteId);
```

---

## 9. CONSULTAS SQL CRÍTICAS

### 9.1 Login - Autenticación

```sql
-- Obtener usuario para autenticación
SELECT
    ul.UsuarioLoginId,
    ul.Email,
    ul.PasswordHash,
    ul.Activo,
    ul.RequiereCambioPassword,
    ul.EsCliente,
    c.ClienteId,
    e.EmpleadoId,
    a.NivelAcceso,
    a.PuedeGestionarAdmins
FROM UsuarioLogin ul
LEFT JOIN Cliente c ON ul.ClienteId = c.ClienteId
LEFT JOIN Empleado e ON ul.EmpleadoId = e.EmpleadoId
LEFT JOIN Administrador a ON e.EmpleadoId = a.EmpleadoId
WHERE ul.Email = @email
    AND ul.Activo = TRUE;
```

### 9.2 Obtener Cliente con Vehículos

```sql
-- Obtener cliente completo con relaciones
SELECT
    -- Cliente
    cli.ClienteId,
    cli.TipoCliente,
    cli.FechaRegistro,
    
    -- Persona (heredado)
    p.Nombres,
    p.PrimerApellido,
    p.SegundoApellido,
    p.Numero AS DocumentoNumero,
    p.Complemento AS DocumentoComplemento,
    p.Telefono,
    p.Email,
    
    -- Vehículos asociados
    v.VehiculoId,
    v.Placa,
    mar.MarcaId,
    mar.Nombre AS MarcaNombre,
    mod.ModeloId,
    mod.Nombre AS ModeloNombre,
    v.Anio,
    col.ColorVehiculoId,
    col.Nombre AS ColorNombre,
    
    -- Órdenes del vehículo
    COUNT(*) FILTER (WHERE ot.OrdenTrabajoId IS NOT NULL)
        AS CantidadOrdenes
        
FROM Cliente cli
JOIN Persona p ON cli.PersonaId = p.PersonaId
LEFT JOIN Vehiculo v ON cli.ClienteId = v.ClienteId
LEFT JOIN Marca mar ON v.MarcaId = mar.MarcaId
LEFT JOIN Modelo mod ON v.ModeloId = mod.ModeloId
LEFT JOIN ColorVehiculo col ON v.ColorVehiculoId = col.ColorVehiculoId
LEFT JOIN OrdenTrabajo ot ON v.VehiculoId = ot.VehiculoId
    AND ot.IsDeleted = FALSE
WHERE cli.ClienteId = @clienteId
    AND cli.IsDeleted = FALSE
GROUP BY
    cli.ClienteId, cli.TipoCliente, cli.FechaRegistro,
    p.PersonaId, p.Nombres, p.PrimerApellido, p.SegundoApellido,
    p.Numero, p.Complemento, p.Telefono, p.Email,
    v.VehiculoId, v.Placa,
    mar.MarcaId, mar.Nombre,
    mod.ModeloId, mod.Nombre,
    v.Anio,
    col.ColorVehiculoId, col.Nombre;
```

### 9.3 Obtener Empleados Activos por Tipo

```sql
-- Mecánicos disponibles
SELECT
    e.EmpleadoId,
    m.MecanicoId,
    p.Nombres,
    p.PrimerApellido,
    m.Especialidad,
    m.SalarioPorHora,
    COUNT(otm.OrdenTrabajoId) AS OrdenesAsignadas
FROM Empleado e
JOIN Mecanico m ON e.EmpleadoId = m.EmpleadoId
JOIN Persona p ON e.PersonaId = p.PersonaId
LEFT JOIN OrdenTrabajoMecanico otm ON m.MecanicoId = otm.MecanicoId
WHERE e.EstadoLaboral = 1 -- Activo
    AND e.IsDeleted = FALSE
GROUP BY e.EmpleadoId, m.MecanicoId, p.Nombres, p.PrimerApellido,
         m.Especialidad, m.SalarioPorHora
ORDER BY OrdenesAsignadas ASC; -- Menos asignados primero
```

### 9.4 Verificar Duplicidad de CI

```sql
-- Verificar si CI existe (para validación de registro)
SELECT COUNT(*) as Existe
FROM Persona
WHERE Numero = @numero
    AND Complemento = @complemento
    AND IsDeleted = FALSE;
```

### 9.5 Verificar Email Único

```sql
-- Verificar si email está disponible
SELECT COUNT(*) as Existe
FROM UsuarioLogin
WHERE Email = @email;
```

---

## 10. SERVICIOS EXTERNOS (ADAPTERS)

### 10.1 ICredentialEmailSender

```csharp
/// <summary>
/// Interfaz (Puerto) para envío de emails
/// Implementación: SmtpCredentialEmailSender
/// </summary>
public interface ICredentialEmailSender
{
    /// <summary>
    /// Envía credenciales al usuario creado
    /// </summary>
    Task<Result> SendCredentialsAsync(
        string toEmail,
        string employeeName,
        string plainPassword);
}

public class SmtpCredentialEmailSender : ICredentialEmailSender
{
    private readonly IOptions<SmtpSettings> _smtpSettings;
    private readonly ILogger<SmtpCredentialEmailSender> _logger;
    
    public async Task<Result> SendCredentialsAsync(
        string toEmail,
        string employeeName,
        string plainPassword)
    {
        try
        {
            var smtpClient = new SmtpClient(_smtpSettings.Value.Host)
            {
                Port = _smtpSettings.Value.Port,
                Credentials = new NetworkCredential(
                    _smtpSettings.Value.Username,
                    _smtpSettings.Value.Password
                ),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = 10000
            };
            
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.Value.From),
                Subject = "Tus Credenciales de Acceso - Taller Mecánico",
                Body = GenerarBodyEmail(employeeName, toEmail, plainPassword),
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);
            
            await smtpClient.SendMailAsync(mailMessage);
            
            _logger.LogInformation(
                "Email de credenciales enviado a {Email}",
                toEmail
            );
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando email a {Email}",
                toEmail);
            return Result.Failure(
                "EMAIL_SEND_FAILED",
                "No se pudo enviar el email de credenciales"
            );
        }
    }
    
    private string GenerarBodyEmail(
        string nombre,
        string email,
        string password)
    {
        return $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Bienvenido al Taller Mecánico</h2>
                <p>Hola {nombre},</p>
                
                <p>Se ha creado tu cuenta de acceso al sistema.
                Tus credenciales son:</p>
                
                <table border='1' cellpadding='10'>
                <tr><td><strong>Email:</strong></td><td>{email}</td></tr>
                <tr><td><strong>Contraseña (Temporal):</strong></td>
                    <td>{password}</td></tr>
                </table>
                
                <p><strong>⚠️ IMPORTANTE:</strong></p>
                <ul>
                    <li>Esta contraseña es temporal</li>
                    <li>Debes cambiarla en tu primer acceso</li>
                    <li>No compartas esta contraseña</li>
                    <li>Si no creaste esta cuenta, 
                        contacta al administrador</li>
                </ul>
                
                <p><a href='https://tallermechanico.com/login'>
                    Acceder al Sistema</a></p>
                
                <hr>
                <p><small>Este es un email automático. 
                No respondes a este email.</small></p>
            </body>
            </html>
        ";
    }
}

public class SmtpSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string From { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}
```

---

## 11. PÁGINAS RAZOR - UI DE USUARIOS

### 11.1 Estructura de Páginas

```
Pages/
├─ Login.cshtml                   # Autenticación
├─ Logout.cshtml                  # Cierre de sesión
├─ ChangePassword.cshtml          # Cambio de contraseña obligatorio
├─ AccesoDenegado.cshtml          # Error de autorización
├─ Usuarios.cshtml                # Gestión de usuarios (admin)
│
├─ Clientes/
│  ├─ Index.cshtml                # Listado de clientes
│  ├─ Create.cshtml               # Crear cliente
│  ├─ Edit.cshtml                 # Editar cliente
│  └─ Perfil.cshtml               # Detalles cliente + vehículos
│
└─ Empleados/
   ├─ Index.cshtml                # Listado de empleados
   ├─ CreateMecanico.cshtml       # Crear mecánico
   ├─ CreateAdmin.cshtml          # Crear administrador
   └─ Edit.cshtml                 # Editar empleado
```

### 11.2 Login.cshtml

**Campos del Formulario:**
```html
<form method="post">
    <!-- Email -->
    <input type="email" 
           name="email" 
           placeholder="correo@taller.com"
           required />
    
    <!-- Contraseña -->
    <input type="password" 
           name="password" 
           placeholder="Contraseña"
           required />
    
    <!-- Recuérdame -->
    <input type="checkbox" 
           name="rememberMe" 
           value="true" />
    <label>Recuérdame</label>
    
    <!-- Botones -->
    <button type="submit">Ingresar</button>
    <a href="/ForgotPassword">¿Olvidaste tu contraseña?</a>
</form>
```

**Validaciones:**
- Email: formato válido
- Contraseña: no vacía
- Response: mensaje genérico si falla (seguridad)

### 11.3 ChangePassword.cshtml

**Campos:**
```html
<form method="post">
    <!-- Contraseña Actual -->
    <input type="password" 
           name="passwordActual" 
           placeholder="Contraseña Actual"
           required />
    
    <!-- Contraseña Nueva -->
    <input type="password" 
           name="passwordNueva" 
           placeholder="Nueva Contraseña"
           minlength="8"
           pattern="^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$"
           required />
    <small>Min. 8 caracteres, mayús, minús, números y símbolo</small>
    
    <!-- Confirmar -->
    <input type="password" 
           name="confirmPassword" 
           placeholder="Confirmar Contraseña"
           required />
    
    <button type="submit">Cambiar Contraseña</button>
</form>
```

**Validaciones:**
- Contraseña actual válida (comparar con hash BCrypt)
- Nueva contraseña ≠ actual
- Cumple requisitos de complejidad
- Confirmación coincide

### 11.4 Clientes/Create.cshtml

**Campos:**
```html
<form method="post" asp-page="Create">

    <!-- SECCIÓN: DATOS PERSONALES -->
    <fieldset>
    <legend>Datos Personales</legend>
    
    <input type="text" 
           name="nombres" 
           placeholder="Nombres"
           maxlength="20"
           required />
    
    <input type="text" 
           name="primerApellido" 
           placeholder="Primer Apellido"
           maxlength="20"
           required />
    
    <input type="text" 
           name="segundoApellido" 
           placeholder="Segundo Apellido (Opcional)"
           maxlength="20" />
    
    <!-- Documento: dos campos -->
    <input type="number" 
           name="numeroDocumento" 
           placeholder="Cédula (ej: 12345678)"
           min="100000"
           max="99999999"
           required />
    
    <input type="text" 
           name="complementoDocumento" 
           placeholder="Complemento (ej: 1G)"
           pattern="[0-9][A-Z]"
           maxlength="2" />
    
    <input type="tel" 
           name="telefono" 
           placeholder="Teléfono"
           pattern="[0-9]{9}"
           required />
    
    <input type="email" 
           name="email" 
           placeholder="Correo Electrónico"
           required />
    
    </fieldset>
    
    <!-- SECCIÓN: TIPO DE CLIENTE -->
    <fieldset>
    <legend>Clasificación</legend>
    
    <select name="tipoCliente" required>
        <option value="">-- Seleccionar --</option>
        <option value="1">Regular</option>
        <option value="2">Frecuente</option>
        <option value="3">Corporativo</option>
    </select>
    
    </fieldset>
    
    <button type="submit">Registrar Cliente</button>
    <a href="/Clientes">Cancelar</a>
</form>
```

**Mensaje de Éxito:**
```html
<div class="alert alert-success">
    Cliente creado exitosamente.
    Email con credenciales temporales enviado a: {email}
</div>
```

---

## 12. VALIDACIONES Y REGLAS DE NEGOCIO

### 12.1 Tabla de Reglas

| Regla | Tipo | Condición | Acción | ErrorCode |
|-------|------|-----------|--------|-----------|
| CI Único | DB | `EXISTS(SELECT * FROM Persona WHERE Numero+Complemento = @ci AND IsDeleted=false)` | Rechazar creación | `CLIENTE_CI_DUPLICADO` |
| Email Único | DB | `EXISTS(SELECT * FROM UsuarioLogin WHERE Email = @email)` | Rechazar | `USUARIO_EMAIL_DUPLICADO` |
| Email Válido | Value | Regex `^[...]+@[...]+\.[...]{2,}$` | Rechazar | `VALIDATION_INVALID_VALUE` |
| Teléfono 9dígit | Value | `length == 9 AND isNumeric` | Rechazar | `VALIDATION_INVALID_VALUE` |
| Password Temporalial | Business | 12 caracteres, mayús, minús, números, símbol | Gen automática | - |
| Password Forzado | Business | `RequiereCambioPassword = true` | Redirect a `/ChangePassword` | - |
| Admin por Gerente | Business | `currentUser.NivelAcceso == Gerente` | Permitir crear Admin | `VALIDATION_INVALID_ACCESS_LEVEL` |
| Email Admin | Business | `tipoEmpleado == Administrador AND nivelAcceso != Cliente` | Email obligatorio | `VALIDATION_ADMIN_EMAIL_REQUIRED` |

### 12.2 Validador Helper

```csharp
public static class ValidationHelper
{
    // Validaciones genéricas
    public static Result Require(
        bool condition,
        string errorCode,
        string message)
    {
        return condition 
            ? Result.Success()
            : Result.Failure(errorCode, message);
    }
    
    public static Result RequireNotNull<T>(
        T? value,
        string errorCode,
        string message) where T : class
    {
        return value != null
            ? Result.Success()
            : Result.Failure(errorCode, message);
    }
    
    // Validaciones de acceso
    public static Result RequireCanCreateAdmin(
        NivelAcceso currentLevel,
        NivelAcceso requestedLevel)
    {
        if (requestedLevel == NivelAcceso.Gerente ||
            requestedLevel == NivelAcceso.Completo)
        {
            return currentLevel == NivelAcceso.Gerente
                ? Result.Success()
                : Result.Failure(
                    "VALIDATION_INVALID_ACCESS_LEVEL",
                    "Solo Gerentes pueden crear administradores");
        }
        
        return Result.Success();
    }
    
    public static Result ValidateAdminEmail(
        TipoEmpleado tipoEmpleado,
        string? email,
        NivelAcceso nivelAcceso)
    {
        if (tipoEmpleado == TipoEmpleado.Administrador &&
            nivelAcceso != NivelAcceso.Cliente)
        {
            return string.IsNullOrWhiteSpace(email)
                ? Result.Failure(
                    "VALIDATION_ADMIN_EMAIL_REQUIRED",
                    "Administradores requieren email")
                : Result.Success();
        }
        
        return Result.Success();
    }
}
```

---

## 13. CAMPOS DE AUDITORÍA

### 13.1 Estructura de Auditoría

Cada entidad principal tiene campos de seguimiento:

```csharp
public class EntidadAuditada
{
    /// <summary>
    /// Usuario que creó el registro (email o ID)
    /// </summary>
    public string CreadoPor { get; set; }
    
    /// <summary>
    /// Marca temporal de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; }
    
    /// <summary>
    /// Último usuario que modificó
    /// </summary>
    public string? ActualizadoPor { get; set; }
    
    /// <summary>
    /// Última modificación
    /// </summary>
    public DateTime? FechaActualizacion { get; set; }
    
    /// <summary>
    /// Usuario que eliminó (soft delete)
    /// </summary>
    public string? EliminadoPor { get; set; }
    
    /// <summary>
    /// Fecha de eliminación lógica
    /// </summary>
    public DateTime? FechaEliminacion { get; set; }
    
    /// <summary>
    /// Flag para soft delete
    /// </summary>
    public bool IsDeleted { get; set; }
}
```

### 13.2 Rellenado de Auditoría en Servicios

```csharp
public class AuditableService<T> where T : EntidadAuditada
{
    public void SetAuditCreation(T entity, string auditUser)
    {
        entity.CreadoPor = auditUser;
        entity.FechaCreacion = DateTime.UtcNow;
    }
    
    public void SetAuditUpdate(T entity, string auditUser)
    {
        entity.ActualizadoPor = auditUser;
        entity.FechaActualizacion = DateTime.UtcNow;
    }
    
    public void SetAuditDeletion(T entity, string auditUser)
    {
        entity.EliminadoPor = auditUser;
        entity.FechaEliminacion = DateTime.UtcNow;
        entity.IsDeleted = true;
    }
}
```

### 13.3 Obtención del Usuario Auditor (AuthenticationHelper)

```csharp
public class AuthenticationHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public string GetCurrentAuditActor()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var email = user?.FindFirst(ClaimTypes.Email)?.Value;
        var id = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        return email ?? id ?? "SYSTEM";
    }
    
    public int? GetCurrentUserEmployeeId()
    {
        var claim = _httpContextAccessor.HttpContext?.User
            .FindFirst("EmpleadoId")?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }
    
    public int? GetCurrentUserClienteId()
    {
        var claim = _httpContextAccessor.HttpContext?.User
            .FindFirst("ClienteId")?.Value;
        return int.TryParse(claim, out var id) ? id : null;
    }
}
```

---

## 14. MAPEO DE PATRONES DE DISEÑO

### 14.1 Repository Pattern
**Aplicación:** Acceso a datos
- `IClienteRepository`, `IEmpleadoRepository`, etc.
- Implementación: `ClienteRepository`, `EmpleadoRepository`, etc.
- **Beneficio:** Abstracción de BD, testeable

### 14.2 Dependency Injection
**Aplicación:** Configuración en `Program.cs`
- Servicios registrados como Scoped/Singleton
- Inyección en constructores
- **Beneficio:** Bajo acoplamiento, reutilización

### 14.3 Factory Method
**Aplicación:** Creación de entidades
- `public static Result<T> Crear(...)`
- `public static Result<T> Reconstituir(...)`
- **Beneficio:** Validación en construcción, no permite inválidos

### 14.4 Value Object
**Aplicación:** `NombreCompleto`, `DocumentoIdentidad`
- Records immutables
- Validación encapsulada
- **Beneficio:** Semántica de dominio, garantías

### 14.5 Result Pattern
**Aplicación:** Manejo de errores
- Success/Failure como Result<T>
- ErrorCodes centralizados
- **Beneficio:** Manejo determinístico, sin excepciones de negocio

### 14.6 Facade Pattern
**Aplicación:** `AuthenticationHelper`
- Unifica lógica de autenticación/sesión
- **Beneficio:** Interfaz simplificada para clientes

### 14.7 Strategy Pattern
**Aplicación:** Repositorios especializados
- Diferentes estrategias de acceso por entidad
- **Beneficio:** Flexibilidad en queries específicas

---

## 15. ESPECIFICACIONES PARA RECONSTRUCCIÓN

### 15.1 Checklist Infrastructure

- [ ] Base de datos PostgreSQL con esquema init.sql
- [ ] Tablas: Persona, Cliente, Empleado, Mecanico, Administrador, UsuarioLogin
- [ ] Value Objects: NombreCompleto, DocumentoIdentidad
- [ ] Result Pattern implementation
- [ ] Repositories con IRepository<T> genérico
- [ ] BCrypt para hash de contraseñas
- [ ] SMTP configurado para envío de emails
- [ ] Autenticación por Cookies
- [ ] Middleware de cambio forzado de password

### 15.2 Checklist Use Cases

- [ ] CreateClienteUseCase
- [ ] CreateEmpleadoUseCase (Mecanico + Admin)
-[ ] GetAllClientesUseCase / Empleados
- [ ] GetClienteByIdUseCase con relaciones
- [ ] UpdateClienteUseCase
- [ ] DeleteClienteUseCase (soft delete)
- [ ] ChangePasswordUseCase
- [ ] LoginUseCase (autenticación)
- [ ] LogoutUseCase

### 15.3 Checklist DTOs

- [ ] CreateClienteDto
- [ ] ClienteListDto, ClienteDetalleDto
- [ ] CreateEmpleadoDto
- [ ] EmpleadoListDto, EmpleadoDetalleDto
- [ ] ChangePasswordDto
- [ ] ResetPasswordDto

### 15.4 Checklist Pages (Razor)

- [ ] Login.cshtml con validaciones
- [ ] Logout.cshtml
- [ ] ChangePassword.cshtml
- [ ] Clientes/Index.cshtml (listado + búsqueda)
- [ ] Clientes/Create.cshtml (formulario)
- [ ] Clientes/Edit.cshtml
- [ ] Empleados/Index.cshtml
- [ ] Empleados/CreateMecanico.cshtml
- [ ] Empleados/CreateAdmin.cshtml
- [ ] AccesoDenegado.cshtml

### 15.5 Checklist Seguridad

- [ ] RequireAccessLevelAttribute en controladores
- [ ] RequirePasswordChangeMiddleware activo
- [ ] HTTPS enforced
- [ ] CSRF tokens en formularios
- [ ] XSS protection (HttpOnly cookies)
- [ ] SQL Injection protection (parameterized queries)
- [ ] Rate limiting en login

---

## CONCLUSIÓN

Este documento proporciona la base técnica completa para:
1. **Migración sin pérdidas:** Toda la lógica de usuarios mapeada
2. **Reconstrucción:** Checklist detallado para nueva arquitectura
3. **Independencia:** El Servicio de Usuarios puede desplegarse autónomamente
4. **Calidad:** Patrones SOLID implementados desde cero

**Próximo paso:** Implementar PARTE 2 - Servicio de Transacciones (Órdenes de Trabajo)

---

*Generado: Mayo 6, 2026*  
*Versión: 1.0 - Draft*  
*Estado: Listo para Revisión*
