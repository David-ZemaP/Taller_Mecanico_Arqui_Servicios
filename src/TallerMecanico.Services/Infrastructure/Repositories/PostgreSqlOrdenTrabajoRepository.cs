using Npgsql;
using TallerMecanico.Core.Contracts;
using TallerMecanico.Core.Models;

namespace TallerMecanico.Services.Infrastructure.Repositories;

internal sealed class PostgreSqlOrdenTrabajoRepository : IOrdenTrabajoRepository
{
    private readonly PostgreSqlDatabase _database;

    public PostgreSqlOrdenTrabajoRepository(PostgreSqlDatabase database)
    {
        _database = database;
        _database.EnsureSchema();
    }

    public OrdenTrabajo Add(OrdenTrabajo ordenTrabajo, int? usuarioId = null)
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();
        try
        {
            ApplyProductStockDelta(connection, transaction, Array.Empty<DetalleProducto>(), ordenTrabajo.Productos);

            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = @"
INSERT INTO ordenes_trabajo (cliente_id, vehiculo_id, descripcion, estado, fecha_creacion, usuario_creacion_id, is_deleted)
VALUES (@cliente_id, @vehiculo_id, @descripcion, @estado, NOW(), @usuario_creacion_id, FALSE)
RETURNING id, fecha_creacion;";
                command.Parameters.AddWithValue("cliente_id", ordenTrabajo.ClienteId);
                command.Parameters.AddWithValue("vehiculo_id", ordenTrabajo.VehiculoId);
                command.Parameters.AddWithValue("descripcion", (object?)ordenTrabajo.Descripcion ?? DBNull.Value);
                command.Parameters.AddWithValue("estado", (int)EstadoOrden.Pendiente);
                command.Parameters.AddWithValue("usuario_creacion_id", (object?)usuarioId ?? DBNull.Value);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    ordenTrabajo.Id = reader.GetInt32(reader.GetOrdinal("id"));
                    ordenTrabajo.FechaCreacion = reader.GetDateTime(reader.GetOrdinal("fecha_creacion"));
                }
            }

            InsertOrderProducts(connection, transaction, ordenTrabajo);
            InsertOrderServices(connection, transaction, ordenTrabajo);

            transaction.Commit();
            ordenTrabajo.Estado = EstadoOrden.Pendiente;
            ordenTrabajo.UsuarioCreacionId = usuarioId;
            return LoadById(connection, ordenTrabajo.Id)!;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public OrdenTrabajo? GetById(int id)
    {
        using var connection = _database.OpenConnection();
        connection.Open();
        return LoadById(connection, id);
    }

    public IEnumerable<OrdenTrabajo> GetAll()
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM ordenes_trabajo WHERE is_deleted = FALSE ORDER BY id DESC";

        using var reader = command.ExecuteReader();
        var result = new List<OrdenTrabajo>();
        while (reader.Read())
        {
            result.Add(PostgreSqlMappings.MapOrden(reader));
        }

        reader.Close();

        return result.Select(orden => LoadById(connection, orden.Id) ?? orden).ToList();
    }

    public IEnumerable<OrdenTrabajo> GetByCliente(int clienteId)
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM ordenes_trabajo WHERE cliente_id = @cliente_id AND is_deleted = FALSE ORDER BY id DESC";
        command.Parameters.AddWithValue("cliente_id", clienteId);

        using var reader = command.ExecuteReader();
        var result = new List<OrdenTrabajo>();
        while (reader.Read())
        {
            result.Add(PostgreSqlMappings.MapOrden(reader));
        }

        reader.Close();

        return result.Select(orden => LoadById(connection, orden.Id) ?? orden).ToList();
    }

    public OrdenTrabajo? Update(int id, OrdenTrabajo ordenTrabajo)
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();
        try
        {
            var existing = LoadById(connection, id, transaction);
            if (existing is null)
            {
                transaction.Rollback();
                return null;
            }

            ApplyProductStockDelta(connection, transaction, existing.Productos, ordenTrabajo.Productos);
            DeleteOrderDetails(connection, transaction, id);

            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = @"
UPDATE ordenes_trabajo
SET cliente_id = @cliente_id,
    vehiculo_id = @vehiculo_id,
    descripcion = @descripcion
WHERE id = @id AND is_deleted = FALSE;";
                command.Parameters.AddWithValue("id", id);
                command.Parameters.AddWithValue("cliente_id", ordenTrabajo.ClienteId);
                command.Parameters.AddWithValue("vehiculo_id", ordenTrabajo.VehiculoId);
                command.Parameters.AddWithValue("descripcion", (object?)ordenTrabajo.Descripcion ?? DBNull.Value);
                command.ExecuteNonQuery();
            }

            InsertOrderProducts(connection, transaction, new OrdenTrabajo
            {
                Id = id,
                Productos = ordenTrabajo.Productos,
                Servicios = ordenTrabajo.Servicios
            });
            InsertOrderServices(connection, transaction, new OrdenTrabajo
            {
                Id = id,
                Productos = ordenTrabajo.Productos,
                Servicios = ordenTrabajo.Servicios
            });

            transaction.Commit();
            return LoadById(connection, id);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public OrdenTrabajo? CambiarEstado(int id, EstadoOrden estado, int? usuarioId = null)
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE ordenes_trabajo
SET estado = @estado,
    fecha_completado = CASE WHEN @estado = 2 THEN NOW() ELSE fecha_completado END
WHERE id = @id AND is_deleted = FALSE;";
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("estado", (int)estado);

        if (command.ExecuteNonQuery() == 0)
        {
            return null;
        }

        return LoadById(connection, id);
    }

    public OrdenTrabajo? Anular(int id, int usuarioId)
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();
        try
        {
            var existing = LoadById(connection, id, transaction);
            if (existing is null)
            {
                transaction.Rollback();
                return null;
            }

            ApplyProductStockDelta(connection, transaction, existing.Productos, Array.Empty<DetalleProducto>());

            using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = @"
UPDATE ordenes_trabajo
SET estado = @estado,
    is_deleted = TRUE,
    fecha_anulacion = NOW(),
    usuario_anulacion_id = @usuario_anulacion_id
WHERE id = @id AND is_deleted = FALSE;";
                command.Parameters.AddWithValue("id", id);
                command.Parameters.AddWithValue("estado", (int)EstadoOrden.Anulada);
                command.Parameters.AddWithValue("usuario_anulacion_id", usuarioId);
                command.ExecuteNonQuery();
            }

            transaction.Commit();
            return LoadById(connection, id);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private OrdenTrabajo? LoadById(NpgsqlConnection connection, int id, NpgsqlTransaction? transaction = null)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT * FROM ordenes_trabajo WHERE id = @id";
        command.Parameters.AddWithValue("id", id);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        var order = PostgreSqlMappings.MapOrden(reader);
        reader.Close();

        order.Productos = LoadProductos(connection, id, transaction);
        order.Servicios = LoadServicios(connection, id, transaction);
        return order;
    }

    private static void InsertOrderProducts(NpgsqlConnection connection, NpgsqlTransaction transaction, OrdenTrabajo ordenTrabajo)
    {
        foreach (var detalle in ordenTrabajo.Productos)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
INSERT INTO ordenes_trabajo_productos (orden_trabajo_id, producto_id, nombre_producto, cantidad, precio_unitario)
VALUES (@orden_trabajo_id, @producto_id, @nombre_producto, @cantidad, @precio_unitario);";
            command.Parameters.AddWithValue("orden_trabajo_id", ordenTrabajo.Id);
            command.Parameters.AddWithValue("producto_id", detalle.ProductoId);
            command.Parameters.AddWithValue("nombre_producto", detalle.NombreProducto);
            command.Parameters.AddWithValue("cantidad", detalle.Cantidad);
            command.Parameters.AddWithValue("precio_unitario", detalle.PrecioUnitario);
            command.ExecuteNonQuery();
        }
    }

    private static void InsertOrderServices(NpgsqlConnection connection, NpgsqlTransaction transaction, OrdenTrabajo ordenTrabajo)
    {
        foreach (var servicio in ordenTrabajo.Servicios)
        {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
INSERT INTO ordenes_trabajo_servicios (orden_trabajo_id, servicio_id, nombre_servicio, descripcion, precio)
VALUES (@orden_trabajo_id, @servicio_id, @nombre_servicio, @descripcion, @precio);";
            command.Parameters.AddWithValue("orden_trabajo_id", ordenTrabajo.Id);
            command.Parameters.AddWithValue("servicio_id", servicio.Id);
            command.Parameters.AddWithValue("nombre_servicio", servicio.Nombre);
            command.Parameters.AddWithValue("descripcion", (object?)servicio.Descripcion ?? DBNull.Value);
            command.Parameters.AddWithValue("precio", servicio.Precio);
            command.ExecuteNonQuery();
        }
    }

    private static void DeleteOrderDetails(NpgsqlConnection connection, NpgsqlTransaction transaction, int orderId)
    {
        using var productCommand = connection.CreateCommand();
        productCommand.Transaction = transaction;
        productCommand.CommandText = "DELETE FROM ordenes_trabajo_productos WHERE orden_trabajo_id = @orden_trabajo_id;";
        productCommand.Parameters.AddWithValue("orden_trabajo_id", orderId);
        productCommand.ExecuteNonQuery();

        using var serviceCommand = connection.CreateCommand();
        serviceCommand.Transaction = transaction;
        serviceCommand.CommandText = "DELETE FROM ordenes_trabajo_servicios WHERE orden_trabajo_id = @orden_trabajo_id;";
        serviceCommand.Parameters.AddWithValue("orden_trabajo_id", orderId);
        serviceCommand.ExecuteNonQuery();
    }

    private static List<DetalleProducto> LoadProductos(NpgsqlConnection connection, int orderId, NpgsqlTransaction? transaction)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT * FROM ordenes_trabajo_productos WHERE orden_trabajo_id = @orden_trabajo_id ORDER BY producto_id";
        command.Parameters.AddWithValue("orden_trabajo_id", orderId);

        using var reader = command.ExecuteReader();
        var result = new List<DetalleProducto>();
        while (reader.Read())
        {
            result.Add(PostgreSqlMappings.MapDetalleProducto(reader));
        }

        return result;
    }

    private static List<Servicio> LoadServicios(NpgsqlConnection connection, int orderId, NpgsqlTransaction? transaction)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "SELECT * FROM ordenes_trabajo_servicios WHERE orden_trabajo_id = @orden_trabajo_id ORDER BY servicio_id";
        command.Parameters.AddWithValue("orden_trabajo_id", orderId);

        using var reader = command.ExecuteReader();
        var result = new List<Servicio>();
        while (reader.Read())
        {
            result.Add(PostgreSqlMappings.MapDetalleServicio(reader));
        }

        return result;
    }

    private static void ApplyProductStockDelta(NpgsqlConnection connection, NpgsqlTransaction transaction, IEnumerable<DetalleProducto> existingDetails, IEnumerable<DetalleProducto> newDetails)
    {
        var existingByProduct = existingDetails
            .GroupBy(detail => detail.ProductoId)
            .ToDictionary(group => group.Key, group => group.Sum(detail => detail.Cantidad));

        var newByProduct = newDetails
            .GroupBy(detail => detail.ProductoId)
            .ToDictionary(group => group.Key, group => group.Sum(detail => detail.Cantidad));

        var productIds = existingByProduct.Keys.Union(newByProduct.Keys);

        foreach (var productId in productIds)
        {
            var existingQuantity = existingByProduct.TryGetValue(productId, out var oldQuantity) ? oldQuantity : 0;
            var newQuantity = newByProduct.TryGetValue(productId, out var currentQuantity) ? currentQuantity : 0;
            var delta = newQuantity - existingQuantity;

            if (delta == 0)
            {
                continue;
            }

            using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = delta > 0
                ? @"
UPDATE productos
SET stock = stock - @cantidad
WHERE id = @id AND is_deleted = FALSE AND stock >= @cantidad;"
                : @"
UPDATE productos
SET stock = stock + @cantidad
WHERE id = @id AND is_deleted = FALSE;";
            command.Parameters.AddWithValue("id", productId);
            command.Parameters.AddWithValue("cantidad", Math.Abs(delta));

            if (command.ExecuteNonQuery() == 0)
            {
                throw new InvalidOperationException($"No se pudo actualizar el stock del producto {productId}.");
            }
        }
    }
}