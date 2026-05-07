# 📋 ANÁLISIS TÉCNICO - MIGRACIÓN A ARQUITECTURA DE SERVICIOS
## PARTE 2: SERVICIO DE TRANSACCIONES (Hexagonal Architecture)

**Proyecto:** Taller Mecánico - Arquitectura de Software  
**Fecha:** Mayo 6, 2026  
**Versión:** 1.0  
**Alcance:** Análisis completo del dominio, lógica y requisitos del Servicio de Órdenes de Trabajo

---

## 📑 TABLA DE CONTENIDOS

1. [Modelado de Dominio - Transacciones](#1-modelado-de-dominio---transacciones)
2. [Entidades de Órdenes de Trabajo](#2-entidades-de-órdenes-de-trabajo)
3. [Entidades de Catálogos](#3-entidades-de-catálogos)
4. [Enumeraciones de Estados](#4-enumeraciones-de-estados)
5. [Lógica de Negocio - Transacciones](#5-lógica-de-negocio---transacciones)
6. [Casos de Uso de Órdenes](#6-casos-de-uso-de-órdenes)
7. [DTOs de Transacciones](#7-dtos-de-transacciones)
8. [Agregados y Límites de Consistencia](#8-agregados-y-límites-de-consistencia)
9. [Repositorios e Interfaces](#9-repositorios-e-interfaces)
10. [Esquema de Base de Datos](#10-esquema-de-base-de-datos)
11. [Consultas SQL Críticas](#11-consultas-sql-críticas)
12. [Lógica de Inventario (Stock)](#12-lógica-de-inventario-stock)
13. [Facades de Transacciones](#13-facades-de-transacciones)
14. [Páginas Razor - UI de Órdenes](#14-páginas-razor---ui-de-órdenes)
15. [Validaciones y Reglas de Negocio](#15-validaciones-y-reglas-de-negocio)
16. [Campos de Auditoría](#16-campos-de-auditoría)
17. [Especificaciones para Reconstrucción](#17-especificaciones-para-reconstrucción)

---

## 1. MODELADO DE DOMINIO - TRANSACCIONES

### 1.1 Conceptos Principales

```
┌───────────────────────────────────────────────────────────────────┐
│ AGREGADO: ORDEN DE TRABAJO (Raíz Agregado - Hexagonal)           │
├───────────────────────────────────────────────────────────────────┤
│                                                                    │
│  OrdenTrabajo (Root Entity)                                       │
│  ├─ Identidad: OrdenTrabajoId                                    │
│  ├─ Estado: EstadoTrabajo + EstadoPago                           │
│  ├─ Vehículo Referencia: VehiculoId (external)                   │
│  ├─ Servicios: List<OrdenTrabajoServicio>                        │
│  ├─ Productos: List<OrdenTrabajoProducto>                        │
│  ├─ Mecánicos: List<OrdenTrabajoMecanico>                        │
│  ├─ Fotos: List<OrdenTrabajoFoto>                                │
│  ├─ Totales: Total, Subtotal, IVA                                │
│  └─ Auditoría: CreadoPor, ActualizadoPor, FechaCreacion, etc.    │
│                                                                    │
│ Invariantes de Dominio:                                           │
│ • Orden solo puede tener 1 VehiculoId                            │
│ • Estado progresa: Recibido → EnDiagnostico → EnReparacion → ... │
│ • No se puede cambiar estado si hay deudas                       │
│ • Total = SUM(OrdenTrabajoProducto.Subtotal +                    │
│             OrdenTrabajoServicio.Subtotal)                       │
│ • Stock de producto ≥ cantidad requerida                         │
│ • Mecánico solo asignado si estado = EnReparacion                │
│                                                                    │
│ Límites de Consistencia:                                          │
│ • Cambio de EstadoTrabajo: Transacción local                     │
│ • Reducción de Stock: Sincronización con Catálogo                │
│ • Pago: Integración con servicio de Pagos (futuro)               │
│                                                                    │
└───────────────────────────────────────────────────────────────────┘
```

### 1.2 Jerarquía de Vehículos

```
CATÁLOGO MAESTRO:
├─ Marca (PK: MarcaId)
├─ Modelo (PK: ModeloId, FK: MarcaId)
├─ ColorVehiculo (PK: ColorVehiculoId)
└─ Vehículo (PK: VehiculoId, FK: ClienteId, MarcaId, ModeloId, ColorVehiculoId)
   └─ OrdenTrabajo[] (Referencia: VehiculoId)
```

---

## 2. ENTIDADES DE ÓRDENES DE TRABAJO

### 2.1 Entidad: ORDEN TRABAJO (Root Aggregrate)

**Descripción:** Representa un trabajo a realizarse sobre un vehículo.

```csharp
public class OrdenTrabajo : AggregateRoot
{
    // ─────────────────────────────────────────
    // IDENTIDAD Y REFERENCIAS EXTERNAS
    // ─────────────────────────────────────────
    public int OrdenTrabajoId { get; set; }
    public int VehiculoId { get; set; } // Referencia a dominio Vehículos
    
    // ─────────────────────────────────────────
    // INFORMACIÓN TEMPORAL
    // ─────────────────────────────────────────
    public DateTime FechaIngreso { get; set; }
    public DateTime? FechaEntrega { get; set; }
    public DateTime? FechaEstimada { get; set; }
    
    // ─────────────────────────────────────────
    // ESTADOS
    // ─────────────────────────────────────────
    /// <summary>
    /// Estado actual del trabajo en taller
    /// Valores: Recibido, EnDiagnostico, EnReparacion, 
    ///         EnEsperaRepuestos, ListoParaEntrega, Entregado
    /// </summary>
    public EstadoTrabajo EstadoTrabajo { get; set; }
    
    /// <summary>
    /// Estado de pago del trabajo
    /// Valores: Pendiente, Parcial, Pagado, Anulado
    /// </summary>
    public EstadoPago EstadoPago { get; set; }
    
    // ─────────────────────────────────────────
    // DESCRIPCIÓN DEL VEHÍCULO
    // ─────────────────────────────────────────
    /// <summary>
    /// Descripción libre del estado del vehículo
    /// Ejemplo: "Rayones en parachoques, falta espejo lateral"
    /// </summary>
    public string EstadoVehiculo { get; set; }
    
    // ─────────────────────────────────────────
    // CÁLCULOS DE PRECIO
    // ─────────────────────────────────────────
    public decimal Total { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal IVA { get; private set; } // 10% en Paraguay
    
    // ─────────────────────────────────────────
    // COLECCIONES (Entidades Débiles)
    // ─────────────────────────────────────────
    public virtual ICollection<OrdenTrabajoProducto> ProductosUsados 
    { 
        get; 
        private set; 
    } = new List<OrdenTrabajoProducto>();
    
    public virtual ICollection<OrdenTrabajoServicio> ServiciosRealizados 
    { 
        get; 
        private set; 
    } = new List<OrdenTrabajoServicio>();
    
    public virtual ICollection<OrdenTrabajoMecanico> MecanicosAsignados 
    { 
        get; 
        private set; 
    } = new List<OrdenTrabajoMecanico>();
    
    public virtual ICollection<OrdenTrabajoFoto> FotosVehiculo 
    { 
        get; 
        private set; 
    } = new List<OrdenTrabajoFoto>();
    
    // ─────────────────────────────────────────
    // AUDITORÍA
    // ─────────────────────────────────────────
    public string CreadoPor { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public string? ActualizadoPor { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public string? EliminadoPor { get; set; }
    public DateTime? FechaEliminacion { get; set; }
    public bool IsDeleted { get; set; }
    
    // ─────────────────────────────────────────
    // MÉTODOS DE DOMINIO
    // ─────────────────────────────────────────
    
    /// <summary>
    /// Crea una nueva orden de trabajo
    /// </summary>
    public static Result<OrdenTrabajo> Crear(
        int vehiculoId,
        string estadoVehiculo,
        DateTime fechaIngreso,
        EstadoTrabajo estadoInicial = EstadoTrabajo.Recibido)
    {
        // Validar estado inicial válido
        if (estadoInicial != EstadoTrabajo.Recibido)
            return Result<OrdenTrabajo>.Failure(
                "VALIDATION_INVALID_VALUE",
                "Orden debe comenzar en estado 'Recibido'");
        
        return Result<OrdenTrabajo>.Success(
            new OrdenTrabajo
            {
                VehiculoId = vehiculoId,
                EstadoVehiculo = estadoVehiculo,
                FechaIngreso = fechaIngreso,
                EstadoTrabajo = estadoInicial,
                EstadoPago = EstadoPago.Pendiente,
                SubTotal = 0,
                IVA = 0,
                Total = 0
            }
        );
    }
    
    /// <summary>
    /// Añade un producto a la orden
    /// Valida disponibilidad de stock
    /// </summary>
    public Result AgregarProducto(
        int productoId,
        int cantidad,
        decimal precioUnitario)
    {
        // Validar que no existe duplicado
        if (ProductosUsados.Any(p => p.ProductoId == productoId))
            return Result.Failure(
                "VALIDATION_DUPLICATE_VALUE",
                $"Producto {productoId} ya existe en orden");
        
        // Validar cantidad positiva
        if (cantidad <= 0)
            return Result.Failure(
                "VALIDATION_INVALID_VALUE",
                "Cantidad debe ser mayor a 0");
        
        // Validar precio positivo
        if (precioUnitario < 0)
            return Result.Failure(
                "VALIDATION_INVALID_VALUE",
                "Precio debe ser >= 0");
        
        var producto = new OrdenTrabajoProducto
        {
            ProductoId = productoId,
            Cantidad = cantidad,
            PrecioUnitario = precioUnitario,
            Subtotal = cantidad * precioUnitario
        };
        
        ProductosUsados.Add(producto);
        RecalcularTotales();
        
        return Result.Success();
    }
    
    /// <summary>
    /// Añade un servicio a la orden
    /// </summary>
    public Result AgregarServicio(
        int servicioId,
        int cantidad,
        decimal precioUnitario)
    {
        // Validaciones similares a AgregarProducto
        if (ServiciosRealizados.Any(s => s.ServicioId == servicioId))
            return Result.Failure(
                "VALIDATION_DUPLICATE_VALUE",
                $"Servicio {servicioId} ya existe en orden");
        
        if (cantidad <= 0)
            return Result.Failure(
                "VALIDATION_INVALID_VALUE",
                "Cantidad debe ser mayor a 0");
        
        var servicio = new OrdenTrabajoServicio
        {
            ServicioId = servicioId,
            Cantidad = cantidad,
            PrecioUnitario = precioUnitario,
            Subtotal = cantidad * precioUnitario
        };
        
        ServiciosRealizados.Add(servicio);
        RecalcularTotales();
        
        return Result.Success();
    }
    
    /// <summary>
    /// Asigna un mecánico a la orden
    /// Solo permitido si EstadoTrabajo = EnReparacion
    /// </summary>
    public Result AsignarMecanico(int mecanicoId)
    {
        if (EstadoTrabajo != EstadoTrabajo.EnReparacion)
            return Result.Failure(
                "VALIDATION_INVALID_VALUE",
                "Mecánico solo puede asignarse en 'EnReparacion'");
        
        if (MecanicosAsignados.Any(m => m.MecanicoId == mecanicoId))
            return Result.Failure(
                "VALIDATION_DUPLICATE_VALUE",
                "Mecánico ya asignado");
        
        MecanicosAsignados.Add(new OrdenTrabajoMecanico
        {
            MecanicoId = mecanicoId,
            FechaAsignacion = DateTime.UtcNow
        });
        
        return Result.Success();
    }
    
    /// <summary>
    /// Cambia el estado del trabajo
    /// Valida transiciones permitidas
    /// </summary>
    public Result CambiarEstado(EstadoTrabajo nuevoEstado)
    {
        // Matriz de transiciones válidas
        bool transicionValida = (EstadoTrabajo, nuevoEstado) switch
        {
            (EstadoTrabajo.Recibido, EstadoTrabajo.EnDiagnostico) => true,
            (EstadoTrabajo.EnDiagnostico, EstadoTrabajo.EnReparacion) => true,
            (EstadoTrabajo.EnDiagnostico, EstadoTrabajo.EnEsperaRepuestos) => true,
            (EstadoTrabajo.EnReparacion, EstadoTrabajo.ListoParaEntrega) => true,
            (EstadoTrabajo.EnEsperaRepuestos, EstadoTrabajo.EnReparacion) => true,
            (EstadoTrabajo.ListoParaEntrega, EstadoTrabajo.Entregado) => true,
            _ => false
        };
        
        if (!transicionValida)
            return Result.Failure(
                "VALIDATION_INVALID_VALUE",
                $"Transición de {EstadoTrabajo} a {nuevoEstado} inválida");
        
        EstadoTrabajo = nuevoEstado;
        FechaActualizacion = DateTime.UtcNow;
        
        return Result.Success();
    }
    
    /// <summary>
    /// Anula la orden de trabajo
    /// Marca como IsDeleted = true
    /// </summary>
    public Result Anular(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            return Result.Failure(
                "VALIDATION_REQUIRED",
                "Motivo de anulación es requerido");
        
        EstadoPago = EstadoPago.Anulado;
        IsDeleted = true;
        FechaEliminacion = DateTime.UtcNow;
        
        return Result.Success();
    }
    
    /// <summary>
    /// Recalcula subtotal, IVA y total
    /// </summary>
    private void RecalcularTotales()
    {
        decimal productosSub = ProductosUsados.Sum(p => p.Subtotal);
        decimal serviciosSub = ServiciosRealizados.Sum(s => s.Subtotal);
        
        SubTotal = productosSub + serviciosSub;
        IVA = SubTotal * 0.10m; // 10% Paraguay
        Total = SubTotal + IVA;
    }
    
    /// <summary>
    /// Reconstructor (para cargar desde BD)
    /// </summary>
    public static OrdenTrabajo Reconstituir(
        int id,
        int vehiculoId,
        DateTime fechaIngreso,
        DateTime? fechaEntrega,
        EstadoTrabajo estado,
        EstadoPago estadoPago,
        string estadoVehiculo,
        decimal total,
        string creadoPor,
        DateTime fechaCreacion,
        bool isDeleted)
    {
        return new OrdenTrabajo
        {
            OrdenTrabajoId = id,
            VehiculoId = vehiculoId,
            FechaIngreso = fechaIngreso,
            FechaEntrega = fechaEntrega,
            EstadoTrabajo = estado,
            EstadoPago = estadoPago,
            EstadoVehiculo = estadoVehiculo,
            Total = total,
            CreadoPor = creadoPor,
            FechaCreacion = fechaCreacion,
            IsDeleted = isDeleted
        };
    }
}
```

---

### 2.2 Entidad Débil: ORDEN_TRABAJO_PRODUCTO

```csharp
public class OrdenTrabajoProducto
{
    // ─────────────────────────────────────────
    // IDENTIDAD
    // ─────────────────────────────────────────
    public int OrdenTrabajoProductoId { get; set; }
    
    // ─────────────────────────────────────────
    // RELACIONES
    // ─────────────────────────────────────────
    public int OrdenTrabajoId { get; set; }
    public int ProductoId { get; set; }
    
    // ─────────────────────────────────────────
    // DATOS DEL PRODUCTO EN ESTA ORDEN
    // ─────────────────────────────────────────
    /// <summary>
    /// Cantidad utilizada en esta orden
    /// </summary>
    public int Cantidad { get; set; }
    
    /// <summary>
    /// Precio unitario al momento de la orden
    /// (puede diferir del precio actual del catálogo)
    /// </summary>
    public decimal PrecioUnitario { get; set; }
    
    /// <summary>
    /// Cálculo: Cantidad * PrecioUnitario
    /// </summary>
    public decimal Subtotal { get; set; }
    
    // ─────────────────────────────────────────
    // NAVEGACIÓN
    // ─────────────────────────────────────────
    public virtual OrdenTrabajo? OrdenTrabajo { get; set; }
    public virtual Producto? Producto { get; set; }
}
```

**Restricciones BD:**
```sql
CONSTRAINT pk_orden_prod PRIMARY KEY (OrdenTrabajoProductoId)
CONSTRAINT uk_orden_prod UNIQUE (OrdenTrabajoId, ProductoId)
CONSTRAINT fk_orden_trabajo FOREIGN KEY (OrdenTrabajoId) 
    REFERENCES OrdenTrabajo ON DELETE CASCADE
CONSTRAINT fk_producto FOREIGN KEY (ProductoId) 
    REFERENCES Producto ON DELETE RESTRICT
```

### 2.3 Entidad Débil: ORDEN_TRABAJO_SERVICIO

```csharp
public class OrdenTrabajoServicio
{
    public int OrdenTrabajoServicioId { get; set; }
    public int OrdenTrabajoId { get; set; }
    public int ServicioId { get; set; }
    
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    
    public virtual OrdenTrabajo? OrdenTrabajo { get; set; }
    public virtual Servicio? Servicio { get; set; }
}
```

### 2.4 Entidad Débil: ORDEN_TRABAJO_MECANICO

```csharp
public class OrdenTrabajoMecanico
{
    // PK Compuesta
    public int OrdenTrabajoId { get; set; }
    public int MecanicoId { get; set; }
    
    // Datos
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaFinalizacion { get; set; }
    
    // Navegación
    public virtual OrdenTrabajo? OrdenTrabajo { get; set; }
    public virtual Mecanico? Mecanico { get; set; }
}
```

**Restricción:**
```sql
PRIMARY KEY (OrdenTrabajoId, MecanicoId)
```

### 2.5 Entidad Débil: ORDEN_TRABAJO_FOTO

```csharp
public class OrdenTrabajoFoto
{
    public int OrdenTrabajoFotoId { get; set; }
    public int OrdenTrabajoId { get; set; }
    
    /// <summary>
    /// Datos binarios de la foto (JPEG, PNG, etc)
    /// </summary>
    public byte[] Datos { get; set; }
    
    /// <summary>
    /// Content type: image/jpeg, image/png, etc
    /// </summary>
    public string ContentType { get; set; }
    
    /// <summary>
    /// Nombre original del archivo
    /// </summary>
    public string NombreArchivo { get; set; }
    
    /// <summary>
    /// Cuándo se tomó/subió la foto
    /// </summary>
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    
    public virtual OrdenTrabajo? OrdenTrabajo { get; set; }
}
```

**Límite de Tamaño:**
```csharp
const long MAX_FOTO_SIZE = 5 * 1024 * 1024; // 5 MB
```

### 2.6 Tabla Legacy: ORDEN_TRABAJO_CATALOGO

```csharp
public class OrdenTrabajoCatalogo
{
    public int OrdenTrabajoCatalogoId { get; set; }
    public int? OrdenTrabajoId { get; set; }
    public int? ProductoId { get; set; }
    
    public int CantidadUtilizada { get; set; }
    public decimal PrecioUnitario { get; set; }
    
    public DateTime FechaRegistro { get; set; }
}
```

**Nota:** Esta tabla es LEGACY. Los datos deben migrarse a `OrdenTrabajoProducto`.

---

## 3. ENTIDADES DE CATÁLOGOS

### 3.1 Entidad: PRODUCTO

```csharp
public class Producto
{
    public int ProductoId { get; set; }
    
    /// <summary>
    /// Nombre único del producto
    /// </summary>
    public string Nombre { get; set; }
    
    /// <summary>
    /// Descripción detallada
    /// </summary>
    public string? Descripcion { get; set; }
    
    /// <summary>
    /// Precio unitario en el catálogo
    /// </summary>
    public decimal Precio { get; set; }
    
    /// <summary>
    /// Stock disponible en almacén
    /// CRÍTICO: debe sincronizarse con OrdenTrabajoProducto
    /// </summary>
    public int Stock { get; set; }
    
    // Auditoría
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }
    public bool IsDeleted { get; set; }
    
    // Navegación
    public virtual ICollection<OrdenTrabajoProducto> OrdenesProducto 
    { 
        get; 
        set; 
    } = new List<OrdenTrabajoProducto>();
}
```

**Restricciones:**
```sql
CONSTRAINT uk_producto_nombre UNIQUE (Nombre) WHERE IsDeleted = FALSE
CONSTRAINT chk_precio CHECK (Precio >= 0)
CONSTRAINT chk_stock CHECK (Stock >= 0)
```

### 3.2 Entidad: SERVICIO

```csharp
public class Servicio
{
    public int ServicioId { get; set; }
    
    public string Nombre { get; set; }
    public string? Descripcion { get; set; }
    
    /// <summary>
    /// Precio unitario del servicio
    /// Para cambios de aceite, reparaciones específicas, etc.
    /// </summary>
    public decimal Precio { get; set; }
    
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }
    public bool IsDeleted { get; set; }
    
    public virtual ICollection<OrdenTrabajoServicio> OrdenesServicio 
    { 
        get; 
        set; 
    } = new List<OrdenTrabajoServicio>();
}
```

**Restricciones:**
```sql
CONSTRAINT uk_servicio_nombre UNIQUE (Nombre) WHERE IsDeleted = FALSE
CONSTRAINT chk_precio CHECK (Precio >= 0)
```

### 3.3 Entidades: MARCA, MODELO, COLOR_VEHICULO

```csharp
public class Marca
{
    public int MarcaId { get; set; }
    public string Nombre { get; set; } // Toyota, Chevrolet, etc.
    public virtual ICollection<Modelo> Modelos { get; set; }
}

public class Modelo
{
    public int ModeloId { get; set; }
    public int MarcaId { get; set; }
    public string Nombre { get; set; } // Corolla, Optra, etc.
    public virtual Marca? Marca { get; set; }
}

public class ColorVehiculo
{
    public int ColorVehiculoId { get; set; }
    public string Nombre { get; set; } // Rojo, Blanco, Azul, etc.
}
```

---

## 4. ENUMERACIONES DE ESTADOS

### 4.1 EstadoTrabajo

```csharp
public enum EstadoTrabajo
{
    /// <summary>
    /// Vehículo ingresó al taller, en espera de diagnóstico
    /// ↓ siguiente: EnDiagnostico
    /// </summary>
    Recibido = 1,
    
    /// <summary>
    /// Mecánico está diagnosticando el vehículo
    /// ↓ siguiente: EnReparacion | EnEsperaRepuestos
    /// </summary>
    EnDiagnostico = 2,
    
    /// <summary>
    /// Se está realizando la reparación
    /// ↓ siguiente: ListoParaEntrega
    /// </summary>
    EnReparacion = 3,
    
    /// <summary>
    /// Trabajo parado esperando repuestos
    /// ↓ siguiente: EnReparacion
    /// </summary>
    EnEsperaRepuestos = 4,
    
    /// <summary>
    /// Trabajo completado, listo para entregar
    /// ↓ siguiente: Entregado
    /// </summary>
    ListoParaEntrega = 5,
    
    /// <summary>
    /// Vehículo entregado a cliente
    /// Terminal (sin más transiciones)
    /// </summary>
    Entregado = 6
}
```

**Diagrama de Transiciones:**

```
┌──────────┐
│ Recibido │
└────┬─────┘
     │
     ▼
┌──────────────┐
│ EnDiagnostico│
└────┬─────┬───┘
     │     │
     │     └────────────┐
     │                  │
     ▼                  ▼
┌──────────┐      ┌─────────────────┐
│ EnRepara │ ◄────┤ EnEsperaRepuest │
└────┬─────┘      └─────────────────┘
     │
     ▼
┌──────────────┐
│ ListoEntrega │
└────┬─────────┘
     │
     ▼
┌──────────┐
│Entregado │ ◄────── Terminal
└──────────┘
```

### 4.2 EstadoPago

```csharp
public enum EstadoPago
{
    /// <summary>
    /// Aún no se ha efectuado ningún pago
    /// Monto pagado: 0
    /// </summary>
    Pendiente = 1,
    
    /// <summary>
    /// Se ha pagado parcialmente
    /// 0 < Monto pagado < Total
    /// </summary>
    Parcial = 2,
    
    /// <summary>
    /// Totalmente pagado
    /// Monto pagado ≥ Total
    /// </summary>
    Pagado = 3,
    
    /// <summary>
    /// Orden anulada (no se cobrará)
    /// Implica devolver stock y revertir cargos
    /// </summary>
    Anulado = 4
}
```

**Reglas:**
- Transición: Pendiente → Parcial → Pagado
- No puede volver atrás (una vez pagado, pagado)
- Anulado es terminal (sin cambios posteriores)

---

## 5. LÓGICA DE NEGOCIO - TRANSACCIONES

### 5.1 Flujo Completo de Orden de Trabajo

```
┌─────────────────────────────────────────────────────────────┐
│  1. REGISTRO DE VEHÍCULO EN TALLER                          │
├─────────────────────────────────────────────────────────────┤
│ Input:                                                       │
│  • VehiculoId (del cliente)                                 │
│  • FechaIngreso (automática = NOW)                          │
│  • EstadoVehiculo (descripción: "rayones, falta espejo")   │
│  • Fotos antes (opcional)                                   │
│                                                              │
│ Actions:                                                     │
│  ├─ Crear OrdenTrabajo en estado "Recibido"               │
│  ├─ Guardar fotos binarias en BD                           │
│  └─ Notificar a cliente (futura integración)               │
└─────────────────────────────────────────────────────────────┘

                         ▼

┌─────────────────────────────────────────────────────────────┐
│  2. DIAGNÓSTICO POR MECÁNICO                                │
├─────────────────────────────────────────────────────────────┤
│ Input:                                                       │
│  • OrdenTrabajoId                                            │
│  • Cambiar estado a "EnDiagnostico"                         │
│                                                              │
│ Actions:                                                     │
│  ├─ Asignar PRI mecánico disponible (sugerencia)           │
│  ├─ Cambiar estado                                         │
│  └─ Log de cambio de estado                                │
└─────────────────────────────────────────────────────────────┘

                         ▼

┌─────────────────────────────────────────────────────────────┐
│  3. PLAN DE REPARACIÓN                                      │
├─────────────────────────────────────────────────────────────┤
│ Input:                                                       │
│  • Productos requeridos (IDs + cantidades)                 │
│  • Servicios a realizar (IDs + cantidades)                 │
│  • Mecánicos que trabajarán                                │
│                                                              │
│ Validaciones:                                               │
│  ├─ Stock disponible ≥ cantidad requerida     ◄─── CRÍTICO │
│  ├─ Producto/Servicio activo (no eliminado)               │
│  ├─ Cantidad > 0                                          │
│  └─ No duplicados en orden                                │
│                                                              │
│ Actions:                                                     │
│  ├─ Agregar productos a OrdenTrabajoProducto              │
│  ├─ Agregar servicios a OrdenTrabajoServicio              │
│  ├─ RESERVAR stock (no descontar aún)                     │
│  ├─ Calcular Total = SUM(subtotales) + IVA                │
│  └─ Cambiar estado a "EnReparacion"                       │
│                                                              │
│ ⚠️ TRANSACCIÓN CRÍTICA:                                    │
│    Stock debe bloquearse momentáneamente para evitar       │
│    sobreventas en otra orden simultánea                   │
└─────────────────────────────────────────────────────────────┘

                         ▼

┌─────────────────────────────────────────────────────────────┐
│  4. ASIGNACIÓN DE MECÁNICOS                                 │
├─────────────────────────────────────────────────────────────┤
│ Input:                                                       │
│  • Lista de MecanicoIds a asignar                          │
│                                                              │
│ Validaciones:                                               │
│  ├─ Mecánico existe y está Activo                         │
│  ├─ EstadoTrabajo = EnReparacion                          │
│  └─ No duplicados                                          │
│                                                              │
│ Actions:                                                     │
│  ├─ INSERT en OrdenTrabajoMecanico(OrdenId, MecanicoId)   │
│  └─ Guardar FechaAsignacion                               │
└─────────────────────────────────────────────────────────────┘

                         ▼

┌─────────────────────────────────────────────────────────────┐
│  5. EJECUCIÓN Y MONITOREO                                   │
├─────────────────────────────────────────────────────────────┤
│ Estados intermedios:                                         │
│  • EnReparacion: Trabajo en progreso                       │
│  • EnEsperaRepuestos: Pausa por componentes                │
│                                                              │
│ Actions:                                                     │
│  ├─ Actualizar progreso (comentarios, notas)              │
│  ├─ Subir fotos de avance                                 │
│  └─ Cambiar estado según necesidad                        │
│     (EnReparacion ↔ EnEsperaRepuestos)                     │
└─────────────────────────────────────────────────────────────┘

                         ▼

┌─────────────────────────────────────────────────────────────┐
│  6. FINALIZACIÓN DE TRABAJO                                 │
├─────────────────────────────────────────────────────────────┤
│ Validaciones previas:                                       │
│  ├─ Todos los servicios completados                       │
│  ├─ Stock efectivamente descontado                        │
│  ├─ Fotos de entrega capturadas                           │
│  └─ Informe técnico completado                            │
│                                                              │
│ Actions:                                                     │
│  ├─ Cambiar estado a "ListoParaEntrega"                  │
│  ├─ Actualizar FechaEstimada → FechaEntrega              │
│  ├─ CONFIRMAR reducción de stock en catálogo             │
│  └─ Calcular costo final                                 │
│     EstadoPago = Pendiente (listo para pagar)            │
└─────────────────────────────────────────────────────────────┘

                         ▼

┌─────────────────────────────────────────────────────────────┐
│  7. ENTREGA AL CLIENTE                                      │
├─────────────────────────────────────────────────────────────┤
│ Input:                                                       │
│  • Confirmación de retiro                                  │
│  • Firma/Comprobante de cliente                           │
│                                                              │
│ Actions:                                                     │
│  ├─ Cambiar estado a "Entregado"                          │
│  ├─ Registrar FechaEntrega = NOW                          │
│  ├─ Generar Comprobante de Entrega                       │
│  └─ Iniciar proceso de Pago (si aplica)                  │
│     └─ EstadoPago = Pendiente → Pago                     │
│                                                              │
│ 🎯 TERMINAL: Orden completada                             │
└─────────────────────────────────────────────────────────────┘
```

### 5.2 Lógica de Stock (CRÍTICA)

#### Operación: Crear Orden con Productos

```
TRANSACCION (SERIALIZABLE):
+-----------+
| Inicio TX |
+-----------+
    │
    ├─1. SELECT * FROM Producto WHERE id IN (...)
    │       FOR UPDATE  ◄─── LOCK exclusivo
    │
    ├─2. Validar: Stock >= cantidad requerida
    │       ├─ SI: Continuar
    │       └─ NO: ROLLBACK + Error "PRODUCTO_STOCK_INSUFICIENTE"
    │
    ├─3. INSERT INTO OrdenTrabajoProducto (...)
    │
    ├─4. UPDATE Producto SET Stock = Stock - cantidad
    │       ◄─ DESCUENTO INMEDIATO
    │
    ├─5. INSERT INTO OrdenTrabajo (...)
    │
    └─ COMMIT
```

**Pseudocódigo:**

```csharp
// UpdateProductStocks - Facade
public async Task<Result> ExecuteAsync(
    List<(int ProductoId, int Cantidad)> productos)
{
    using (var transaction = await _dbConnection.BeginTransactionAsync())
    {
        try
        {
            foreach (var (productoId, cantidad) in productos)
            {
                // 1. Obtener producto CON LOCK
                var producto = await _repo.GetByIdWithLockAsync(productoId);
                
                // 2. Validar stock
                if (producto.Stock < cantidad)
                    return Result.Failure(
                        "PRODUCTO_STOCK_INSUFICIENTE",
                        $"Stock insuficiente para producto {productoId}");
                
                // 3. Actualizar stock
                producto.Stock -= cantidad;
                await _repo.UpdateAsync(producto);
            }
            
            await transaction.CommitAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result.Failure("DB_ERROR", ex.Message);
        }
    }
}
```

#### Operación: Anular Orden (Revertir Stock)

```
TRANSACCION (SERIALIZABLE):
┌─────────────┐
│ Inicio TX   │
└──────┬──────┘
       │
├─1. SELECT * FROM OrdenTrabajo WHERE id = @id
│
├─2. Para cada OrdenTrabajoProducto:
│    ├─ SELECT Producto FOR UPDATE
│    └─ UPDATE Stock = Stock + cantidad  ◄─ RESTAURAR
│
├─3. UPDATE OrdenTrabajo
│    └─ SET EstadoPago = Anulado, IsDeleted = true
│
└─ COMMIT
```

---

### 5.3 Reglas de Transición de Estados

```csharp
// Máquina de estados
public static bool PuedeTransicionar(
    EstadoTrabajo actual,
    EstadoTrabajo solicitado)
{
    return (actual, solicitado) switch
    {
        // Línea principal
        (EstadoTrabajo.Recibido, EstadoTrabajo.EnDiagnostico) => true,
        (EstadoTrabajo.EnDiagnostico, EstadoTrabajo.EnReparacion) => true,
        (EstadoTrabajo.EnReparacion, EstadoTrabajo.ListoParaEntrega) => true,
        (EstadoTrabajo.ListoParaEntrega, EstadoTrabajo.Entregado) => true,
        
        // Desvíos (esperar repuestos)
        (EstadoTrabajo.EnDiagnostico, EstadoTrabajo.EnEsperaRepuestos) => true,
        (EstadoTrabajo.EnEsperaRepuestos, EstadoTrabajo.EnReparacion) => true,
        
        // Re-diagnósticos (volver atrás)
        (EstadoTrabajo.EnReparacion, EstadoTrabajo.EnDiagnostico) => true,
        
        _ => false
    };
}
```

---

## 6. CASOS DE USO DE ÓRDENES

### 6.1 CreateOrdenTrabajoUseCase

```csharp
public class CreateOrdenTrabajoUseCase
{
    private readonly IOrdenTrabajoRepository _ordenRepo;
    private readonly IProductoRepository _productoRepo;
    private readonly IVehiculoRepository _vehiculoRepo;
    private readonly UpdateProductStocks _updateStocks;
    private readonly AuthenticationHelper _auditHelper;
    
    public async Task<Result<OrdenTrabajoDto>> ExecuteAsync(
        CreateOrdenTrabajoDto request,
        int usuarioId)
    {
        // 1. Validar vehículo existe
        var vehiculo = await _vehiculoRepo.GetByIdAsync(request.VehiculoId);
        if (vehiculo == null)
            return Result<OrdenTrabajoDto>.Failure(
                "VEHICULO_NOT_FOUND",
                $"Vehículo {request.VehiculoId} no encontrado");
        
        // 2. Crear orden en estado inicial
        var ordenResult = OrdenTrabajo.Crear(
            vehiculoId: request.VehiculoId,
            estadoVehiculo: request.EstadoVehiculo,
            fechaIngreso: DateTime.UtcNow
        );
        if (ordenResult.IsFailure)
            return Result<OrdenTrabajoDto>.Failure(
                ordenResult.ErrorCode,
                ordenResult.ErrorMessage);
        
        var orden = ordenResult.Value;
        
        try
        {
            // 3. Procesar productos (con validación de stock)
            foreach (var prod in request.Productos)
            {
                var producto = await _productoRepo.GetByIdAsync(prod.ProductoId);
                if (producto == null)
                    return Result<OrdenTrabajoDto>.Failure(
                        "PRODUCTO_NOT_FOUND",
                        $"Producto {prod.ProductoId} no existe");
                
                // Validar stock
                if (producto.Stock < prod.Cantidad)
                    return Result<OrdenTrabajoDto>.Failure(
                        "PRODUCTO_STOCK_INSUFICIENTE",
                        $"Stock insuficiente para {producto.Nombre}");
                
                // Agregar a orden
                var addResult = orden.AgregarProducto(
                    productoId: prod.ProductoId,
                    cantidad: prod.Cantidad,
                    precioUnitario: producto.Precio
                );
                if (addResult.IsFailure)
                    return Result<OrdenTrabajoDto>.Failure(
                        addResult.ErrorCode,
                        addResult.ErrorMessage);
            }
            
            // 4. Procesar servicios
            foreach (var srv in request.Servicios)
            {
                var servicio = await _servicioRepo.GetByIdAsync(srv.ServicioId);
                if (servicio == null)
                    return Result<OrdenTrabajoDto>.Failure(
                        "SERVICIO_NOT_FOUND",
                        $"Servicio {srv.ServicioId} no existe");
                
                var addResult = orden.AgregarServicio(
                    servicioId: srv.ServicioId,
                    cantidad: srv.Cantidad,
                    precioUnitario: servicio.Precio
                );
                if (addResult.IsFailure)
                    return Result<OrdenTrabajoDto>.Failure(
                        addResult.ErrorCode,
                        addResult.ErrorMessage);
            }
            
            // 5. Guardar orden
            orden.CreadoPor = _auditHelper.GetCurrentAuditActor();
            var ordenGuardada = await _ordenRepo.AddAsync(orden);
            
            // 6. Descontar stock (dentro de transacción)
            var productosParaDescontar = request.Productos
                .Select(p => (p.ProductoId, p.Cantidad))
                .ToList();
            
            var descuentoResult = await _updateStocks.ExecuteAsync(
                productosParaDescontar
            );
            if (descuentoResult.IsFailure)
                return Result<OrdenTrabajoDto>.Failure(
                    descuentoResult.ErrorCode,
                    descuentoResult.ErrorMessage);
            
            return Result<OrdenTrabajoDto>.Success(
                _mapper.Map<OrdenTrabajoDto>(ordenGuardada)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando orden de trabajo");
            return Result<OrdenTrabajoDto>.Failure(
                "DB_INSERT_FAILED",
                "No se pudo crear la orden"
            );
        }
    }
}
```

### 6.2 AnularOrdenTrabajoUseCase

```csharp
public class AnularOrdenTrabajoUseCase
{
    private readonly IOrdenTrabajoRepository _ordenRepo;
    private readonly UpdateProductStocks _updateStocks;
    
    public async Task<Result> ExecuteAsync(int ordnId, string motivo)
    {
        // 1. Obtener orden
        var orden = await _ordenRepo.GetByIdAsync(ordnId);
        if (orden == null)
            return Result.Failure(
                "ORDEN_TRABAJO_NOT_FOUND",
                $"Orden {ordnId} no existe");
        
        if (orden.IsDeleted)
            return Result.Failure(
                "ORDEN_YA_ANULADA",
                "Orden ya fue anulada");
        
        try
        {
            // 2. Anular orden
            var anularResult = orden.Anular(motivo);
            if (anularResult.IsFailure)
                return anularResult;
            
            // 3. Restaurar stock de productos
            var productosOriginales = orden.ProductosUsados
                .Select(p => (p.ProductoId, p.Cantidad))
                .ToList();
            
            var restoreResult = await _updateStocks.RestoreAsync(
                productosOriginales
            );
            if (restoreResult.IsFailure)
                return restoreResult;
            
            // 4. Guardar cambios
            await _ordenRepo.UpdateAsync(orden);
            
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error anulando orden {Id}", ordnId);
            return Result.Failure("DB_ERROR", "No se pudo anular orden");
        }
    }
}
```

### 6.3 CambiarEstadoOrdenUseCase

```csharp
public class CambiarEstadoOrdenUseCase
{
    private readonly IOrdenTrabajoRepository _repo;
    
    public async Task<Result> ExecuteAsync(
        int ordenId,
        EstadoTrabajo nuevoEstado)
    {
        var orden = await _repo.GetByIdAsync(ordenId);
        if (orden == null)
            return Result.Failure(
                "ORDEN_NOT_FOUND",
                "Orden no existe");
        
        // Usar método de dominio para cambiar estado
        var result = orden.CambiarEstado(nuevoEstado);
        if (result.IsFailure)
            return result;
        
        orden.ActualizadoPor = _auditHelper.GetCurrentAuditActor();
        orden.FechaActualizacion = DateTime.UtcNow;
        
        await _repo.UpdateAsync(orden);
        
        return Result.Success();
    }
}
```

---

## 7. DTOs DE TRANSACCIONES

### 7.1 Crear Orden de Trabajo

```csharp
public record CreateOrdenTrabajoDto
{
    [Required]
    public int VehiculoId { get; init; }
    
    [Required]
    [StringLength(500)]
    public string EstadoVehiculo { get; init; }
    
    public DateTime? FechaEstimadaEntrega { get; init; }
    
    /// <summary>
    /// Lista de productos a usar
    /// </summary>
    [Required]
    public List<CreateOrdenProductoDto> Productos { get; init; }
        = new List<CreateOrdenProductoDto>();
    
    /// <summary>
    /// Lista de servicios a realizar
    /// </summary>
    [Required]
    public List<CreateOrdenServicioDto> Servicios { get; init; }
        = new List<CreateOrdenServicioDto>();
    
    /// <summary>
    /// Mecánicos a asignar
    /// </summary>
    public List<int> MecanicoIds { get; init; }
        = new List<int>();
}

public record CreateOrdenProductoDto
{
    [Required]
    public int ProductoId { get; init; }
    
    [Required]
    [Range(1, 1000)]
    public int Cantidad { get; init; }
}

public record CreateOrdenServicioDto
{
    [Required]
    public int ServicioId { get; init; }
    
    [Required]
    [Range(1, 1000)]
    public int Cantidad { get; init; }
}
```

### 7.2 DTOs de Listado y Detalle

```csharp
public record OrdenTrabajoListDto
{
    public int OrdenTrabajoId { get; init; }
    public int VehiculoId { get; init; }
    public string Placa { get; init; }
    public DateTime FechaIngreso { get; init; }
    public string EstadoTrabajo { get; init; }
    public string EstadoPago { get; init; }
    public decimal Total { get; init; }
    public int CantidadProductos { get; init; }
    public int CantidadMecanicos { get; init; }
}

public record OrdenTrabajoDetalleDto
{
    public int OrdenTrabajoId { get; init; }
    public int VehiculoId { get; init; }
    
    // Cliente info (desde vehículo)
    public int ClienteId { get; init; }
    public string ClienteNombre { get; init; }
    public string ClienteEmail { get; init; }
    
    // Vehículo
    public string Marca { get; init; }
    public string Modelo { get; init; }
    public string Placa { get; init; }
    public string Color { get; init; }
    public int Anio { get; init; }
    public string EstadoVehiculo { get; init; }
    
    // Estados y fechas
    public DateTime FechaIngreso { get; init; }
    public DateTime? FechaEstimada { get; init; }
    public DateTime? FechaEntrega { get; init; }
    public EstadoTrabajo EstadoTrabajo { get; init; }
    public EstadoPago EstadoPago { get; init; }
    
    // Detalle
    public List<OrdenProductoDetalleDto> Productos { get; init; }
    public List<OrdenServicioDetalleDto> Servicios { get; init; }
    public List<MecanicoAsignadoDto> Mecanicos { get; init; }
    
    // Totales
    public decimal SubTotal { get; init; }
    public decimal IVA { get; init; }
    public decimal Total { get; init; }
    
    // Auditoría
    public string CreadoPor { get; init; }
    public DateTime FechaCreacion { get; init; }
    public string? ActualizadoPor { get; init; }
}

public record OrdenProductoDetalleDto
{
    public int ProductoId { get; init; }
    public string ProductoNombre { get; init; }
    public int Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
    public decimal Subtotal { get; init; }
}

public record MecanicoAsignadoDto
{
    public int MecanicoId { get; init; }
    public string Nombre { get; init; }
    public string Especialidad { get; init; }
    public DateTime FechaAsignacion { get; init; }
}
```

---

## 8. AGREGADOS Y LÍMITES DE CONSISTENCIA

### 8.1 OrdenTrabajo como Aggregate Root

```
AGREGADO: OrdenTrabajo (Límite de Consistencia)
┌─────────────────────────────────────────────────────┐
│                                                      │
│  OrdenTrabajo (Root Entity)                        │
│  ├─ OrdenTrabajoProducto[] (Entities débiles)      │
│  ├─ OrdenTrabajoServicio[] (Entities débiles)      │
│  ├─ OrdenTrabajoMecanico[] (Entities débiles)      │
│  └─ OrdenTrabajoFoto[] (Value Objects)             │
│                                                      │
│  Límites:                                           │
│  • Cambios atómicos de estado                      │
│  • Cálculos locales de totales                     │
│  • Validación de transiciones                      │
│                                                      │
│  Fuera del Agregado:                               │
│  • Vehículo (referencia externa)                   │
│  • Producto (referencia externa)                   │
│  • Servicio (referencia externa)                   │
│  • Mecanico (referencia externa)                   │
│                                                      │
└─────────────────────────────────────────────────────┘

SINCRONIZACIÓN CON OTROS DOMINIOS:
┌─────────────────────────────────────────────────────┐
│  Cuando OrdenTrabajo cambia:                        │
│                                                      │
│  1. Productos descontados:                         │
│     └─ Event: "OrdenCreada" o "StockDescontado"   │
│        → Dominio Catálogo (Productos)              │
│        → Ejecuta: reducir stock                    │
│                                                      │
│  2. Mecánicos asignados:                           │
│     └─ Event: "MecanicoAsignado"                  │
│        → Dominio Recurso (Empleados)               │
│        → Notifica: carga nueva asignación          │
│                                                      │
│  3. Orden entregada:                               │
│     └─ Event: "OrdenEntregada"                    │
│        → Dominio Facturación (futuro)              │
│        → Genera: factura / recibo                  │
│                                                      │
└─────────────────────────────────────────────────────┘
```

### 8.2 Boundary de Consistencia en Stock

**Problema:** Dos órdenes creadas simultáneamente pueden usar el mismo stock.

**Solución:** Nivel SERIALIZABLE de aislamiento

```sql
-- Configurar aislamiento al máximo
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

BEGIN;

-- LOCK exclusivo
SELECT * FROM Producto WHERE id = @id FOR UPDATE;

-- Validar stock
IF stock < cantidad THEN
    ROLLBACK;
    RAISE EXCEPTION 'Stock insuficiente';
END IF;

-- Descontar
UPDATE Producto SET Stock = Stock - cantidad WHERE id = @id;

COMMIT;
```

---

## 9. REPOSITORIOS E INTERFACES

### 9.1 IOrdenTrabajoRepository

```csharp
public interface IOrdenTrabajoRepository : IRepository<OrdenTrabajo>
{
    /// <summary>
    /// Obtiene orden con todos sus detalles
    /// </summary>
    Task<OrdenTrabajo?> GetByIdWithDetailsAsync(int id);
    
    /// <summary>
    /// Obtiene órdenes por vehículo
    /// </summary>
    Task<List<OrdenTrabajo>> GetByVehiculoIdAsync(int vehiculoId);
    
    /// <summary>
    /// Obtiene órdenes por estado
    /// </summary>
    Task<List<OrdenTrabajo>> GetByEstadoAsync(EstadoTrabajo estado);
    
    /// <summary>
    /// Obtiene órdenes pendientes de pago
    /// </summary>
    Task<List<OrdenTrabajo>> GetByEstadoPagoAsync(EstadoPago estado);
    
    /// <summary>
    /// Busca órdenes en rango de fechas
    /// </summary>
    Task<List<OrdenTrabajo>> GetByFechasAsync(
        DateTime fechaInicio,
        DateTime fechaFin);
    
    /// <summary>
    /// Obtiene métricas de utilización de mecánicos
    /// </summary>
    Task<List<MecanicoUtilizacionDto>> GetMecanicoUtilizacionAsync();
    
    /// <summary>
    /// Obtiene ganancias por período
    /// </summary>
    Task<decimal> GetGananciasPorPeriodoAsync(
        DateTime inicio,
        DateTime fin);
    
    /// <summary>
    /// Marca orden como anulada
    /// </summary>
    Task<bool> SetAnuladoAsync(int id, bool anulado);
}
```

### 9.2 IProductoRepository

```csharp
public interface IProductoRepository : IRepository<Producto>
{
    /// <summary>
    /// Obtiene producto por nombre
    /// </summary>
    Task<Producto?> GetByNombreAsync(string nombre);
    
    /// <summary>
    /// Obtiene productos con bajo stock
    /// </summary>
    Task<List<Producto>> GetBajoStockAsync(int umbral = 5);
    
    /// <summary>
    /// Obtiene producto con LOCK para actualización
    /// </summary>
    Task<Producto?> GetByIdWithLockAsync(int id);
    
    /// <summary>
    /// Descuenta stock atomicamente
    /// </summary>
    Task<Result> DescontarStockAsync(int id, int cantidad);
    
    /// <summary>
    /// Repone stock (para anulaciones)
    /// </summary>
    Task<Result> ReponeStockAsync(int id, int cantidad);
    
    /// <summary>
    /// Obtiene productos más utilizados
    /// </summary>
    Task<List<ProductoEstadisticaDto>> GetMasUtilizadosAsync(
        DateTime desde,
        int top = 10);
}
```

### 9.3 IServicioRepository

```csharp
public interface IServicioRepository : IRepository<Servicio>
{
    Task<Servicio?> GetByNombreAsync(string nombre);
    
    Task<List<ServicioEstadisticaDto>> GetMasRealizadosAsync(
        DateTime desde,
        int top = 10);
}
```

---

## 10. ESQUEMA DE BASE DE DATOS

### 10.1 Tabla: ORDEN_TRABAJO (Principal)

```sql
-- ============================================
-- TABLA: ORDEN_TRABAJO
-- ============================================
CREATE TABLE OrdenTrabajo (
    OrdenTrabajoId BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    
    -- Referencias externas
    VehiculoId BIGINT NOT NULL,
    
    -- Temporal
    FechaIngreso TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaEntrega TIMESTAMP,
    FechaEstimada TIMESTAMP,
    
    -- Estados
    EstadoTrabajo SMALLINT NOT NULL DEFAULT 1, -- 1=Recibido
    EstadoPago SMALLINT NOT NULL DEFAULT 1,    -- 1=Pendiente
    
    -- Descripción  
    EstadoVehiculo TEXT,
    
    -- Montos
    SubTotal NUMERIC(10, 2) NOT NULL DEFAULT 0,
    IVA NUMERIC(10, 2) NOT NULL DEFAULT 0,
    Total NUMERIC(10, 2) NOT NULL DEFAULT 0,
    
    -- Auditoría
    CreadoPor VARCHAR(255) NOT NULL,
    FechaCreacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ActualizadoPor VARCHAR(255),
    FechaActualizacion TIMESTAMP,
    EliminadoPor VARCHAR(255),
    FechaEliminacion TIMESTAMP,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    
    -- Constraints
    CONSTRAINT fk_orden_vehiculo FOREIGN KEY (VehiculoId)
        REFERENCES Vehiculo(VehiculoId) ON DELETE RESTRICT,
    CONSTRAINT chk_estado_trabajo CHECK (EstadoTrabajo IN (1, 2, 3, 4, 5, 6)),
    CONSTRAINT chk_estado_pago CHECK (EstadoPago IN (1, 2, 3, 4)),
    CONSTRAINT chk_total CHECK (Total >= 0),
    CONSTRAINT chk_fecha_entrega CHECK (
        FechaEntrega IS NULL OR FechaEntrega >= FechaIngreso
    )
);

CREATE INDEX idx_orden_vehiculo ON OrdenTrabajo(VehiculoId);
CREATE INDEX idx_orden_estado ON OrdenTrabajo(EstadoTrabajo);
CREATE INDEX idx_orden_pago ON OrdenTrabajo(EstadoPago);
CREATE INDEX idx_orden_fechas ON OrdenTrabajo(FechaIngreso, FechaEntrega);
CREATE INDEX idx_orden_deleted ON OrdenTrabajo(IsDeleted);
```

### 10.2 Tabla: ORDEN_TRABAJO_PRODUCTO

```sql
CREATE TABLE OrdenTrabajoProducto (
    OrdenTrabajoProductoId BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    OrdenTrabajoId BIGINT NOT NULL,
    ProductoId BIGINT NOT NULL,
    
    Cantidad INTEGER NOT NULL CHECK (Cantidad > 0),
    PrecioUnitario NUMERIC(10, 2) NOT NULL CHECK (PrecioUnitario >= 0),
    Subtotal NUMERIC(10, 2) GENERATED ALWAYS AS (Cantidad * PrecioUnitario) STORED,
    
    CONSTRAINT fk_orden_trab FOREIGN KEY (OrdenTrabajoId)
        REFERENCES OrdenTrabajo(OrdenTrabajoId) ON DELETE CASCADE,
    CONSTRAINT fk_producto FOREIGN KEY (ProductoId)
        REFERENCES Producto(ProductoId) ON DELETE RESTRICT,
    CONSTRAINT uk_orden_prod UNIQUE (OrdenTrabajoId, ProductoId)
);

CREATE INDEX idx_orden_prod_orden ON OrdenTrabajoProducto(OrdenTrabajoId);
CREATE INDEX idx_orden_prod_producto ON OrdenTrabajoProducto(ProductoId);
```

### 10.3 Tabla: ORDEN_TRABAJO_SERVICIO

```sql
CREATE TABLE OrdenTrabajoServicio (
    OrdenTrabajoServicioId BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    OrdenTrabajoId BIGINT NOT NULL,
    ServicioId BIGINT NOT NULL,
    
    Cantidad INTEGER NOT NULL CHECK (Cantidad > 0),
    PrecioUnitario NUMERIC(10, 2) NOT NULL CHECK (PrecioUnitario >= 0),
    Subtotal NUMERIC(10, 2) GENERATED ALWAYS AS (Cantidad * PrecioUnitario) STORED,
    
    CONSTRAINT fk_orden_srv FOREIGN KEY (OrdenTrabajoId)
        REFERENCES OrdenTrabajo(OrdenTrabajoId) ON DELETE CASCADE,
    CONSTRAINT fk_servicio FOREIGN KEY (ServicioId)
        REFERENCES Servicio(ServicioId) ON DELETE RESTRICT,
    CONSTRAINT uk_orden_srv UNIQUE (OrdenTrabajoId, ServicioId)
);

CREATE INDEX idx_orden_srv_orden ON OrdenTrabajoServicio(OrdenTrabajoId);
```

### 10.4 Tabla: ORDEN_TRABAJO_MECANICO

```sql
CREATE TABLE OrdenTrabajoMecanico (
    OrdenTrabajoId BIGINT NOT NULL,
    MecanicoId BIGINT NOT NULL,
    
    FechaAsignacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaFinalizacion TIMESTAMP,
    
    PRIMARY KEY (OrdenTrabajoId, MecanicoId),
    CONSTRAINT fk_otm_orden FOREIGN KEY (OrdenTrabajoId)
        REFERENCES OrdenTrabajo(OrdenTrabajoId) ON DELETE CASCADE,
    CONSTRAINT fk_otm_mecanico FOREIGN KEY (MecanicoId)
        REFERENCES Mecanico(MecanicoId) ON DELETE RESTRICT
);

CREATE INDEX idx_otm_mecanico ON OrdenTrabajoMecanico(MecanicoId);
```

### 10.5 Tabla: ORDEN_TRABAJO_FOTO

```sql
CREATE TABLE OrdenTrabajoFoto (
    OrdenTrabajoFotoId BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    OrdenTrabajoId BIGINT NOT NULL,
    
    Datos BYTEA NOT NULL,
    ContentType VARCHAR(50) NOT NULL,
    NombreArchivo VARCHAR(255) NOT NULL,
    FechaRegistro TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT fk_otf_orden FOREIGN KEY (OrdenTrabajoId)
        REFERENCES OrdenTrabajo(OrdenTrabajoId) ON DELETE CASCADE
);

CREATE INDEX idx_otf_orden ON OrdenTrabajoFoto(OrdenTrabajoId);
```

### 10.6 Tabla: PRODUCTO (Catálogo)

```sql
CREATE TABLE Producto (
    ProductoId BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    
    Nombre VARCHAR(255) NOT NULL,
    Descripcion TEXT,
    Precio NUMERIC(10, 2) NOT NULL CHECK (Precio >= 0),
    Stock INTEGER NOT NULL CHECK (Stock >= 0),
    
    FechaCreacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaActualizacion TIMESTAMP,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    
    CONSTRAINT uk_producto_nombre UNIQUE (Nombre) 
        WHERE IsDeleted = FALSE
);

CREATE INDEX idx_producto_stock ON Producto(Stock);
CREATE INDEX idx_producto_deleted ON Producto(IsDeleted);
```

### 10.7 Tabla: SERVICIO (Catálogo)

```sql
CREATE TABLE Servicio (
    ServicioId BIGINT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    
    Nombre VARCHAR(255) NOT NULL,
    Descripcion TEXT,
    Precio NUMERIC(10, 2) NOT NULL CHECK (Precio >= 0),
    
    FechaCreacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FechaActualizacion TIMESTAMP,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    
    CONSTRAINT uk_servicio_nombre UNIQUE (Nombre) 
        WHERE IsDeleted = FALSE
);
```

---

## 11. CONSULTAS SQL CRÍTICAS

### 11.1 Obtener Orden con Todos los Detalles

```sql
SELECT
    -- Orden
    ot.OrdenTrabajoId,
    ot.VehiculoId,
    ot.FechaIngreso,
    ot.FechaEntrega,
    ot.EstadoTrabajo,
    ot.EstadoPago,
    ot.Total,
    
    -- Vehículo
    v.Placa,
    c.ClienteId,
    CONCAT(p.Nombres, ' ', p.PrimerApellido) AS ClienteNombre,
    mar.Nombre AS Marca,
    mod.Nombre AS Modelo,
    
    -- Agregados
    COUNT(DISTINCT otp.OrdenTrabajoProductoId) AS CantidadProductos,
    COUNT(DISTINCT otm.MecanicoId) AS CantidadMecanicos,
    COUNT(DISTINCT otf.OrdenTrabajoFotoId) AS CantidadFotos
    
FROM OrdenTrabajo ot
JOIN Vehiculo v ON ot.VehiculoId = v.VehiculoId
JOIN Cliente c ON v.ClienteId = c.ClienteId
JOIN Persona p ON c.PersonaId = p.PersonaId
JOIN Marca mar ON v.MarcaId = mar.MarcaId
JOIN Modelo mod ON v.ModeloId = mod.ModeloId
LEFT JOIN OrdenTrabajoProducto otp ON ot.OrdenTrabajoId = otp.OrdenTrabajoId
LEFT JOIN OrdenTrabajoMecanico otm ON ot.OrdenTrabajoId = otm.OrdenTrabajoId
LEFT JOIN OrdenTrabajoFoto otf ON ot.OrdenTrabajoId = otf.OrdenTrabajoId
WHERE ot.OrdenTrabajoId = @id
    AND ot.IsDeleted = FALSE
GROUP BY 
    ot.OrdenTrabajoId, ot.VehiculoId, ot.FechaIngreso, ot.FechaEntrega,
    ot.EstadoTrabajo, ot.EstadoPago, ot.Total,
    v.Placa, c.ClienteId,
    p.Nombres, p.PrimerApellido,
    mar.Nombre, mod.Nombre;
```

### 11.2 Órdenes Pendientes por Entregar

```sql
SELECT
    ot.OrdenTrabajoId,
    ot.FechaIngreso,
    v.Placa,
    CONCAT(p.Nombres, ' ', p.PrimerApellido) AS Cliente,
    ot.EstadoTrabajo,
    ot.Total,
    CURRENT_DATE - ot.FechaIngreso::DATE AS DiasEnTaller
FROM OrdenTrabajo ot
JOIN Vehiculo v ON ot.VehiculoId = v.VehiculoId
JOIN Cliente c ON v.ClienteId = c.ClienteId
JOIN Persona p ON c.PersonaId = p.PersonaId
WHERE ot.EstadoTrabajo != 6 -- No Entregado
    AND ot.IsDeleted = FALSE
ORDER BY ot.FechaIngreso ASC;
```

### 11.3 Reporte de Ganancias por Período

```sql
SELECT
    DATE_TRUNC('month', ot.FechaIngreso)::DATE AS Mes,
    COUNT(*) AS TotalOrdenes,
    SUM(ot.SubTotal) AS MontoProductos,
    SUM(ot.IVA) AS TotalIVA,
    SUM(ot.Total) AS TotalMes,
    AVG(ot.Total) AS PromedioOrden
FROM OrdenTrabajo ot
WHERE ot.FechaIngreso >= @fechaInicio
    AND ot.FechaIngreso < @fechaFin
    AND ot.IsDeleted = FALSE
    AND ot.EstadoPago IN (2, 3) -- Parcial o Pagado
GROUP BY DATE_TRUNC('month', ot.FechaIngreso)
ORDER BY Mes DESC;
```

### 11.4 Productos más Utilizados

```sql
SELECT
    p.ProductoId,
    p.Nombre,
    SUM(otp.Cantidad) AS TotalUtilizado,
    SUM(otp.Subtotal) AS MontoTotal,
    AVG(otp.PrecioUnitario) AS PrecioPromedio,
    COUNT(DISTINCT otp.OrdenTrabajoId) AS OrdenesQueUsaron
FROM OrdenTrabajoProducto otp
JOIN Producto p ON otp.ProductoId = p.ProductoId
JOIN OrdenTrabajo ot ON otp.OrdenTrabajoId = ot.OrdenTrabajoId
WHERE ot.FechaIngreso >= @fechaDesde
    AND ot.IsDeleted = FALSE
GROUP BY p.ProductoId, p.Nombre
ORDER BY TotalUtilizado DESC
LIMIT 10;
```

### 11.5 Utilización de Mecánicos

```sql
SELECT
    m.MecanicoId,
    CONCAT(p.Nombres, ' ', p.PrimerApellido) AS Nombre,
    m.Especialidad,
    COUNT(DISTINCT otm.OrdenTrabajoId) AS OrdenesAsignadas,
    COUNT(DISTINCT CASE WHEN otm.FechaFinalizacion IS NOT NULL 
                        THEN otm.OrdenTrabajoId END) AS OrdenesCompletadas,
    AVG(EXTRACT(DAY FROM otm.FechaFinalizacion - otm.FechaAsignacion))
        AS DiaPromedioPorOrden
FROM Mecanico m
JOIN Empleado e ON m.EmpleadoId = e.EmpleadoId
JOIN Persona p ON e.PersonaId = p.PersonaId
LEFT JOIN OrdenTrabajoMecanico otm ON m.MecanicoId = otm.MecanicoId
    AND otm.FechaAsignacion >= @fechaDesde
WHERE e.EstadoLaboral = 1 -- Activo
GROUP BY m.MecanicoId, p.Nombres, p.PrimerApellido, m.Especialidad
ORDER BY OrdenesAsignadas DESC;
```

---

## 12. LÓGICA DE INVENTARIO (STOCK)

### 12.1 Operación: Descontar Stock

**Escenario:** Se crea una orden con productos.

```csharp
public class UpdateProductStocks
{
    private readonly IProductoRepository _productoRepo;
    private readonly IDbConnection _connection;
    
    public async Task<Result> ExecuteAsync(
        List<(int ProductoId, int Cantidad)> productosADescontar)
    {
        using (var transaction = await _connection.BeginTransactionAsync(
            System.Data.IsolationLevel.Serializable))
        {
            try
            {
                foreach (var (productoId, cantidad) in productosADescontar)
                {
                    // 1. Obtener con LOCK
                    var producto = await _productoRepo
                        .GetByIdWithLockAsync(productoId);
                    
                    if (producto == null)
                        return Result.Failure(
                            "PRODUCTO_NOT_FOUND",
                            $"Producto {productoId} no existe");
                    
                    // 2. Validar stock suficiente
                    if (producto.Stock < cantidad)
                        return Result.Failure(
                            "PRODUCTO_STOCK_INSUFICIENTE",
                            $"Stock insuficiente para {producto.Nombre}");
                    
                    // 3. Descontar
                    producto.Stock -= cantidad;
                    await _productoRepo.UpdateAsync(producto);
                }
                
                await transaction.CommitAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result.Failure("DB_ERROR", ex.Message);
            }
        }
    }
    
    public async Task<Result> RestoreAsync(
        List<(int ProductoId, int Cantidad)> productosAReponer)
    {
        using (var transaction = await _connection.BeginTransactionAsync())
        {
            try
            {
                foreach (var (productoId, cantidad) in productosAReponer)
                {
                    var producto = await _productoRepo
                        .GetByIdWithLockAsync(productoId);
                    
                    if (producto == null)
                        return Result.Failure(
                            "PRODUCTO_NOT_FOUND",
                            $"No se puede reponer producto {productoId}");
                    
                    // Reponer
                    producto.Stock += cantidad;
                    await _productoRepo.UpdateAsync(producto);
                }
                
                await transaction.CommitAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result.Failure("DB_ERROR", ex.Message);
            }
        }
    }
}
```

### 12.2 Reporte de Stock Crítico

```csharp
public class StockCriticoReporte
{
    public async Task<List<ProductoStockCriticoDto>> ObtenerAsync()
    {
        const int UMBRAL = 5;
        
        var query = @"
            SELECT 
                ProductoId,
                Nombre,
                Stock,
                Precio,
                (Stock * Precio) AS MontoStock
            FROM Producto
            WHERE Stock <= @umbral
                AND IsDeleted = FALSE
            ORDER BY Stock ASC
        ";
        
        return await _repo.ExecuteQueryAsync<ProductoStockCriticoDto>(
            query,
            new { umbral = UMBRAL });
    }
}
```

---

## 13. FACADES DE TRANSACCIONES

### 13.1 OrdenTrabajoCreate Facade

```csharp
public class OrdenTrabajoCreate
{
    private readonly IOrdenTrabajoRepository _ordenRepo;
    private readonly IVehiculoRepository _vehiculoRepo;
    private readonly IProductoRepository _productoRepo;
    private readonly IServicioRepository _servicioRepo;
    private readonly CreateOrdenTrabajoUseCase _createOT;
    private readonly CambiarEstadoOrdenUseCase _cambiarEstado;
    
    /// <summary>
    /// Obtiene todos los vehículos disponibles para el cliente
    /// </summary>
    public async Task<List<VehiculoLookupDto>> GetAllVehiculosAsync(
        int clienteId)
    {
        var vehiculos = await _vehiculoRepo.GetByClienteIdAsync(clienteId);
        return _mapper.Map<List<VehiculoLookupDto>>(vehiculos);
    }
    
    /// <summary>
    /// Obtiene productos disponibles (con stock > 0)
    /// </summary>
    public async Task<List<ProductoDto>> GetProductosDisponiblesAsync()
    {
        var query = @"
            SELECT ProductoId, Nombre, Precio, Stock
            FROM Producto
            WHERE IsDeleted = FALSE AND Stock > 0
            ORDER BY Nombre
        ";
        
        return await _productoRepo.ExecuteQueryAsync<ProductoDto>(query);
    }
    
    /// <summary>
    /// Obtiene servicios disponibles
    /// </summary>
    public async Task<List<ServicioDto>> GetServiciosDisponiblesAsync()
    {
        var servicios = await _servicioRepo.GetAllAsync();
        return _mapper.Map<List<ServicioDto>>(
            servicios.Where(s => !s.IsDeleted));
    }
    
    /// <summary>
    /// Registra el proceso principal (crear orden + stock)
    /// </summary>
    public async Task<Result<OrdenTrabajoDto>> RegistrarProcesoPrincipalAsync(
        CreateOrdenTrabajoDto request,
        int usuarioId)
    {
        // Usar use case
        return await _createOT.ExecuteAsync(request, usuarioId);
    }
    
    /// <summary>
    /// Opciones para dropdowns
    /// </summary>
    public List<SelectListItem> GetEstadoTrabajoOptions()
    {
        return typeof(EstadoTrabajo)
            .GetEnumValues()
            .Cast<EstadoTrabajo>()
            .Select(e => new SelectListItem
            {
                Value = ((int)e).ToString(),
                Text = e.ToString()
            })
            .ToList();
    }
    
    public List<SelectListItem> GetEstadoPagoOptions()
    {
        return typeof(EstadoPago)
            .GetEnumValues()
            .Cast<EstadoPago>()
            .Select(e => new SelectListItem
            {
                Value = ((int)e).ToString(),
                Text = e.ToString()
            })
            .ToList();
    }
}
```

### 13.2 OrdenTrabajoAnular Facade

```csharp
public class OrdenTrabajoAnular
{
    private readonly AnularOrdenTrabajoUseCase _anularOT;
    private readonly UpdateProductStocks _updateStocks;
    
    public async Task<Result> AnularProcesoPrincipalAsync(
        int ordenId,
        string motivo)
    {
        return await _anularOT.ExecuteAsync(ordenId, motivo);
    }
}
```

---

## 14. PÁGINAS RAZOR - UI DE ÓRDENES

### 14.1 Estructura de Páginas

```
Pages/ordentrabajo/
├─ Index.cshtml                  # Listado + búsqueda
├─ Create.cshtml                 # Crear nueva orden
├─ Edit.cshtml                   # Editar estado y detalles
├─ Detalle.cshtml                # Ver completa con fotos
└─ Anular.cshtml                 # Confirmar anulación
```

### 14.2 Create.cshtml (Formulario Principal)

**Campos:**

```html
<form method="post" asp-page="Create" enctype="multipart/form-data">

    <!-- SECCIÓN 1: Seleccionar Vehículo -->
    <fieldset>
    <legend>Vehículo a Reparar</legend>
    
    <select name="vehiculoId" required id="vehiculoSelect">
        <option value="">-- Seleccionar Vehículo --</option>
        @foreach (var v in Model.Vehiculos)
        {
            <option value="@v.VehiculoId">
                @v.Marca @v.Modelo (@v.Placa)
            </option>
        }
    </select>
    
    (Mostrar: Marca, Modelo, Placa, Color, Año)
    
    </fieldset>

    <!-- SECCIÓN 2: Estado del Vehículo -->
    <fieldset>
    <legend>Descripción Inicial</legend>
    
    <textarea name="estadoVehiculo" 
              placeholder="Describe el estado: rayones, fallas, ruidos..."
              maxlength="500"
              required></textarea>
    
    </fieldset>

    <!-- SECCIÓN 3: Fotos Antes -->
    <fieldset>
    <legend>Fotos del Vehículo (Antes)</legend>
    
    <input type="file" 
           name="fotos" 
           multiple 
           accept="image/*" 
           id="fotosInput" />
    <small>Máximo 5 MB cada una</small>
    
    <div id="fotosPreview"></div>
    
    </fieldset>

    <!-- SECCIÓN 4: PRODUCTOS A UTILIZAR -->
    <fieldset>
    <legend>Productos</legend>
    
    <table id="productosTable">
    <thead>
        <tr>
            <th>Producto</th>
            <th>Stock</th>
            <th>Cantidad</th>
            <th>P.U.</th>
            <th>Subtotal</th>
            <th>Acción</th>
        </tr>
    </thead>
    <tbody id="productosBody">
        <!-- Agregadas dinámicamente -->
    </tbody>
    </table>
    
    <select id="productoSelect" required>
        <option value="">-- Agregar Producto --</optio>
        @foreach (var p in Model.ProductosDisponibles)
        {
            <option value="@p.ProductoId" 
                    data-nombre="@p.Nombre"
                    data-precio="@p.Precio"
                    data-stock="@p.Stock">
                @p.Nombre (Stock: @p.Stock) - $@p.Precio
            </option>
        }
    </select>
    
    <input type="number" 
           id="cantidadProducto" 
           placeholder="Cantidad" 
           min="1" />
    
    <button type="button" id="agregarProductoBtn">
        Agregar Producto
    </button>
    
    </fieldset>

    <!-- SECCIÓN 5: SERVICIOS A REALIZAR -->
    <fieldset>
    <legend>Servicios</legend>
    
    <table id="serviciosTable">
    <thead>
        <tr>
            <th>Servicio</th>
            <th>Cantidad</th>
            <th>P.U.</th>
            <th>Subtotal</th>
            <th>Acción</th>
        </tr>
    </thead>
    <tbody id="serviciosBody">
    </tbody>
    </table>
    
    <select id="servicioSelect" required>
        <option value="">-- Agregar Servicio --</option>
        @foreach (var s in Model.ServiciosDisponibles)
        {
            <option value="@s.ServicioId"
                    data-nombre="@s.Nombre"
                    data-precio="@s.Precio">
                @s.Nombre - $@s.Precio
            </option>
        }
    </select>
    
    <input type="number" 
           id="cantidadServicio" 
           placeholder="Cantidad" 
           min="1" />
    
    <button type="button" id="agregarServicioBtn">
        Agregar Servicio
    </button>
    
    </fieldset>

    <!-- SECCIÓN 6: RESUMEN DE COSTOS -->
    <fieldset>
    <legend>Resumen Económico</legend>
    
    <table id="resumenTable">
    <tr>
        <td>Subtotal:</td>
        <td id="subTotal">$0.00</td>
    </tr>
    <tr>
        <td>IVA (10%):</td>
        <td id="iva">$0.00</td>
    </tr>
    <tr>
        <td><strong>Total:</strong></td>
        <td id="total"><strong>$0.00</strong></td>
    </tr>
    </table>
    
    </fieldset>

    <!-- SECCIÓN 7: ASIGNACIÓN DE MECÁNICOS -->
    <fieldset>
    <legend>Mecánicos Asignados</legend>
    
    <div id="mecanicosCheckboxes">
        @foreach (var m in Model.MecanicosDisponibles)
        {
            <label>
                <input type="checkbox" 
                       name="mecanicoIds" 
                       value="@m.MecanicoId" />
                @m.Nombre - @m.Especialidad
            </label>
        }
    </div>
    
    </fieldset>

    <!-- Botones de Acción -->
    <button type="submit" id="guardarBtn">
        Crear Orden de Trabajo
    </button>
    
    <button type="reset">
        Limpiar
    </button>
    
    <a href="/ordentrabajo" class="btn-secondary">
        Cancelar
    </a>

</form>
```

**JavaScript Dinámico:**

```javascript
// Agregar producto a tabla
document.getElementById('agregarProductoBtn').addEventListener('click', () => {
    const select = document.getElementById('productoSelect');
    const cantidad = document.getElementById('cantidadProducto');
    
    const option = select.options[select.selectedIndex];
    const id = option.value;
    const nombre = option.dataset.nombre;
    const precio = parseFloat(option.dataset.precio);
    const stock = parseInt(option.dataset.stock);
    
    if (!id || !cantidad.value) return;
    if (parseInt(cantidad.value) > stock) {
        alert('Cantidad excedesupone disponible');
        return;
    }
    
    // Agregar fila
    const row = `
        <tr data-producto-id="${id}">
            <td>${nombre}</td>
            <td>${stock}</td>
            <td>${cantidad.value}</td>
            <td>$${precio.toFixed(2)}</td>
            <td>$${(precio * cantidad.value).toFixed(2)}</td>
            <td><button type="button" onclick="this.parentElement.parentElement.remove(); recalcularTotales();">X</button></td>
        </tr>
    `;
    document.getElementById('productosBody').insertAdjacentHTML('beforeend', row);
    
    recalcularTotales();
    select.selectedIndex = 0;
    cantidad.value = '';
});

function recalcularTotales() {
    let subtotal = 0;
    
    // Suma productos
    document.querySelectorAll('#productosBody tr').forEach(row => {
        const subtot = parseFloat(row.querySelectorAll('td')[4].textContent.replace('$', ''));
        subtotal += subtot;
    });
    
    // Suma servicios
    document.querySelectorAll('#serviciosBody tr').forEach(row => {
        const subtot = parseFloat(row.querySelectorAll('td')[3].textContent.replace('$', ''));
        subtotal += subtot;
    });
    
    const iva = subtotal * 0.10;
    const total = subtotal + iva;
    
    document.getElementById('subTotal').textContent = `$${subtotal.toFixed(2)}`;
    document.getElementById('iva').textContent = `$${iva.toFixed(2)}`;
    document.getElementById('total').textContent = `$${total.toFixed(2)}`;
}
```

### 14.3 Index.cshtml (Listado)

**Filtros:**
```html
<form method="get" asp-page="Index">
    
    <select name="estadoTrabajo">
        <option value="">-- Todos los Estados --</option>
        <option value="1">Recibido</option>
        <option value="2">En Diagnóstico</option>
        <option value="3">En Reparación</option>
        <option value="4">Espera Repuestos</option>
        <option value="5">Listo Entrega</option>
        <option value="6">Entregado</option>
    </select>
    
    <select name="estadoPago">
        <option value="">-- Estado Pago --</option>
        <option value="1">Pendiente</option>
        <option value="2">Parcial</option>
        <option value="3">Pagado</option>
    </select>
    
    <input type="text" name="buscar" placeholder="Placa, Cliente..." />
    
    <button type="submit">Filtrar</button>
    <a href="/ordentrabajo">Limpiar</a>
    
</form>
```

**Tabla:**
```html
<table>
<thead>
    <tr>
        <th>Orden ID</th>
        <th>Placa</th>
        <th>Cliente</th>
        <th>Fecha Ingreso</th>
        <th>Estado Trabajo</th>
        <th>Estado Pago</th>
        <th>Total</th>
        <th>Acciones</th>
    </tr>
</thead>
<tbody>
    @foreach (var orden in Model.Ordenes)
    {
        <tr class="estado-@orden.EstadoTrabajo">
            <td><a href="/ordentrabajo/detalle/@orden.OrdenTrabajoId">
                #@orden.OrdenTrabajoId</a></td>
            <td>@orden.Placa</td>
            <td>@orden.ClienteNombre</td>
            <td>@orden.FechaIngreso:d</td>
            <td><span class="badge estado-@orden.EstadoTrabajo">
                @orden.EstadoTrabajo</span></td>
            <td><span class="badge pago-@orden.EstadoPago">
                @orden.EstadoPago</span></td>
            <td>$@orden.Total.ToString("N2")</td>
            <td>
                <a href="/ordentrabajo/edit/@orden.OrdenTrabajoId">
                    Editar
                </a>
                <a href="/ordentrabajo/anular/@orden.OrdenTrabajoId">
                    Anular
                </a>
            </td>
        </tr>
    }
</tbody>
</table>
```

---

## 15. VALIDACIONES Y REGLAS DE NEGOCIO

| Regla | Tipo | Condición | Acción | ErrorCode |
|-------|------|-----------|--------|-----------|
| Stock suficiente | DB | `Producto.Stock >= cantidad` | Rechazar orden | `PRODUCTO_STOCK_INSUFICIENTE` |
| Estado válido | Domain | Transición en matriz | Permitir / Rechazar | `VALIDATION_INVALID_VALUE` |
| Vehículo existe | DB | `EXISTS(Vehiculo)` | Rechazar | `VEHICULO_NOT_FOUND` |
| Producto existe | DB | `EXISTS(Producto)` | Rechazar | `PRODUCTO_NOT_FOUND` |
| Cantidad positiva | Value | `cantidad > 0` | Rechazar | `VALIDATION_INVALID_VALUE` |
| Total recalculado | Domain | `SUM(detalles) + IVA` | Automático | - |
| Mecánico activo | DB | `EstadoLaboral == Activo` | Rechazar asignación | `EMPLEADO_INACTIVO` |
| Foto tamaño | Value | `< 5MB` | Rechazar carga | `VALIDATION_FILE_SIZE_EXCEEDED` |

---

## 16. CAMPOS DE AUDITORÍA

Cada orden registra:
- **CreadoPor:** Email/ID del usuario que creó
- **FechaCreacion:** Timestamp UTC
- **ActualizadoPor:** Email/ID del último que modificó
- **FechaActualizacion:** Timestamp del cambio
- **EliminadoPor:** Email/ID quien anuló
- **FechaEliminacion:** Timestamp de anulación
- **IsDeleted:** Flag soft delete

---

## 17. ESPECIFICACIONES PARA RECONSTRUCCIÓN

### 17.1 Checklist Base de Datos

- [ ] Tabla OrdenTrabajo con constraint de estados
- [ ] Tabla OrdenTrabajoProducto con UNIQUE (Orden, Producto)
- [ ] Tabla OrdenTrabajoServicio
- [ ] Tabla OrdenTrabajoMecanico (PK compuesta)
- [ ] Tabla OrdenTrabajoFoto para binarios
- [ ] Tabla Producto con Stock integer
- [ ] Tabla Servicio con Precio
- [ ] Índices de búsqueda frecuentes
- [ ] Migración de datos legacy (OrdenTrabajoCatalogo)

### 17.2 Checklist Lógica de Dominio

- [ ] OrdenTrabajo aggregate root con validaciones
- [ ] Máquina de estados (EstadoTrabajo)
- [ ] Cálculo automático de totales (Subtotal + IVA)
- [ ] Métodos: AgregarProducto, AsignarMecanico, CambiarEstado, Anular
- [ ] Value Objects: PrecioUnitario, Cantidad
- [ ] Transacciones SERIALIZABLE para stock

### 17.3 Checklist Use Cases

- [ ] CreateOrdenTrabajoUseCase
- [ ] AnularOrdenTrabajoUseCase
- [ ] CambiarEstadoOrdenUseCase
- [ ] GetOrdenDetalleUseCase
- [ ] ListarOrdenesUseCase (con filtros)
- [ ] AsignarMecanicoUseCase

### 17.4 Checklist Facades

- [ ] OrdenTrabajoCreate (crear + stock)
- [ ] OrdenTrabajoAnular (anular + reponer)
- [ ] UpdateProductStocks (transacción crítica)

### 17.5 Checklist Pages (Razor)

- [ ] /ordentrabajo/Create.cshtml (multi-step)
- [ ] /ordentrabajo/Index.cshtml (listado + filtros)
- [ ] /ordentrabajo/Edit.cshtml (estado)
- [ ] /ordentrabajo/Detalle.cshtml (lectura)
- [ ] /ordentrabajo/Anular.cshtml (confirmación)

### 17.6 Checklist Reportes

- [ ] Reporte de Órdenes por Período
- [ ] Reporte de Ganancias (ingresos netos)
- [ ] Reporte de Mecánicos (utilización, carga)
- [ ] Reporte de Productos (más utilizados, stock bajo)
- [ ] Reporte de Clientes (frecuencia, gasto total)

---

## CONCLUSIÓN

**PARTE 2** cubre completamente la lógica transaccional y operativa del taller mecánico. Integrada con PARTE 1 (Users), forma la base sólida para:

1. **Independencia de Servicios:** Cada dominio puede escalar por separado
2. **Consistencia de Datos:** Transacciones ACID garantizadas
3. **Auditoría Completa:** Rastreo de cambios desde creación
4. **Reportes Analíticos:** Datos para decisiones comerciales

**Próximos Pasos:**
- Migrar datos históricos sin pérdidas
- Implementar Event Sourcing (futuro)
- Integración con servicio de Pagos
- API RESTful para clientes externos

---

*Generado: Mayo 6, 2026*  
*Versión: 1.0 - Draft*  
*Estado: Listo para Implementación*
