# Taller Mecánico – Arquitectura de Servicios

Proyecto de aprendizaje en **.NET 8** para la materia de **Arquitectura de Software** en la universidad.  
Se implementa una API RESTful para la gestión de un taller mecánico, aplicando el patrón **Arquitectura en Capas** con **Inyección de Dependencias** y una **Capa de Servicios**.

---

## 🏗️ Estructura de la solución

```
TallerMecanico/
├── src/
│   ├── TallerMecanico.Core/        # Capa de dominio: modelos e interfaces
│   │   ├── Models/
│   │   │   ├── Cliente.cs
│   │   │   ├── Vehiculo.cs
│   │   │   ├── Servicio.cs
│   │   │   └── OrdenTrabajo.cs
│   │   └── Interfaces/
│   │       ├── IClienteService.cs
│   │       ├── IVehiculoService.cs
│   │       ├── IServicioService.cs
│   │       └── IOrdenTrabajoService.cs
│   ├── TallerMecanico.Services/    # Capa de servicios: lógica de negocio
│   │   ├── ClienteService.cs
│   │   ├── VehiculoService.cs
│   │   ├── ServicioService.cs
│   │   └── OrdenTrabajoService.cs
│   └── TallerMecanico.API/         # Capa de presentación: controladores REST
│       └── Controllers/
│           ├── ClientesController.cs
│           ├── VehiculosController.cs
│           ├── ServiciosController.cs
│           └── OrdenesTrabajoController.cs
└── tests/
    └── TallerMecanico.Tests/       # Pruebas unitarias (xUnit)
```

---

## 🚀 Cómo ejecutar

### Requisitos
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Ejecutar la API

```bash
dotnet run --project src/TallerMecanico.API
```

La API estará disponible en `https://localhost:7137` (HTTPS) o `http://localhost:5290` (HTTP).  
La interfaz Swagger UI se abre en `/swagger`.

### Ejecutar las pruebas

```bash
dotnet test tests/TallerMecanico.Tests
```

---

## 📦 Entidades del dominio

| Entidad        | Descripción                                      |
|----------------|--------------------------------------------------|
| **Cliente**    | Persona que lleva su vehículo al taller          |
| **Vehiculo**   | Automóvil registrado a nombre de un cliente      |
| **Servicio**   | Tipo de trabajo que realiza el taller (catálogo) |
| **OrdenTrabajo** | Registro de la reparación con estado y servicios |

### Estados de una orden de trabajo
`Pendiente` → `EnProceso` → `Completada` / `Cancelada`

---

## 🔌 Endpoints de la API

### Clientes – `/api/clientes`
| Método | Ruta              | Descripción              |
|--------|-------------------|--------------------------|
| GET    | `/`               | Listar todos los clientes |
| GET    | `/{id}`           | Obtener cliente por ID   |
| POST   | `/`               | Crear nuevo cliente      |
| PUT    | `/{id}`           | Actualizar cliente       |
| DELETE | `/{id}`           | Eliminar cliente         |

### Vehículos – `/api/vehiculos`
| Método | Ruta                    | Descripción                      |
|--------|-------------------------|----------------------------------|
| GET    | `/`                     | Listar todos los vehículos       |
| GET    | `/{id}`                 | Obtener vehículo por ID          |
| GET    | `/cliente/{clienteId}`  | Vehículos de un cliente          |
| POST   | `/`                     | Registrar vehículo               |
| PUT    | `/{id}`                 | Actualizar vehículo              |
| DELETE | `/{id}`                 | Eliminar vehículo                |

### Servicios – `/api/servicios`
| Método | Ruta    | Descripción              |
|--------|---------|--------------------------|
| GET    | `/`     | Listar catálogo           |
| GET    | `/{id}` | Obtener servicio por ID  |
| POST   | `/`     | Crear servicio           |
| PUT    | `/{id}` | Actualizar servicio      |
| DELETE | `/{id}` | Eliminar servicio        |

### Órdenes de Trabajo – `/api/ordenestrabajo`
| Método | Ruta                    | Descripción                     |
|--------|-------------------------|---------------------------------|
| GET    | `/`                     | Listar todas las órdenes        |
| GET    | `/{id}`                 | Obtener orden por ID            |
| GET    | `/cliente/{clienteId}`  | Órdenes de un cliente           |
| POST   | `/`                     | Crear nueva orden               |
| PUT    | `/{id}`                 | Actualizar orden                |
| PATCH  | `/{id}/estado`          | Cambiar estado de la orden      |
| DELETE | `/{id}`                 | Eliminar orden                  |

---

## 🧠 Conceptos de arquitectura aplicados

- **Separación de responsabilidades** (SoC): cada capa tiene una única responsabilidad.
- **Inversión de dependencias** (DIP): los controladores dependen de interfaces, no de implementaciones concretas.
- **Inyección de dependencias** (DI): los servicios se registran y resuelven mediante el contenedor de .NET.
- **Patrón Servicio**: la lógica de negocio reside en la capa `Services`, desacoplada de la API.
