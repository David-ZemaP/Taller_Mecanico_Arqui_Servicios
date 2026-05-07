using Npgsql;
using OrdenTrabajoService.Infrastructure.Persistence;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Enums;
using Taller_Mecanico_Arqui.Domain.Ports;
using OrdenTrabajoEntity = Taller_Mecanico_Arqui.Domain.Entities.OrdenTrabajo;

namespace OrdenTrabajoService.Infrastructure.Repositories
{
    public class OrdenTrabajoRepository : IOrdenTrabajoRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public OrdenTrabajoRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<OrdenTrabajoEntity>> GetAllAsync()
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT ordentrabajoid, vehiculoid, fechaingreso, fechaentrega, estadotrabajo, estadopago, estadovehiculo, total, isdeleted, fechaactualizacion
FROM ordentrabajo
WHERE isdeleted = FALSE
ORDER BY fechaingreso DESC;";

            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            var ordenes = new List<OrdenTrabajoEntity>();
            while (await reader.ReadAsync())
            {
                ordenes.Add(OrdenTrabajo.Reconstituir(
                    reader.GetInt32(0),
                    reader.GetInt32(1),
                    reader.GetDateTime(2),
                    reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                    Enum.Parse<EstadoTrabajo>(reader.GetString(4), true),
                    Enum.Parse<EstadoPago>(reader.GetString(5), true),
                    reader.GetString(6),
                    Convert.ToDouble(reader.GetDecimal(7)),
                    reader.GetBoolean(8),
                    reader.IsDBNull(9) ? null : reader.GetDateTime(9)
                ));
            }

            return ordenes;
        }

        public async Task<Result<OrdenTrabajoEntity?>> GetByIdAsync(int id)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT ordentrabajoid, vehiculoid, fechaingreso, fechaentrega, estadotrabajo, estadopago, estadovehiculo, total, isdeleted, fechaactualizacion
FROM ordentrabajo
WHERE ordentrabajoid = @id;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("id", id);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return Result<OrdenTrabajoEntity?>.Success(null);
            }

            var orden = OrdenTrabajo.Reconstituir(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetDateTime(2),
                reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                Enum.Parse<EstadoTrabajo>(reader.GetString(4), true),
                Enum.Parse<EstadoPago>(reader.GetString(5), true),
                reader.GetString(6),
                Convert.ToDouble(reader.GetDecimal(7)),
                reader.GetBoolean(8),
                reader.IsDBNull(9) ? null : reader.GetDateTime(9)
            );

            return Result<OrdenTrabajo?>.Success(orden);
        }

        public async Task<Result<int>> AddAsync(OrdenTrabajoEntity entity)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                const string insertSql = @"
INSERT INTO ordentrabajo (vehiculoid, fechaingreso, fechaentrega, estadotrabajo, estadopago, estadovehiculo, total, estado, isdeleted, creadopor)
VALUES (@vehiculoid, @fechaingreso, @fechaentrega, @estadotrabajo, @estadopago, @estadovehiculo, @total, 1, FALSE, @creadopor)
RETURNING ordentrabajoid;";

                await using var command = new NpgsqlCommand(insertSql, connection, transaction);
                command.Parameters.AddWithValue("vehiculoid", entity.VehiculoId);
                command.Parameters.AddWithValue("fechaingreso", entity.FechaIngreso);
                command.Parameters.AddWithValue("fechaentrega", (object?)entity.FechaEntrega ?? DBNull.Value);
                command.Parameters.AddWithValue("estadotrabajo", entity.EstadoTrabajo.ToString());
                command.Parameters.AddWithValue("estadopago", entity.EstadoPago.ToString());
                command.Parameters.AddWithValue("estadovehiculo", entity.EstadoVehiculo);
                command.Parameters.AddWithValue("total", Convert.ToDecimal(entity.Total));
                command.Parameters.AddWithValue("creadopor", (object?)entity.CreadoPor ?? DBNull.Value);

                var idObj = await command.ExecuteScalarAsync();
                if (idObj == null || idObj == DBNull.Value)
                {
                    await transaction.RollbackAsync();
                    return Result<int>.Failure(ErrorCodes.DbInsertFailed, "No se pudo crear la orden de trabajo.");
                }

                var ordenTrabajoId = Convert.ToInt32(idObj);

                await InsertarProductosAsync(connection, transaction, ordenTrabajoId, entity.ProductosUsados);
                await InsertarServiciosAsync(connection, transaction, ordenTrabajoId, entity.ServiciosRealizados);

                await transaction.CommitAsync();
                return Result<int>.Success(ordenTrabajoId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Result<int>.Failure(ErrorCodes.DbError, $"Error al registrar orden de trabajo: {ex.Message}");
            }
        }

        public async Task<Result> UpdateAsync(OrdenTrabajoEntity entity)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
UPDATE ordentrabajo
SET estadotrabajo = @estadotrabajo,
    estadopago = @estadopago,
    total = @total,
    fechaactualizacion = @fechaactualizacion,
    actualizadopor = @actualizadopor
WHERE ordentrabajoid = @ordenid;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("ordenid", entity.OrdenTrabajoId);
            command.Parameters.AddWithValue("estadotrabajo", entity.EstadoTrabajo.ToString());
            command.Parameters.AddWithValue("estadopago", entity.EstadoPago.ToString());
            command.Parameters.AddWithValue("total", Convert.ToDecimal(entity.Total));
            command.Parameters.AddWithValue("fechaactualizacion", DateTime.UtcNow);
            command.Parameters.AddWithValue("actualizadopor", (object?)entity.ActualizadoPor ?? DBNull.Value);

            await command.ExecuteNonQueryAsync();
            return Result.Success();
        }

        public async Task DeleteAsync(int id, string? eliminadoPor = null)
        {
            await SetAnuladoAsync(id, true, eliminadoPor);
        }

        public async Task<Result> SetAnuladoAsync(int id, bool anulado, string? eliminadoPor = null)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
UPDATE ordentrabajo
SET estado = @estado,
    isdeleted = @isdeleted,
    fechaactualizacion = @fechaactualizacion,
    eliminadopor = @eliminadoPor
WHERE ordentrabajoid = @ordenid;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("ordenid", id);
            command.Parameters.AddWithValue("estado", anulado ? 0 : 1);
            command.Parameters.AddWithValue("isdeleted", anulado);
            command.Parameters.AddWithValue("fechaactualizacion", DateTime.UtcNow);
            command.Parameters.AddWithValue("eliminadoPor", (object?)eliminadoPor ?? DBNull.Value);

            await command.ExecuteNonQueryAsync();
            return Result.Success();
        }

        private static async Task InsertarProductosAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            int ordenTrabajoId,
            IReadOnlyCollection<OrdenTrabajoProducto> productos)
        {
            if (productos.Count == 0)
            {
                return;
            }

            const string sql = @"
INSERT INTO ordentrabajoproducto (ordentrabajoid, productoid, cantidad, preciounitario, subtotal)
VALUES (@ordentrabajoid, @productoid, @cantidad, @preciounitario, @subtotal);";

            foreach (var producto in productos)
            {
                await using var command = new NpgsqlCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("ordentrabajoid", ordenTrabajoId);
                command.Parameters.AddWithValue("productoid", producto.ProductoId);
                command.Parameters.AddWithValue("cantidad", producto.Cantidad);
                command.Parameters.AddWithValue("preciounitario", Convert.ToDecimal(producto.PrecioUnitario));
                command.Parameters.AddWithValue("subtotal", Convert.ToDecimal(producto.Subtotal));
                await command.ExecuteNonQueryAsync();
            }
        }

        private static async Task InsertarServiciosAsync(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            int ordenTrabajoId,
            IReadOnlyCollection<OrdenTrabajoServicio> servicios)
        {
            if (servicios.Count == 0)
            {
                return;
            }

            const string sql = @"
INSERT INTO ordentrabajoservicio (ordentrabajoid, servicioid, cantidad, preciounitario, subtotal)
VALUES (@ordentrabajoid, @servicioid, @cantidad, @preciounitario, @subtotal);";

            foreach (var servicio in servicios)
            {
                await using var command = new NpgsqlCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("ordentrabajoid", ordenTrabajoId);
                command.Parameters.AddWithValue("servicioid", servicio.ServicioId);
                command.Parameters.AddWithValue("cantidad", servicio.Cantidad);
                command.Parameters.AddWithValue("preciounitario", Convert.ToDecimal(servicio.PrecioUnitario));
                command.Parameters.AddWithValue("subtotal", Convert.ToDecimal(servicio.Subtotal));
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
