using Npgsql;
using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Interfaces;
using OrdenTrabajoService.Infrastructure.Persistence;
using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Application.Persistence;
using Taller_Mecanico_Users.Application.Services;

namespace OrdenTrabajoService.Infrastructure.Repositories
{
    public class ServicioRepository : IRepository<Servicio>
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly OrdenTrabajoQueryService _queryService;
        private readonly IAuthenticationHelper _authHelper;

        public ServicioRepository(
            ISqlConnectionFactory connectionFactory,
            OrdenTrabajoQueryService queryService,
            IAuthenticationHelper authHelper)
        {
            _connectionFactory = connectionFactory;
            _queryService = queryService;
            _authHelper = authHelper;
        }

        public async Task<IEnumerable<Servicio>> GetAllAsync()
        {
            await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();
            return _queryService.LoadServicios(connection);
        }

        public async Task<Result<Servicio?>> GetByIdAsync(int id)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();

                const string sql = "SELECT servicioid, nombre, precio FROM servicio WHERE servicioid = @id AND NOT isdeleted;";
                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("id", id);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    return Result<Servicio?>.Success(null);

                return Result<Servicio?>.Success(Servicio.Reconstituir(
                    reader.GetInt32(reader.GetOrdinal("servicioid")),
                    reader.GetString(reader.GetOrdinal("nombre")),
                    Convert.ToDouble(reader.GetDecimal(reader.GetOrdinal("precio")))));
            }
            catch (Exception ex)
            {
                return Result<Servicio?>.Failure(ErrorCodes.DbError,
                    $"Error al obtener servicio: {ex.Message}");
            }
        }

        public async Task<Result<int>> AddAsync(Servicio entity)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();

                var actor = _authHelper.GetCurrentAuditActor();
                const string sql = @"
INSERT INTO servicio (nombre, precio, creadopor) VALUES (@nombre, @precio, @actor) RETURNING servicioid;";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("nombre", entity.Nombre);
                cmd.Parameters.AddWithValue("precio", Convert.ToDecimal(entity.Precio));
                cmd.Parameters.AddWithValue("actor", actor);

                var id = await cmd.ExecuteScalarAsync();
                return Result<int>.Success(Convert.ToInt32(id));
            }
            catch (Exception ex)
            {
                return Result<int>.Failure(ErrorCodes.DbError,
                    $"Error al registrar servicio: {ex.Message}");
            }
        }

        public async Task<Result> UpdateAsync(Servicio entity)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();

                var actor = _authHelper.GetCurrentAuditActor();
                const string sql = @"
UPDATE servicio
SET nombre = @nombre, precio = @precio, actualizadopor = @actor, fechaactualizacion = @fecha
WHERE servicioid = @id AND NOT isdeleted;";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("nombre", entity.Nombre);
                cmd.Parameters.AddWithValue("precio", Convert.ToDecimal(entity.Precio));
                cmd.Parameters.AddWithValue("actor", actor);
                cmd.Parameters.AddWithValue("fecha", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("id", entity.ServicioId);

                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    return Result.Failure(ErrorCodes.NotFound,
                        $"Servicio con ID {entity.ServicioId} no encontrado.");
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorCodes.DbError,
                    $"Error al actualizar servicio: {ex.Message}");
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();

                var actor = _authHelper.GetCurrentAuditActor();
                const string sql = "UPDATE servicio SET isdeleted = TRUE, eliminadopor = @actor, fechaactualizacion = @fecha WHERE servicioid = @id;";
                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("actor", actor);
                cmd.Parameters.AddWithValue("fecha", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("id", id);
                await cmd.ExecuteNonQueryAsync();
            }
            catch { }
        }
    }
}

