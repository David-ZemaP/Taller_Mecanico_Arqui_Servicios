# Instrucciones para Ejecutar el Proyecto Taller Mecánico

## Descripción General
Este proyecto es una aplicación de microservicios desarrollada en .NET Core con PostgreSQL. Consta de 3 servicios independientes:
- **Frontend**: Aplicación Razor Pages ASP.NET Core (Puerto 5146)
- **UsersService**: API de autenticación y usuarios (Puerto 5297)
- **OrdenTrabajoService**: API de órdenes de trabajo (Puerto 5229)
- **PostgreSQL**: Base de datos (Puerto 5432)

## Requisitos Previos
- Docker y Docker Compose instalados
- .NET 8 o superior instalado
- Acceso a terminal/línea de comandos

## Paso 1: Iniciar la Base de Datos con Docker Compose

```bash
cd /home/Pololi15/Documentos/Universidad/7_Semestre/ARQUITECTURA_DE_SOFTWARE/codigos_docente/ec2_3er_entregable/Deivi/Taller_Mecanico_Arqui_Servicios

# Iniciar PostgreSQL y pgAdmin
docker-compose up -d
```

**Verificar que la BD está lista:**
```bash
# Esperar 30 segundos para que PostgreSQL se inicialice
sleep 30

# Verificar conexión
psql -h localhost -U postgres -d taller_mecanico -c "SELECT COUNT(*) FROM usuariologin;"
```

## Paso 2: Iniciar los Servicios .NET

**Opción A: Ejecutar todo en una terminal (Recomendado para inicio rápido)**

```bash
# Desde la raíz del proyecto
cd /home/Pololi15/Documentos/Universidad/7_Semestre/ARQUITECTURA_DE_SOFTWARE/codigos_docente/ec2_3er_entregable/Deivi/Taller_Mecanico_Arqui_Servicios

# Terminal 1 - UsersService (Requisito: debe iniciar primero)
dotnet run --project UsersService/App -c Debug

# Terminal 2 - OrdenTrabajoService (cuando UsersService esté listo)
dotnet run --project OrdenTrabajoService -c Debug

# Terminal 3 - Frontend (cuando ambos servicios anteriores estén listos)
dotnet run --project Frontend -c Debug
```

**Opción B: Ejecutar en background (Más conveniente)**

```bash
# Matar cualquier proceso dotnet anterior
pkill -f "dotnet run" || true
sleep 2

# Iniciar servicios en background
cd /home/Pololi15/Documentos/Universidad/7_Semestre/ARQUITECTURA_DE_SOFTWARE/codigos_docente/ec2_3er_entregable/Deivi/Taller_Mecanico_Arqui_Servicios

# Iniciar UsersService
dotnet run --project UsersService/App -c Debug > /tmp/users-service.log 2>&1 &
sleep 3

# Iniciar OrdenTrabajoService
dotnet run --project OrdenTrabajoService -c Debug > /tmp/orden-service.log 2>&1 &
sleep 2

# Iniciar Frontend
dotnet run --project Frontend -c Debug > /tmp/frontend.log 2>&1 &
sleep 2

# Verificar que están corriendo
ps aux | grep "dotnet run" | grep -v grep
```

## Paso 3: Acceder a la Aplicación

**URL del Frontend:**
```
http://localhost:5146
```

## Usuarios de Prueba

Todos los usuarios usan la contraseña: **`prueba123`**

| Email | Contraseña | Nivel de Acceso | Rol |
|-------|-----------|-----------------|-----|
| admin@prueba.local | prueba123 | Gerente | Administrador |
| completo@prueba.local | prueba123 | Completo | Empleado |
| parcial@prueba.local | prueba123 | Parcial | Empleado |
| cliente@prueba.local | prueba123 | Cliente | Cliente |

## Controles de Acceso por Nivel

- **Gerente**: Acceso a todo (Reportes, Empleados, Productos/Servicios, Clientes, Órdenes)
- **Completo**: Empleados, Productos/Servicios, Órdenes
- **Parcial**: Clientes, Órdenes
- **Cliente**: Solo ver mis órdenes de trabajo

## Detener los Servicios

```bash
# Matar todos los procesos dotnet
pkill -f "dotnet run" || true

# Detener Docker (opcional, si no necesitas la BD)
docker-compose down
```

## Logs y Debugging

**Ver logs de servicio en background:**
```bash
# UsersService
tail -f /tmp/users-service.log

# OrdenTrabajoService
tail -f /tmp/orden-service.log

# Frontend
tail -f /tmp/frontend.log
```

**Base de datos (pgAdmin):**
```
http://localhost:5050
Usuario: admin@example.com
Contraseña: admin
```

## Monitoreo de APIs

**Verificar que UsersService está respondiendo:**
```bash
curl -H 'Content-Type: application/json' \
  -d '{"email":"admin@prueba.local","password":"prueba123"}' \
  http://localhost:5297/api/auth/login | jq .
```

**Verificar que OrdenTrabajoService está respondiendo:**
```bash
curl http://localhost:5229/weatherforecast | jq .
```

## Troubleshooting

### Error: "Connection refused" en puerto 5432
**Solución:** Verificar que Docker Compose está corriendo
```bash
docker-compose ps
```

### Error: "Cannot connect to database"
**Solución:** Reiniciar los contenedores
```bash
docker-compose restart
```

### Error: "Port already in use"
**Solución:** Matar procesos anteriores y esperar
```bash
pkill -f "dotnet run" || true
sleep 3
# Luego reiniciar servicios
```

### Las transacciones no persisten
**Solución:** Verificar que PostgreSQL está inicializado
```bash
# Ejecutar script de inicialización manualmente
psql -h localhost -U postgres < Scripts/init.sql
```

## Script Automatizado (Recomendado)

Crea un archivo `run-project.sh`:

```bash
#!/bin/bash
set -e

PROJECT_DIR="/home/Pololi15/Documentos/Universidad/7_Semestre/ARQUITECTURA_DE_SOFTWARE/codigos_docente/ec2_3er_entregable/Deivi/Taller_Mecanico_Arqui_Servicios"

echo "🚀 Iniciando Taller Mecánico..."

# Detener servicios anteriores
echo "🛑 Deteniendo servicios anteriores..."
pkill -f "dotnet run" || true
sleep 2

cd "$PROJECT_DIR"

# Iniciar Docker Compose si no está corriendo
if ! docker ps | grep -q postgres; then
    echo "🐳 Iniciando Docker Compose..."
    docker-compose up -d
    sleep 15
fi

# Iniciar servicios
echo "▶️  Iniciando UsersService..."
dotnet run --project UsersService/App -c Debug > /tmp/users-service.log 2>&1 &
sleep 3

echo "▶️  Iniciando OrdenTrabajoService..."
dotnet run --project OrdenTrabajoService -c Debug > /tmp/orden-service.log 2>&1 &
sleep 2

echo "▶️  Iniciando Frontend..."
dotnet run --project Frontend -c Debug > /tmp/frontend.log 2>&1 &
sleep 3

echo "✅ Todos los servicios están corriendo!"
echo ""
echo "📱 Frontend: http://localhost:5146"
echo "📊 Usuario de prueba: admin@prueba.local / prueba123"
echo ""
echo "Para ver logs:"
echo "  tail -f /tmp/users-service.log"
echo "  tail -f /tmp/orden-service.log"
echo "  tail -f /tmp/frontend.log"
echo ""
echo "Para detener todos los servicios:"
echo "  pkill -f 'dotnet run'"
```

Hacer el script ejecutable:
```bash
chmod +x run-project.sh
```

Ejecutar:
```bash
./run-project.sh
```

## Notas Técnicas

- **Esquema de Autenticación**: Cookie-based para Frontend, JWT para inter-servicios
- **Seguridad**: Las contraseñas se hashean con BCrypt
- **Base de Datos**: PostgreSQL 16 con inicialización automática
- **Puertos**: 
  - Frontend: 5146
  - UsersService: 5297
  - OrdenTrabajoService: 5229
  - PostgreSQL: 5432
  - pgAdmin: 5050
