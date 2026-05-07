using Npgsql;
using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Interfaces;
using OrdenTrabajoService.Infrastructure.Persistence;
using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Framework.Persistence;
using Taller_Mecanico_Users.Framework.Services;

namespace OrdenTrabajoService.Infrastructure.Repositories
{
    public class ClienteRepository : IRepository<Cliente>
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly IAuthenticationHelper _authHelper;

        public ClienteRepository(ISqlConnectionFactory connectionFactory, IAuthenticationHelper authHelper)
        {
            _connectionFactory = connectionFactory;
            _authHelper = authHelper;
        }

        public async Task<IEnumerable<Cliente>> GetAllAsync()
        {
            await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT clienteid, nombre, primerapellido, segundoapellido, ci, cicomplemento, telefono, email, isdeleted, fecharegistro
FROM cliente WHERE NOT isdeleted ORDER BY primerapellido, nombre;";

            var clientes = new List<Cliente>();
            await using var cmd = new NpgsqlCommand(sql, connection);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                clientes.Add(Leer(reader));
            }
            return clientes;
        }

        public async Task<Result<Cliente?>> GetByIdAsync(int id)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();

                const string sql = @"
SELECT clienteid, nombre, primerapellido, segundoapellido, ci, cicomplemento, telefono, email, isdeleted, fecharegistro
FROM cliente WHERE clienteid = @id AND NOT isdeleted;";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("id", id);
                await using var reader = await cmd.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                    return Result<Cliente?>.Success(null);

                return Result<Cliente?>.Success(Leer(reader));
            }
            catch (Exception ex)
            {
                return Result<Cliente?>.Failure(ErrorCodes.DbError, $"Error al obtener cliente: {ex.Message}");
            }
        }

        public async Task<Result<int>> AddAsync(Cliente entity)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();

                var actor = _authHelper.GetCurrentAuditActor();
                const string sql = @"
INSERT INTO cliente (nombre, primerapellido, segundoapellido, ci, cicomplemento, telefono, email, creadopor)
VALUES (@nombre, @primerapellido, @segundoapellido, @ci, @cicomplemento, @telefono, @email, @actor)
RETURNING clienteid;";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("nombre", entity.Nombre);
                cmd.Parameters.AddWithValue("primerapellido", entity.PrimerApellido);
                cmd.Parameters.AddWithValue("segundoapellido", (object?)entity.SegundoApellido ?? DBNull.Value);
                cmd.Parameters.AddWithValue("ci", entity.Ci);
                cmd.Parameters.AddWithValue("cicomplemento", (object?)entity.CiComplemento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("telefono", entity.Telefono);
                cmd.Parameters.AddWithValue("email", entity.Email);
                cmd.Parameters.AddWithValue("actor", actor);

                var id = await cmd.ExecuteScalarAsync();
                return Result<int>.Success(Convert.ToInt32(id));
            }
            catch (Exception ex)
            {
                return Result<int>.Failure(ErrorCodes.DbError, $"Error al registrar cliente: {ex.Message}");
            }
        }

        public async Task<Result> UpdateAsync(Cliente entity)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();

                var actor = _authHelper.GetCurrentAuditActor();
                const string sql = @"
UPDATE cliente
SET nombre = @nombre, primerapellido = @primerapellido, segundoapellido = @segundoapellido,
    ci = @ci, cicomplemento = @cicomplemento, telefono = @telefono, email = @email,
    actualizadopor = @actor, fechaactualizacion = @fecha
WHERE clienteid = @id AND NOT isdeleted;";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("nombre", entity.Nombre);
                cmd.Parameters.AddWithValue("primerapellido", entity.PrimerApellido);
                cmd.Parameters.AddWithValue("segundoapellido", (object?)entity.SegundoApellido ?? DBNull.Value);
                cmd.Parameters.AddWithValue("ci", entity.Ci);
                cmd.Parameters.AddWithValue("cicomplemento", (object?)entity.CiComplemento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("telefono", entity.Telefono);
                cmd.Parameters.AddWithValue("email", entity.Email);
                cmd.Parameters.AddWithValue("actor", actor);
                cmd.Parameters.AddWithValue("fecha", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("id", entity.ClienteId);

                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    return Result.Failure(ErrorCodes.DbError, $"Cliente con ID {entity.ClienteId} no encontrado.");
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorCodes.DbError, $"Error al actualizar cliente: {ex.Message}");
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();

                var actor = _authHelper.GetCurrentAuditActor();
                const string sql = @"UPDATE cliente SET isdeleted = TRUE, eliminadopor = @actor, fechaactualizacion = @fecha WHERE clienteid = @id;";
                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("actor", actor);
                cmd.Parameters.AddWithValue("fecha", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("id", id);
                await cmd.ExecuteNonQueryAsync();
            }
            catch { }
        }

        private static Cliente Leer(NpgsqlDataReader reader)
        {
            return Cliente.Reconstituir(
                reader.GetInt32(reader.GetOrdinal("clienteid")),
                reader.GetString(reader.GetOrdinal("nombre")),
                reader.GetString(reader.GetOrdinal("primerapellido")),
                reader.IsDBNull(reader.GetOrdinal("segundoapellido")) ? null : reader.GetString(reader.GetOrdinal("segundoapellido")),
                reader.GetInt32(reader.GetOrdinal("ci")),
                reader.IsDBNull(reader.GetOrdinal("cicomplemento")) ? null : reader.GetString(reader.GetOrdinal("cicomplemento")),
                reader.GetInt32(reader.GetOrdinal("telefono")),
                reader.GetString(reader.GetOrdinal("email")),
                reader.GetBoolean(reader.GetOrdinal("isdeleted")),
                reader.IsDBNull(reader.GetOrdinal("fecharegistro")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("fecharegistro")));
        }
    }
}
