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
            try
            {
                await using var connection = _connectionFactory.CreateConnection();
                await connection.OpenAsync();

                await using var transaction = await connection.BeginTransactionAsync();

            var sql = @"
INSERT INTO ordentrabajo (vehiculoid, fechaingreso, fechaentrega, estadotrabajo, estadopago, estadovehiculo, total, isdeleted, fechaactualizacion, creadopor)
VALUES (@vehiculoid, @fechaingreso, @fechaentrega, @estadotrabajo, @estadopago, @estadovehiculo, @total, FALSE, @fechaactualizacion, @creadopor)
RETURNING ordentrabajoid;";

                var actorAuditoria = _authHelper.GetCurrentAuditActor();

                await using var command = new Npgsql.NpgsqlCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("vehiculoid", entity.VehiculoId);
                command.Parameters.AddWithValue("fechaingreso", entity.FechaIngreso == default ? DateTime.UtcNow : entity.FechaIngreso);
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

                await transaction.CommitAsync();
                return Result<int>.Success(ordenTrabajoId);
            }
            catch (Exception ex)
            {
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
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var sql = "UPDATE ordentrabajo SET isdeleted = TRUE, fechaactualizacion = @fechaactualizacion, eliminadopor = @eliminadopor WHERE ordentrabajoid = @ordenid;";

            var actorAuditoria = _authHelper.GetCurrentAuditActor();

            await using var command = new Npgsql.NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("ordenid", id);
            command.Parameters.AddWithValue("fechaactualizacion", DateTime.UtcNow);
            command.Parameters.AddWithValue("eliminadopor", actorAuditoria);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<Result> SetAnuladoAsync(int id, bool anulado)
        {
            try
            {
                await using var connection = _connectionFactory.CreateConnection();
                await connection.OpenAsync();

                var sql = @"
UPDATE ordentrabajo
SET isdeleted = @isdeleted,
    fechaactualizacion = @fechaactualizacion,
    eliminadopor = CASE WHEN @isdeleted THEN @actor ELSE NULL END,
    actualizadopor = CASE WHEN NOT @isdeleted THEN @actor ELSE actualizadopor END
WHERE ordentrabajoid = @ordenid;";

                var actorAuditoria = _authHelper.GetCurrentAuditActor();

                await using var command = new Npgsql.NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("ordenid", id);
                command.Parameters.AddWithValue("isdeleted", anulado);
                command.Parameters.AddWithValue("fechaactualizacion", DateTime.UtcNow);
                command.Parameters.AddWithValue("actor", actorAuditoria);

                var rows = await command.ExecuteNonQueryAsync();
                if (rows == 0)
                    return Result.Failure(ErrorCodes.OrdenTrabajoNotFound, $"Orden de trabajo con ID {id} no encontrada");

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorCodes.DbError, $"Error al cambiar el estado de anulación de la orden de trabajo: {ex.Message}");
            }
        }
    }
}