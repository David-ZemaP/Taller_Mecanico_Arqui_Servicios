using Npgsql;
using OrdenTrabajoService.Infrastructure.Persistence;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Enums;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Domain.ValueObjects;

namespace OrdenTrabajoService.Infrastructure.Repositories
{
    public class ClienteRepository : IClienteRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public ClienteRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Cliente>> GetAllAsync()
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT c.clienteid, c.nombre, c.primerapellido, c.segundoapellido, c.ci, c.cicomplemento,
       c.telefono, c.email, c.fecharegistro, c.tipocliente, c.usuariologinid,
       c.fechaactualizacion, c.isdeleted
FROM cliente c
WHERE c.isdeleted = FALSE
ORDER BY c.primerapellido, c.nombre;";

            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            var clientes = new List<Cliente>();
            while (await reader.ReadAsync())
            {
                var nombreResult = NombreCompleto.Crear(
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.IsDBNull(3) ? null : reader.GetString(3));

                var ciResult = DocumentoIdentidad.Crear(
                    reader.GetInt32(4),
                    reader.IsDBNull(5) ? null : reader.GetString(5));

                if (nombreResult.IsFailure || ciResult.IsFailure)
                    continue;

                clientes.Add(Cliente.Reconstituir(
                    reader.GetInt32(0),
                    nombreResult.Value,
                    ciResult.Value,
                    reader.GetInt32(6),
                    reader.IsDBNull(7) ? null : reader.GetString(7),
                    reader.GetDateTime(8),
                    Enum.Parse<TipoCliente>(reader.GetString(9), true),
                    reader.IsDBNull(10) ? null : reader.GetInt32(10),
                    reader.IsDBNull(11) ? null : reader.GetDateTime(11),
                    reader.GetBoolean(12)
                ));
            }

            return clientes;
        }

        public async Task<Result<Cliente?>> GetByIdAsync(int id)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT c.clienteid, c.nombre, c.primerapellido, c.segundoapellido, c.ci, c.cicomplemento,
       c.telefono, c.email, c.fecharegistro, c.tipocliente, c.usuariologinid,
       c.fechaactualizacion, c.isdeleted
FROM cliente c
WHERE c.clienteid = @id AND c.isdeleted = FALSE;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("id", id);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return Result<Cliente?>.Success(null);

            var nombreResult = NombreCompleto.Crear(
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3));

            var ciResult = DocumentoIdentidad.Crear(
                reader.GetInt32(4),
                reader.IsDBNull(5) ? null : reader.GetString(5));

            if (nombreResult.IsFailure || ciResult.IsFailure)
                return Result<Cliente?>.Success(null);

            var cliente = Cliente.Reconstituir(
                reader.GetInt32(0),
                nombreResult.Value,
                ciResult.Value,
                reader.GetInt32(6),
                reader.IsDBNull(7) ? null : reader.GetString(7),
                reader.GetDateTime(8),
                Enum.Parse<TipoCliente>(reader.GetString(9), true),
                reader.IsDBNull(10) ? null : reader.GetInt32(10),
                reader.IsDBNull(11) ? null : reader.GetDateTime(11),
                reader.GetBoolean(12)
            );

            return Result<Cliente?>.Success(cliente);
        }

        public async Task<Cliente?> GetByCiAsync(int ci)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT c.clienteid, c.nombre, c.primerapellido, c.segundoapellido, c.ci, c.cicomplemento,
       c.telefono, c.email, c.fecharegistro, c.tipocliente, c.usuariologinid,
       c.fechaactualizacion, c.isdeleted
FROM cliente c
WHERE c.ci = @ci AND c.isdeleted = FALSE;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("ci", ci);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            var nombreResult = NombreCompleto.Crear(
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3));

            var ciResult = DocumentoIdentidad.Crear(
                reader.GetInt32(4),
                reader.IsDBNull(5) ? null : reader.GetString(5));

            if (nombreResult.IsFailure || ciResult.IsFailure)
                return null;

            return Cliente.Reconstituir(
                reader.GetInt32(0),
                nombreResult.Value,
                ciResult.Value,
                reader.GetInt32(6),
                reader.IsDBNull(7) ? null : reader.GetString(7),
                reader.GetDateTime(8),
                Enum.Parse<TipoCliente>(reader.GetString(9), true),
                reader.IsDBNull(10) ? null : reader.GetInt32(10),
                reader.IsDBNull(11) ? null : reader.GetDateTime(11),
                reader.GetBoolean(12)
            );
        }

        public async Task<Cliente?> GetByEmailAsync(string email)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT c.clienteid, c.nombre, c.primerapellido, c.segundoapellido, c.ci, c.cicomplemento,
       c.telefono, c.email, c.fecharegistro, c.tipocliente, c.usuariologinid,
       c.fechaactualizacion, c.isdeleted
FROM cliente c
WHERE c.email = @email AND c.isdeleted = FALSE;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("email", email);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            var nombreResult = NombreCompleto.Crear(
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3));

            var ciResult = DocumentoIdentidad.Crear(
                reader.GetInt32(4),
                reader.IsDBNull(5) ? null : reader.GetString(5));

            if (nombreResult.IsFailure || ciResult.IsFailure)
                return null;

            return Cliente.Reconstituir(
                reader.GetInt32(0),
                nombreResult.Value,
                ciResult.Value,
                reader.GetInt32(6),
                reader.IsDBNull(7) ? null : reader.GetString(7),
                reader.GetDateTime(8),
                Enum.Parse<TipoCliente>(reader.GetString(9), true),
                reader.IsDBNull(10) ? null : reader.GetInt32(10),
                reader.IsDBNull(11) ? null : reader.GetDateTime(11),
                reader.GetBoolean(12)
            );
        }

        public async Task<Result<int>> AddAsync(Cliente entity)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
INSERT INTO cliente (nombre, primerapellido, segundoapellido, ci, cicomplemento,
                     telefono, email, fecharegistro, tipocliente, usuariologinid, isdeleted, creadopor)
VALUES (@nombre, @primerapellido, @segundoapellido, @ci, @cicomplemento,
        @telefono, @email, @fecharegistro, @tipocliente, @usuariologinid, FALSE, @creadopor)
RETURNING clienteid;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("nombre", entity.NombreCompleto.Nombres);
            command.Parameters.AddWithValue("primerapellido", entity.NombreCompleto.PrimerApellido);
            command.Parameters.AddWithValue("segundoapellido", (object?)entity.NombreCompleto.SegundoApellido ?? DBNull.Value);
            command.Parameters.AddWithValue("ci", entity.Ci.Numero);
            command.Parameters.AddWithValue("cicomplemento", (object?)entity.Ci.Complemento ?? DBNull.Value);
            command.Parameters.AddWithValue("telefono", entity.Telefono);
            command.Parameters.AddWithValue("email", (object?)entity.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("fecharegistro", entity.FechaRegistro);
            command.Parameters.AddWithValue("tipocliente", entity.TipoCliente.ToString());
            command.Parameters.AddWithValue("usuariologinid", (object?)entity.UsuarioLoginId ?? DBNull.Value);
            command.Parameters.AddWithValue("creadopor", (object?)entity.CreadoPor ?? DBNull.Value);

            var result = await command.ExecuteScalarAsync();
            if (result == null || result == DBNull.Value)
                return Result<int>.Failure(ErrorCodes.DbInsertFailed, "No se pudo registrar el cliente.");

            return Result<int>.Success(Convert.ToInt32(result));
        }

        public async Task<Result> UpdateAsync(Cliente entity)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
UPDATE cliente
SET nombre = @nombre,
    primerapellido = @primerapellido,
    segundoapellido = @segundoapellido,
    ci = @ci,
    cicomplemento = @cicomplemento,
    telefono = @telefono,
    email = @email,
    tipocliente = @tipocliente,
    usuariologinid = @usuariologinid,
    fechaactualizacion = @fechaactualizacion,
    actualizadopor = @actualizadopor
WHERE clienteid = @clienteid;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("clienteid", entity.ClienteId);
            command.Parameters.AddWithValue("nombre", entity.NombreCompleto.Nombres);
            command.Parameters.AddWithValue("primerapellido", entity.NombreCompleto.PrimerApellido);
            command.Parameters.AddWithValue("segundoapellido", (object?)entity.NombreCompleto.SegundoApellido ?? DBNull.Value);
            command.Parameters.AddWithValue("ci", entity.Ci.Numero);
            command.Parameters.AddWithValue("cicomplemento", (object?)entity.Ci.Complemento ?? DBNull.Value);
            command.Parameters.AddWithValue("telefono", entity.Telefono);
            command.Parameters.AddWithValue("email", (object?)entity.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("tipocliente", entity.TipoCliente.ToString());
            command.Parameters.AddWithValue("usuariologinid", (object?)entity.UsuarioLoginId ?? DBNull.Value);
            command.Parameters.AddWithValue("fechaactualizacion", (object?)entity.FechaActualizacion ?? DBNull.Value);
            command.Parameters.AddWithValue("actualizadopor", (object?)entity.ActualizadoPor ?? DBNull.Value);

            await command.ExecuteNonQueryAsync();
            return Result.Success();
        }

        public async Task DeleteAsync(int id, string? eliminadoPor = null)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
UPDATE cliente SET isdeleted = TRUE, fechaactualizacion = @fechaactualizacion, eliminadopor = @eliminadoPor
WHERE clienteid = @id;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("fechaactualizacion", DateTime.UtcNow);
            command.Parameters.AddWithValue("eliminadoPor", (object?)eliminadoPor ?? DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }
    }
}
