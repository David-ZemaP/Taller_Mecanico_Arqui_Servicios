using Npgsql;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Enums;

namespace Taller_Mecanico_Arqui.Infrastructure.Persistence;

public class SqlEntityQueryService
{
    public IEnumerable<OrdenTrabajo> LoadOrdenesTrabajo(NpgsqlConnection connection)
    {
        var ordenes = new List<OrdenTrabajo>();

        const string sql = @"
SELECT ot.ordentrabajoid, ot.vehiculoid, ot.fechaingreso, ot.fechaentrega,
       ot.estadotrabajo, ot.estadopago, ot.estadovehiculo, ot.total,
       ot.isdeleted, ot.fechaactualizacion,
       v.vehiculoid as v_id, v.placa, v.marca, v.modelo, v.anio, v.clienteid, v.isdeleted as v_deleted,
       c.clienteid as c_id, c.ci, c.nombres, c.primerapellido, c.segundoapellido
FROM ordentrabajo ot
LEFT JOIN vehiculo v ON v.vehiculoid = ot.vehiculoid
LEFT JOIN cliente c ON c.clienteid = v.clienteid
WHERE ot.isdeleted = FALSE";

        using var cmd = new NpgsqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var estadoTrabajo = Enum.TryParse<EstadoTrabajo>(reader["estadotrabajo"]?.ToString(), true, out var et)
                ? et : EstadoTrabajo.Recibido;
            var estadoPago = Enum.TryParse<EstadoPago>(reader["estadopago"]?.ToString(), true, out var ep)
                ? ep : EstadoPago.Pendiente;

            Cliente? cliente = null;
            if (reader["c_id"] != DBNull.Value)
            {
                cliente = new Cliente
                {
                    ClienteId = Convert.ToInt32(reader["c_id"]),
                    Ci = reader["ci"] as string,
                    Nombres = reader["nombres"] as string,
                    PrimerApellido = reader["primerapellido"] as string,
                    SegundoApellido = reader["segundoapellido"] as string
                };
            }

            Vehiculo? vehiculo = null;
            if (reader["v_id"] != DBNull.Value)
            {
                vehiculo = new Vehiculo
                {
                    VehiculoId = Convert.ToInt32(reader["v_id"]),
                    Placa = reader["placa"]?.ToString() ?? string.Empty,
                    Marca = reader["marca"]?.ToString() ?? string.Empty,
                    Modelo = reader["modelo"]?.ToString() ?? string.Empty,
                    Anio = reader["anio"] != DBNull.Value ? Convert.ToInt32(reader["anio"]) : 0,
                    ClienteId = Convert.ToInt32(reader["clienteid"]),
                    IsDeleted = Convert.ToBoolean(reader["v_deleted"]),
                    Cliente = cliente
                };
            }

            var orden = OrdenTrabajo.Reconstituir(
                Convert.ToInt32(reader["ordentrabajoid"]),
                Convert.ToInt32(reader["vehiculoid"]),
                Convert.ToDateTime(reader["fechaingreso"]),
                reader["fechaentrega"] as DateTime?,
                estadoTrabajo,
                estadoPago,
                reader["estadovehiculo"]?.ToString() ?? string.Empty,
                Convert.ToDouble(reader["total"]),
                Convert.ToBoolean(reader["isdeleted"]),
                reader["fechaactualizacion"] as DateTime?,
                vehiculo);

            ordenes.Add(orden);
        }

        return ordenes;
    }

    public IEnumerable<Producto> LoadProductos(NpgsqlConnection connection)
    {
        var productos = new List<Producto>();
        const string sql = "SELECT productoid, nombre, precio, stock, activo FROM producto WHERE activo = TRUE";

        using var cmd = new NpgsqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            productos.Add(new Producto
            {
                ProductoId = Convert.ToInt32(reader["productoid"]),
                Nombre = reader["nombre"]?.ToString() ?? string.Empty,
                Precio = Convert.ToDouble(reader["precio"]),
                Stock = Convert.ToInt32(reader["stock"]),
                Activo = Convert.ToBoolean(reader["activo"])
            });
        }
        return productos;
    }

    public IEnumerable<Servicio> LoadServicios(NpgsqlConnection connection)
    {
        var servicios = new List<Servicio>();
        const string sql = "SELECT servicioid, nombre, precio, activo FROM servicio WHERE activo = TRUE";

        using var cmd = new NpgsqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            servicios.Add(new Servicio
            {
                ServicioId = Convert.ToInt32(reader["servicioid"]),
                Nombre = reader["nombre"]?.ToString() ?? string.Empty,
                Precio = Convert.ToDouble(reader["precio"]),
                Activo = Convert.ToBoolean(reader["activo"])
            });
        }
        return servicios;
    }

    public IEnumerable<Vehiculo> LoadVehiculos(NpgsqlConnection connection)
    {
        var vehiculos = new List<Vehiculo>();
        const string sql = @"
SELECT v.vehiculoid, v.placa, v.marca, v.modelo, v.anio, v.clienteid, v.isdeleted,
       c.clienteid as c_id, c.ci, c.nombres, c.primerapellido, c.segundoapellido
FROM vehiculo v
LEFT JOIN cliente c ON c.clienteid = v.clienteid
WHERE v.isdeleted = FALSE";

        using var cmd = new NpgsqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            Cliente? cliente = null;
            if (reader["c_id"] != DBNull.Value)
            {
                cliente = new Cliente
                {
                    ClienteId = Convert.ToInt32(reader["c_id"]),
                    Ci = reader["ci"] as string,
                    Nombres = reader["nombres"] as string,
                    PrimerApellido = reader["primerapellido"] as string,
                    SegundoApellido = reader["segundoapellido"] as string
                };
            }

            vehiculos.Add(new Vehiculo
            {
                VehiculoId = Convert.ToInt32(reader["vehiculoid"]),
                Placa = reader["placa"]?.ToString() ?? string.Empty,
                Marca = reader["marca"]?.ToString() ?? string.Empty,
                Modelo = reader["modelo"]?.ToString() ?? string.Empty,
                Anio = reader["anio"] != DBNull.Value ? Convert.ToInt32(reader["anio"]) : 0,
                ClienteId = Convert.ToInt32(reader["clienteid"]),
                IsDeleted = Convert.ToBoolean(reader["isdeleted"]),
                Cliente = cliente
            });
        }
        return vehiculos;
    }

    public IEnumerable<OrdenTrabajoCatalogo> LoadOrdenTrabajoCatalogos(NpgsqlConnection connection)
    {
        var catalogos = new List<OrdenTrabajoCatalogo>();
        const string sql = "SELECT ordentrabajocatalogoid, ordentrabajoid, productoid, cantidadutilizada, preciounitario, fecharegistro FROM ordentrabajocatalogo";

        using var cmd = new NpgsqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            catalogos.Add(OrdenTrabajoCatalogo.Crear(
                Convert.ToInt32(reader["ordentrabajoid"]),
                Convert.ToInt32(reader["productoid"]),
                Convert.ToInt32(reader["cantidadutilizada"]),
                Convert.ToDecimal(reader["preciounitario"])));
        }
        return catalogos;
    }
}
