using Npgsql;
using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Interfaces;
using OrdenTrabajoService.Infrastructure.Persistence;
using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Application.Persistence;
using Taller_Mecanico_Users.Application.Services;

namespace OrdenTrabajoService.Infrastructure.Repositories
{
    public class OrdenTrabajoRepository : IOrdenTrabajoRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly OrdenTrabajoQueryService _queryService;
        private readonly IAuthenticationHelper _authHelper;

        public OrdenTrabajoRepository(
            ISqlConnectionFactory connectionFactory,
            OrdenTrabajoQueryService queryService,
            IAuthenticationHelper authHelper)
        {
            _connectionFactory = connectionFactory;
            _queryService = queryService;
            _authHelper = authHelper;
        }

        public async Task<IEnumerable<OrdenTrabajo>> GetAllAsync()
        {
            await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();
            return _queryService.LoadOrdenesTrabajo(connection)
                .OrderByDescending(o => o.FechaIngreso)
                .ToList();
        }

        public async Task<Result<OrdenTrabajo?>> GetByIdAsync(int id)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();

                var orden = _queryService.LoadOrdenesTrabajo(connection)
                    .FirstOrDefault(o => o.OrdenTrabajoId == id);

                return Result<OrdenTrabajo?>.Success(orden);
            }
            catch (Exception ex)
            {
                return Result<OrdenTrabajo?>.Failure(ErrorCodes.DbError,
                    $"Error al obtener orden de trabajo con ID {id}: {ex.Message}");
            }
        }

        // =====================================================================
        // AddAsync — TRANSACCIÓN ÚNICA: inserta la orden, sus detalles y
        // ajusta el stock de productos en un solo bloque atómico.
        // Si cualquier paso falla se hace ROLLBACK automático.
        // =====================================================================
        public async Task<Result<int>> AddAsync(OrdenTrabajo entity)
        {
            await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var actor = _authHelper.GetCurrentAuditActor();
                var fechaIngreso = entity.FechaIngreso == default ? DateTime.UtcNow : entity.FechaIngreso;

                const string sql = @"
INSERT INTO ordentrabajo
    (vehiculoid, fechaingreso, fechaentrega, estadotrabajo, estadopago,
     estadovehiculo, total, estado, isdeleted, fechaactualizacion, creadopor)
VALUES
    (@vehiculoid, @fechaingreso, @fechaentrega, @estadotrabajo, @estadopago,
     @estadovehiculo, @total, 1, FALSE, @fechaactualizacion, @creadopor)
RETURNING ordentrabajoid;";

                await using var cmd = new NpgsqlCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("vehiculoid", entity.VehiculoId);
                cmd.Parameters.AddWithValue("fechaingreso", fechaIngreso);
                cmd.Parameters.AddWithValue("fechaentrega", (object?)entity.FechaEntrega ?? DBNull.Value);
                cmd.Parameters.AddWithValue("estadotrabajo", entity.EstadoTrabajo.ToString());
                cmd.Parameters.AddWithValue("estadopago", entity.EstadoPago.ToString());
                cmd.Parameters.AddWithValue("estadovehiculo", entity.EstadoVehiculo);
                cmd.Parameters.AddWithValue("total", entity.Total);
                cmd.Parameters.AddWithValue("fechaactualizacion", (object?)entity.FechaActualizacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("creadopor", actor);

                var idObj = await cmd.ExecuteScalarAsync();
                if (idObj == null || idObj == DBNull.Value)
                {
                    await transaction.RollbackAsync();
                    return Result<int>.Failure(ErrorCodes.DbInsertFailed,
                        "No se pudo recuperar el ID de la orden registrada.");
                }

                var ordenId = Convert.ToInt32(idObj);

                await InsertarProductosAsync(connection, transaction, ordenId, entity.ProductosUsados, actor);
                await InsertarServiciosAsync(connection, transaction, ordenId, entity.ServiciosRealizados, actor);
                await InsertarMecanicosAsync(connection, transaction, ordenId, entity.MecanicosAsignados, actor);

                // Reducción de stock dentro de la misma transacción (COMMIT/ROLLBACK atómico)
                var stockResult = await AjustarStockProductosAsync(
                    connection, transaction, entity.ProductosUsados, movimiento: -1, actor);
                if (stockResult.IsFailure)
                {
                    await transaction.RollbackAsync();
                    return Result<int>.Failure(stockResult.ErrorCode!, stockResult.ErrorMessage!);
                }

                await transaction.CommitAsync();
                return Result<int>.Success(ordenId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<int>.Failure(ErrorCodes.DbError,
                    $"Error al registrar orden de trabajo: {ex.Message}");
            }
        }

        public async Task<Result> UpdateAsync(OrdenTrabajo entity)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();

                var actor = _authHelper.GetCurrentAuditActor();
                const string sql = @"
UPDATE ordentrabajo
SET vehiculoid = @vehiculoid, fechaingreso = @fechaingreso, fechaentrega = @fechaentrega,
    estadotrabajo = @estadotrabajo, estadopago = @estadopago, estadovehiculo = @estadovehiculo,
    total = @total, fechaactualizacion = @fechaactualizacion, actualizadopor = @actor
WHERE ordentrabajoid = @id;";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("id", entity.OrdenTrabajoId);
                cmd.Parameters.AddWithValue("vehiculoid", entity.VehiculoId);
                cmd.Parameters.AddWithValue("fechaingreso", entity.FechaIngreso);
                cmd.Parameters.AddWithValue("fechaentrega", (object?)entity.FechaEntrega ?? DBNull.Value);
                cmd.Parameters.AddWithValue("estadotrabajo", entity.EstadoTrabajo.ToString());
                cmd.Parameters.AddWithValue("estadopago", entity.EstadoPago.ToString());
                cmd.Parameters.AddWithValue("estadovehiculo", entity.EstadoVehiculo);
                cmd.Parameters.AddWithValue("total", entity.Total);
                cmd.Parameters.AddWithValue("fechaactualizacion", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("actor", actor);

                await cmd.ExecuteNonQueryAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorCodes.DbError,
                    $"Error al actualizar orden de trabajo: {ex.Message}");
            }
        }

        public async Task DeleteAsync(int id) => await SetAnuladoAsync(id, true);

        // =====================================================================
        // SetAnuladoAsync — DELETE LÓGICO con restauración/descuento de stock.
        // Usa FOR UPDATE para bloqueo a nivel de fila y evitar condiciones de carrera.
        // La restauración del stock ocurre dentro de la misma transacción.
        // =====================================================================
        public async Task<Result> SetAnuladoAsync(int id, bool anulado)
        {
            await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var actor = _authHelper.GetCurrentAuditActor();

                // Bloqueo de fila para evitar anulaciones concurrentes
                var estadoActual = await ObtenerEstadoActualAsync(connection, transaction, id);
                if (estadoActual.IsFailure)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure(estadoActual.ErrorCode!, estadoActual.ErrorMessage!);
                }

                if (!estadoActual.Value.HasValue)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure(ErrorCodes.OrdenTrabajoNotFound,
                        $"Orden de trabajo con ID {id} no encontrada.");
                }

                // Idempotencia: si ya está en el estado solicitado, commit sin cambios
                if (estadoActual.Value.Value == anulado)
                {
                    await transaction.CommitAsync();
                    return Result.Success();
                }

                // Obtener productos de la orden para ajustar el stock
                var productosResult = await ObtenerProductosOrdenAsync(connection, transaction, id);
                if (productosResult.IsFailure)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure(productosResult.ErrorCode!, productosResult.ErrorMessage!);
                }

                // Anular → devolver stock (+1); Restaurar → descontar stock (-1)
                var movimiento = anulado ? +1 : -1;
                var stockResult = await AjustarStockProductosAsync(
                    connection, transaction, productosResult.Value!, movimiento, actor);
                if (stockResult.IsFailure)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure(stockResult.ErrorCode!, stockResult.ErrorMessage!);
                }

                const string sqlUpdate = @"
UPDATE ordentrabajo
SET estado          = @estado,
    isdeleted       = @isdeleted,
    fechaactualizacion = @fecha,
    eliminadopor    = CASE WHEN @isdeleted THEN @actor ELSE NULL END,
    actualizadopor  = CASE WHEN NOT @isdeleted THEN @actor ELSE actualizadopor END
WHERE ordentrabajoid = @id;";

                await using var cmdUpdate = new NpgsqlCommand(sqlUpdate, connection, transaction);
                cmdUpdate.Parameters.AddWithValue("id", id);
                cmdUpdate.Parameters.AddWithValue("estado", anulado ? 0 : 1);
                cmdUpdate.Parameters.AddWithValue("isdeleted", anulado);
                cmdUpdate.Parameters.AddWithValue("fecha", DateTime.UtcNow);
                cmdUpdate.Parameters.AddWithValue("actor", actor);

                var rows = await cmdUpdate.ExecuteNonQueryAsync();
                if (rows == 0)
                {
                    await transaction.RollbackAsync();
                    return Result.Failure(ErrorCodes.OrdenTrabajoNotFound,
                        $"Orden de trabajo con ID {id} no encontrada.");
                }

                await transaction.CommitAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result.Failure(ErrorCodes.DbError,
                    $"Error al cambiar estado de anulación: {ex.Message}");
            }
        }

        // =====================================================================
        // Helpers privados
        // =====================================================================

        private static async Task InsertarProductosAsync(
            NpgsqlConnection connection, NpgsqlTransaction transaction,
            int ordenId, IReadOnlyCollection<OrdenTrabajoProducto> productos, string actor)
        {
            if (productos.Count == 0) return;
            const string sql = @"
INSERT INTO ordentrabajoproducto
    (ordentrabajoid, productoid, cantidad, preciounitario, subtotal, creadopor)
VALUES (@ordenid, @productoid, @cantidad, @precio, @subtotal, @actor);";

            foreach (var p in productos)
            {
                await using var cmd = new NpgsqlCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("ordenid", ordenId);
                cmd.Parameters.AddWithValue("productoid", p.ProductoId);
                cmd.Parameters.AddWithValue("cantidad", p.Cantidad);
                cmd.Parameters.AddWithValue("precio", Convert.ToDecimal(p.PrecioUnitario));
                cmd.Parameters.AddWithValue("subtotal", Convert.ToDecimal(p.Subtotal));
                cmd.Parameters.AddWithValue("actor", actor);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private static async Task InsertarServiciosAsync(
            NpgsqlConnection connection, NpgsqlTransaction transaction,
            int ordenId, IReadOnlyCollection<OrdenTrabajoServicio> servicios, string actor)
        {
            if (servicios.Count == 0) return;
            const string sql = @"
INSERT INTO ordentrabajoservicio
    (ordentrabajoid, servicioid, cantidad, preciounitario, subtotal, creadopor)
VALUES (@ordenid, @servicioid, @cantidad, @precio, @subtotal, @actor);";

            foreach (var s in servicios)
            {
                await using var cmd = new NpgsqlCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("ordenid", ordenId);
                cmd.Parameters.AddWithValue("servicioid", s.ServicioId);
                cmd.Parameters.AddWithValue("cantidad", s.Cantidad);
                cmd.Parameters.AddWithValue("precio", Convert.ToDecimal(s.PrecioUnitario));
                cmd.Parameters.AddWithValue("subtotal", Convert.ToDecimal(s.Subtotal));
                cmd.Parameters.AddWithValue("actor", actor);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private static async Task InsertarMecanicosAsync(
            NpgsqlConnection connection, NpgsqlTransaction transaction,
            int ordenId, IReadOnlyCollection<OrdenTrabajoMecanico> mecanicos, string actor)
        {
            if (mecanicos.Count == 0) return;
            const string sql = @"
INSERT INTO ordentrabajomecanico (ordentrabajoid, mecanicoid, fechaasignacion)
VALUES (@ordenid, @mecanicoid, @fecha)
ON CONFLICT DO NOTHING;";

            foreach (var m in mecanicos)
            {
                await using var cmd = new NpgsqlCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("ordenid", ordenId);
                cmd.Parameters.AddWithValue("mecanicoid", m.MecanicoId);
                cmd.Parameters.AddWithValue("fecha", m.FechaAsignacion);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        private static async Task<Result<bool?>> ObtenerEstadoActualAsync(
            NpgsqlConnection connection, NpgsqlTransaction transaction, int id)
        {
            const string sql = "SELECT isdeleted FROM ordentrabajo WHERE ordentrabajoid = @id FOR UPDATE;";
            await using var cmd = new NpgsqlCommand(sql, connection, transaction);
            cmd.Parameters.AddWithValue("id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return Result<bool?>.Success(null);

            return Result<bool?>.Success(reader.GetBoolean(0));
        }

        private static async Task<Result<IReadOnlyCollection<OrdenTrabajoProducto>>> ObtenerProductosOrdenAsync(
            NpgsqlConnection connection, NpgsqlTransaction transaction, int id)
        {
            const string sql = @"
SELECT productoid, cantidad FROM ordentrabajoproducto
WHERE ordentrabajoid = @id ORDER BY ordentrabajoproductoid;";

            var lista = new List<OrdenTrabajoProducto>();
            await using var cmd = new NpgsqlCommand(sql, connection, transaction);
            cmd.Parameters.AddWithValue("id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                lista.Add(new OrdenTrabajoProducto(id, reader.GetInt32(0), reader.GetInt32(1), 0, 0));

            return Result<IReadOnlyCollection<OrdenTrabajoProducto>>.Success(lista);
        }

        // =====================================================================
        // AjustarStockProductosAsync
        // movimiento = -1 → reducir stock (crear/restaurar orden)
        // movimiento = +1 → devolver stock (anular orden)
        // Usa FOR UPDATE en cada producto para bloqueo a nivel de fila.
        // =====================================================================
        private static async Task<Result> AjustarStockProductosAsync(
            NpgsqlConnection connection, NpgsqlTransaction transaction,
            IEnumerable<OrdenTrabajoProducto> productos, int movimiento, string actor)
        {
            var agrupados = productos
                .Where(p => p.ProductoId > 0 && p.Cantidad > 0)
                .GroupBy(p => p.ProductoId)
                .Select(g => new { ProductoId = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
                .ToList();

            if (agrupados.Count == 0) return Result.Success();

            foreach (var item in agrupados)
            {
                const string sqlLeer = "SELECT nombre, stock FROM producto WHERE productoid = @id FOR UPDATE;";
                await using var cmdLeer = new NpgsqlCommand(sqlLeer, connection, transaction);
                cmdLeer.Parameters.AddWithValue("id", item.ProductoId);

                string? nombre = null;
                int stockActual;

                await using (var reader = await cmdLeer.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                        return Result.Failure(ErrorCodes.ValidationInvalidValue,
                            $"Producto con ID {item.ProductoId} no encontrado.");

                    nombre = reader.IsDBNull(0) ? null : reader.GetString(0);
                    stockActual = reader.GetInt32(1);
                }

                var nuevoStock = stockActual + (movimiento * item.Cantidad);
                if (nuevoStock < 0)
                    return Result.Failure(ErrorCodes.ValidationInvalidValue,
                        $"Stock insuficiente para '{nombre ?? item.ProductoId.ToString()}'. Stock actual: {stockActual}.");

                const string sqlActualizar = @"
UPDATE producto
SET stock = @stock, fechaactualizacion = @fecha, actualizadopor = @actor
WHERE productoid = @id;";

                await using var cmdActualizar = new NpgsqlCommand(sqlActualizar, connection, transaction);
                cmdActualizar.Parameters.AddWithValue("stock", nuevoStock);
                cmdActualizar.Parameters.AddWithValue("fecha", DateTime.UtcNow);
                cmdActualizar.Parameters.AddWithValue("actor", actor);
                cmdActualizar.Parameters.AddWithValue("id", item.ProductoId);

                var rows = await cmdActualizar.ExecuteNonQueryAsync();
                if (rows == 0)
                    return Result.Failure(ErrorCodes.DbError,
                        $"No se pudo actualizar el stock del producto con ID {item.ProductoId}.");
            }

            return Result.Success();
        }
    }
}

