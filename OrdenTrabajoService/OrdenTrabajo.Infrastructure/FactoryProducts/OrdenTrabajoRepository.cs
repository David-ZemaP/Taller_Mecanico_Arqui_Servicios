using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Infrastructure.Persistence;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Infrastructure.Services;

namespace Taller_Mecanico_Arqui.Infrastructure.Persistence.Repositories
{
    public class OrdenTrabajoRepository : IOrdenTrabajoRepository, IRepository<OrdenTrabajo>
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly SqlEntityQueryService _queryService;
        private readonly AuthenticationHelper _authHelper;

        public OrdenTrabajoRepository(ISqlConnectionFactory connectionFactory, SqlEntityQueryService queryService, AuthenticationHelper authHelper)
        {
            _connectionFactory = connectionFactory;
            _queryService = queryService;
            _authHelper = authHelper;
        }

        public async Task<IEnumerable<OrdenTrabajo>> GetAllAsync()
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            
            return _queryService.LoadOrdenesTrabajo(connection)
                .OrderByDescending(o => o.FechaIngreso)
                .ToList();
        }

        public async Task<Result<OrdenTrabajo?>> GetByIdAsync(int id)
        {
            try
            {
                await using var connection = _connectionFactory.CreateConnection();
                await connection.OpenAsync();

                var orden = _queryService.LoadOrdenesTrabajo(connection)
                    .FirstOrDefault(o => o.OrdenTrabajoId == id);

                return Result<Domain.Entities.OrdenTrabajo?>.Success(orden);
            }
            catch (Exception ex)
            {
                return Result<Domain.Entities.OrdenTrabajo?>.Failure(ErrorCodes.DbError, $"Error al obtener orden de trabajo con ID {id}: {ex.Message}");
            }
        }

        public async Task<Result<int>> AddAsync(OrdenTrabajo entity)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var actorAuditoria = _authHelper.GetCurrentAuditActor();
                var fechaIngreso = entity.FechaIngreso == default ? DateTime.UtcNow : entity.FechaIngreso;

                const string sql = @"
INSERT INTO ordentrabajo (vehiculoid, fechaingreso, fechaentrega, estadotrabajo, estadopago, estadovehiculo, total, estado, isdeleted, fechaactualizacion, creadopor)
VALUES (@vehiculoid, @fechaingreso, @fechaentrega, @estadotrabajo, @estadopago, @estadovehiculo, @total, 1, FALSE, @fechaactualizacion, @creadopor)
RETURNING ordentrabajoid;";

                await using var command = new Npgsql.NpgsqlCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("vehiculoid", entity.VehiculoId);
                command.Parameters.AddWithValue("fechaingreso", fechaIngreso);
                command.Parameters.AddWithValue("fechaentrega", (object?)entity.FechaEntrega ?? DBNull.Value);
                command.Parameters.AddWithValue("estadotrabajo", entity.EstadoTrabajo.ToString());
                command.Parameters.AddWithValue("estadopago", entity.EstadoPago.ToString());
                command.Parameters.AddWithValue("estadovehiculo", entity.EstadoVehiculo);
                command.Parameters.AddWithValue("total", entity.Total);
                command.Parameters.AddWithValue("fechaactualizacion", (object?)entity.FechaActualizacion ?? DBNull.Value);
                command.Parameters.AddWithValue("creadopor", actorAuditoria);

                var ordenIdObj = await command.ExecuteScalarAsync();
                if (ordenIdObj == null || ordenIdObj == DBNull.Value)
                {
                    await transaction.RollbackAsync();
                    return Result<int>.Failure(ErrorCodes.DbInsertFailed, "No se pudo recuperar el ID de la orden de trabajo registrada.");
                }

                var ordenTrabajoId = Convert.ToInt32(ordenIdObj);

                await InsertarProductosAsync(connection, transaction, ordenTrabajoId, entity.ProductosUsados, actorAuditoria);
                await InsertarServiciosAsync(connection, transaction, ordenTrabajoId, entity.ServiciosRealizados, actorAuditoria);

                var stockResult = await AjustarStockProductosAsync(connection, transaction, entity.ProductosUsados, -1, actorAuditoria);
                if (stockResult.IsFailure)
                {
                    await transaction.RollbackAsync();
                    return Result<int>.Failure(
                        stockResult.ErrorCode ?? ErrorCodes.DbError,
                        stockResult.ErrorMessage ?? "No se pudo actualizar el stock de los productos de la orden.");
                }

                await transaction.CommitAsync();
                return Result<int>.Success(ordenTrabajoId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<int>.Failure(ErrorCodes.DbError, $"Error al registrar orden de trabajo: {ex.Message}");
            }
        }

        private static async Task InsertarProductosAsync(
            Npgsql.NpgsqlConnection connection,
            Npgsql.NpgsqlTransaction transaction,
            int ordenTrabajoId,
            IReadOnlyCollection<OrdenTrabajoProducto> productos,
            string actorAuditoria)
        {
            if (productos.Count == 0)
            {
                return;
            }

            const string sql = @"
INSERT INTO ordentrabajoproducto (ordentrabajoid, productoid, cantidad, preciounitario, subtotal, creadopor)
VALUES (@ordentrabajoid, @productoid, @cantidad, @preciounitario, @subtotal, @creadopor);";

            foreach (var producto in productos)
            {
                await using var command = new Npgsql.NpgsqlCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("ordentrabajoid", ordenTrabajoId);
                command.Parameters.AddWithValue("productoid", producto.ProductoId);
                command.Parameters.AddWithValue("cantidad", producto.Cantidad);
                command.Parameters.AddWithValue("preciounitario", Convert.ToDecimal(producto.PrecioUnitario));
                command.Parameters.AddWithValue("subtotal", Convert.ToDecimal(producto.Subtotal));
                command.Parameters.AddWithValue("creadopor", actorAuditoria);
                await command.ExecuteNonQueryAsync();
            }
        }

        private static async Task InsertarServiciosAsync(
            Npgsql.NpgsqlConnection connection,
            Npgsql.NpgsqlTransaction transaction,
            int ordenTrabajoId,
            IReadOnlyCollection<OrdenTrabajoServicio> servicios,
            string actorAuditoria)
        {
            if (servicios.Count == 0)
            {
                return;
            }

            const string sql = @"
INSERT INTO ordentrabajoservicio (ordentrabajoid, servicioid, cantidad, preciounitario, subtotal, creadopor)
VALUES (@ordentrabajoid, @servicioid, @cantidad, @preciounitario, @subtotal, @creadopor);";

            foreach (var servicio in servicios)
            {
                await using var command = new Npgsql.NpgsqlCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("ordentrabajoid", ordenTrabajoId);
                command.Parameters.AddWithValue("servicioid", servicio.ServicioId);
                command.Parameters.AddWithValue("cantidad", servicio.Cantidad);
                command.Parameters.AddWithValue("preciounitario", Convert.ToDecimal(servicio.PrecioUnitario));
                command.Parameters.AddWithValue("subtotal", Convert.ToDecimal(servicio.Subtotal));
                command.Parameters.AddWithValue("creadopor", actorAuditoria);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<Result> UpdateAsync(OrdenTrabajo entity)
        {
            try
            {
                await using var connection = _connectionFactory.CreateConnection();
                await connection.OpenAsync();

            var sql = @"
UPDATE ordentrabajo SET vehiculoid = @vehiculoid, fechaingreso = @fechaingreso, fechaentrega = @fechaentrega,
estadotrabajo = @estadotrabajo, estadopago = @estadopago, estadovehiculo = @estadovehiculo, total = @total, fechaactualizacion = @fechaactualizacion, actualizadopor = @actualizadopor
WHERE ordentrabajoid = @ordenid;";

            var actorAuditoria = _authHelper.GetCurrentAuditActor();

            await using var command = new Npgsql.NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("ordenid", entity.OrdenTrabajoId);
            command.Parameters.AddWithValue("vehiculoid", entity.VehiculoId);
            command.Parameters.AddWithValue("fechaingreso", entity.FechaIngreso);
            command.Parameters.AddWithValue("fechaentrega", (object?)entity.FechaEntrega ?? DBNull.Value);
            command.Parameters.AddWithValue("estadotrabajo", entity.EstadoTrabajo.ToString());
            command.Parameters.AddWithValue("estadopago", entity.EstadoPago.ToString());
            command.Parameters.AddWithValue("estadovehiculo", entity.EstadoVehiculo);
            command.Parameters.AddWithValue("total", entity.Total);
            command.Parameters.AddWithValue("fechaactualizacion", DateTime.UtcNow);
            command.Parameters.AddWithValue("actualizadopor", actorAuditoria);

            await command.ExecuteNonQueryAsync();
            return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorCodes.DbError, $"Error al actualizar orden de trabajo: {ex.Message}");
            }
        }

        public async Task DeleteAsync(int id)
        {
            await SetAnuladoAsync(id, true);
        }

        public async Task<Result> SetAnuladoAsync(int id, bool anulado)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var actorAuditoria = _authHelper.GetCurrentAuditActor();

                var estadoActual = await ObtenerEstadoActualAsync(connection, transaction, id);
                if (estadoActual.IsFailure)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure(estadoActual.ErrorCode ?? ErrorCodes.DbError, estadoActual.ErrorMessage ?? "No se pudo consultar la orden de trabajo.");
                }

                if (!estadoActual.Value.HasValue)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure(ErrorCodes.OrdenTrabajoNotFound, $"Orden de trabajo con ID {id} no encontrada");
                }

                if (estadoActual.Value.Value == anulado)
                {
                    await transaction.CommitAsync();
                    return Result.Success();
                }

                var productos = await ObtenerProductosOrdenAsync(connection, transaction, id);
                if (productos.IsFailure)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure(productos.ErrorCode ?? ErrorCodes.DbError, productos.ErrorMessage ?? "No se pudieron leer los productos asociados a la orden.");
                }

                var movimientoStock = anulado ? 1 : -1;
                var stockResult = await AjustarStockProductosAsync(connection, transaction, productos.Value!, movimientoStock, actorAuditoria);
                if (stockResult.IsFailure)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure(stockResult.ErrorCode ?? ErrorCodes.DbError, stockResult.ErrorMessage ?? "No se pudo ajustar el stock asociado a la orden.");
                }

                const string sql = @"
UPDATE ordentrabajo
SET estado = @estado,
    isdeleted = @isdeleted,
    fechaactualizacion = @fechaactualizacion,
    eliminadopor = CASE WHEN @isdeleted THEN @actor ELSE NULL END,
    actualizadopor = CASE WHEN NOT @isdeleted THEN @actor ELSE actualizadopor END
WHERE ordentrabajoid = @ordenid;";

                await using var command = new Npgsql.NpgsqlCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("ordenid", id);
                command.Parameters.AddWithValue("estado", anulado ? 0 : 1);
                command.Parameters.AddWithValue("isdeleted", anulado);
                command.Parameters.AddWithValue("fechaactualizacion", DateTime.UtcNow);
                command.Parameters.AddWithValue("actor", actorAuditoria);

                var rows = await command.ExecuteNonQueryAsync();
                if (rows == 0)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure(ErrorCodes.OrdenTrabajoNotFound, $"Orden de trabajo con ID {id} no encontrada");
                }

                await transaction.CommitAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result.Failure(ErrorCodes.DbError, $"Error al cambiar el estado de anulación de la orden de trabajo: {ex.Message}");
            }
        }

        private static async Task<Result<bool?>> ObtenerEstadoActualAsync(
            Npgsql.NpgsqlConnection connection,
            Npgsql.NpgsqlTransaction transaction,
            int ordenTrabajoId)
        {
            const string sql = @"
SELECT isdeleted
FROM ordentrabajo
WHERE ordentrabajoid = @ordenid
FOR UPDATE;";

            await using var command = new Npgsql.NpgsqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("ordenid", ordenTrabajoId);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return Result<bool?>.Success(null);
            }

            return Result<bool?>.Success(reader.GetBoolean(0));
        }

        private static async Task<Result<IReadOnlyCollection<OrdenTrabajoProducto>>> ObtenerProductosOrdenAsync(
            Npgsql.NpgsqlConnection connection,
            Npgsql.NpgsqlTransaction transaction,
            int ordenTrabajoId)
        {
            const string sql = @"
SELECT productoid, cantidad
FROM ordentrabajoproducto
WHERE ordentrabajoid = @ordenid
ORDER BY ordentrabajoproductoid;";

            var productos = new List<OrdenTrabajoProducto>();

            await using var command = new Npgsql.NpgsqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("ordenid", ordenTrabajoId);

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var productoId = reader.GetInt32(reader.GetOrdinal("productoid"));
                var cantidad = reader.GetInt32(reader.GetOrdinal("cantidad"));
                productos.Add(new OrdenTrabajoProducto(ordenTrabajoId, productoId, cantidad, 0, 0));
            }

            return Result<IReadOnlyCollection<OrdenTrabajoProducto>>.Success(productos);
        }

        private static async Task<Result> AjustarStockProductosAsync(
            Npgsql.NpgsqlConnection connection,
            Npgsql.NpgsqlTransaction transaction,
            IEnumerable<OrdenTrabajoProducto> productos,
            int movimiento,
            string actorAuditoria)
        {
            var productosAgrupados = productos
                .Where(p => p.ProductoId > 0 && p.Cantidad > 0)
                .GroupBy(p => p.ProductoId)
                .Select(grupo => new
                {
                    ProductoId = grupo.Key,
                    Cantidad = grupo.Sum(x => x.Cantidad)
                })
                .ToList();

            if (productosAgrupados.Count == 0)
            {
                return Result.Success();
            }

            foreach (var producto in productosAgrupados)
            {
                const string consultaProductoSql = @"
SELECT nombre, stock
FROM producto
WHERE productoid = @productoid
FOR UPDATE;";

                await using var consultaProducto = new Npgsql.NpgsqlCommand(consultaProductoSql, connection, transaction);
                consultaProducto.Parameters.AddWithValue("productoid", producto.ProductoId);

                string? nombreProducto = null;
                int stockActual;

                await using (var reader = await consultaProducto.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        return Result.Failure(ErrorCodes.ValidationInvalidValue, $"Producto con ID {producto.ProductoId} no encontrado.");
                    }

                    nombreProducto = reader.IsDBNull(reader.GetOrdinal("nombre")) ? null : reader.GetString(reader.GetOrdinal("nombre"));
                    stockActual = reader.GetInt32(reader.GetOrdinal("stock"));
                }

                var nuevoStock = stockActual + (movimiento * producto.Cantidad);
                if (nuevoStock < 0)
                {
                    return Result.Failure(
                        ErrorCodes.ValidationInvalidValue,
                        $"Stock insuficiente para el producto '{nombreProducto ?? producto.ProductoId.ToString()}'. Stock actual: {stockActual}.");
                }

                const string actualizarStockSql = @"
UPDATE producto
SET stock = @stock,
    fechaactualizacion = @fechaactualizacion,
    actualizadopor = @actualizadopor
WHERE productoid = @productoid;";

                await using var actualizarStock = new Npgsql.NpgsqlCommand(actualizarStockSql, connection, transaction);
                actualizarStock.Parameters.AddWithValue("productoid", producto.ProductoId);
                actualizarStock.Parameters.AddWithValue("stock", nuevoStock);
                actualizarStock.Parameters.AddWithValue("fechaactualizacion", DateTime.UtcNow);
                actualizarStock.Parameters.AddWithValue("actualizadopor", actorAuditoria);

                var filasAfectadas = await actualizarStock.ExecuteNonQueryAsync();
                if (filasAfectadas == 0)
                {
                    return Result.Failure(ErrorCodes.DbError, $"No se pudo actualizar el stock del producto con ID {producto.ProductoId}.");
                }
            }

            return Result.Success();
        }
    }
}