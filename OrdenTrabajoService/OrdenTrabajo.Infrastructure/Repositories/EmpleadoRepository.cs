using Npgsql;
using OrdenTrabajoService.Infrastructure.Persistence;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Enums;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Domain.ValueObjects;

namespace OrdenTrabajoService.Infrastructure.Repositories
{
    public class EmpleadoRepository : IEmpleadoRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public EmpleadoRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Empleado>> GetAllAsync()
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT e.empleadoid, e.nombres, e.primerapellido, e.segundoapellido, e.cinumero, e.cicomplemento,
       e.telefono, e.email, e.fechacontratacion, e.estadolaboral, e.fechaactualizacion, e.isdeleted
FROM empleado e
WHERE e.isdeleted = FALSE
ORDER BY e.primerapellido, e.nombres;";

            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            var empleados = new List<Empleado>();
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

                empleados.Add(Empleado.Reconstituir(
                    reader.GetInt32(0),
                    nombreResult.Value,
                    ciResult.Value,
                    reader.GetInt32(6),
                    reader.IsDBNull(7) ? null : reader.GetString(7),
                    reader.GetDateTime(8),
                    Enum.Parse<EstadoLaboral>(reader.GetString(9), true),
                    reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                    reader.GetBoolean(11)
                ));
            }

            return empleados;
        }

        public async Task<Result<Empleado?>> GetByIdAsync(int id)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT e.empleadoid, e.nombres, e.primerapellido, e.segundoapellido, e.cinumero, e.cicomplemento,
       e.telefono, e.email, e.fechacontratacion, e.estadolaboral, e.fechaactualizacion, e.isdeleted
FROM empleado e
WHERE e.empleadoid = @id AND e.isdeleted = FALSE;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("id", id);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return Result<Empleado?>.Success(null);

            var nombreResult = NombreCompleto.Crear(
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetString(3));

            var ciResult = DocumentoIdentidad.Crear(
                reader.GetInt32(4),
                reader.IsDBNull(5) ? null : reader.GetString(5));

            if (nombreResult.IsFailure || ciResult.IsFailure)
                return Result<Empleado?>.Success(null);

            var empleado = Empleado.Reconstituir(
                reader.GetInt32(0),
                nombreResult.Value,
                ciResult.Value,
                reader.GetInt32(6),
                reader.IsDBNull(7) ? null : reader.GetString(7),
                reader.GetDateTime(8),
                Enum.Parse<EstadoLaboral>(reader.GetString(9), true),
                reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                reader.GetBoolean(11)
            );

            return Result<Empleado?>.Success(empleado);
        }

        public async Task<Empleado?> GetByCiAsync(int ci)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT e.empleadoid, e.nombres, e.primerapellido, e.segundoapellido, e.cinumero, e.cicomplemento,
       e.telefono, e.email, e.fechacontratacion, e.estadolaboral, e.fechaactualizacion, e.isdeleted
FROM empleado e
WHERE e.cinumero = @ci AND e.isdeleted = FALSE;";

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

            return Empleado.Reconstituir(
                reader.GetInt32(0),
                nombreResult.Value,
                ciResult.Value,
                reader.GetInt32(6),
                reader.IsDBNull(7) ? null : reader.GetString(7),
                reader.GetDateTime(8),
                Enum.Parse<EstadoLaboral>(reader.GetString(9), true),
                reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                reader.GetBoolean(11)
            );
        }

        public async Task<Result<int>> AddAsync(Empleado entity)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
INSERT INTO empleado (nombres, primerapellido, segundoapellido, cinumero, cicomplemento,
                      telefono, email, fechacontratacion, estadolaboral, isdeleted, creadopor)
VALUES (@nombres, @primerapellido, @segundoapellido, @cinumero, @cicomplemento,
        @telefono, @email, @fechacontratacion, @estadolaboral, FALSE, @creadopor)
RETURNING empleadoid;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("nombres", entity.NombreCompleto.Nombres);
            command.Parameters.AddWithValue("primerapellido", entity.NombreCompleto.PrimerApellido);
            command.Parameters.AddWithValue("segundoapellido", (object?)entity.NombreCompleto.SegundoApellido ?? DBNull.Value);
            command.Parameters.AddWithValue("cinumero", entity.Ci.Numero);
            command.Parameters.AddWithValue("cicomplemento", (object?)entity.Ci.Complemento ?? DBNull.Value);
            command.Parameters.AddWithValue("telefono", entity.Telefono);
            command.Parameters.AddWithValue("email", (object?)entity.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("fechacontratacion", entity.FechaContratacion);
            command.Parameters.AddWithValue("estadolaboral", entity.EstadoLaboral.ToString());
            command.Parameters.AddWithValue("creadopor", (object?)entity.CreadoPor ?? DBNull.Value);

            var result = await command.ExecuteScalarAsync();
            if (result == null || result == DBNull.Value)
                return Result<int>.Failure(ErrorCodes.DbInsertFailed, "No se pudo registrar el empleado.");

            return Result<int>.Success(Convert.ToInt32(result));
        }

        public async Task<Result> UpdateAsync(Empleado entity)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
UPDATE empleado
SET nombres = @nombres,
    primerapellido = @primerapellido,
    segundoapellido = @segundoapellido,
    cinumero = @cinumero,
    cicomplemento = @cicomplemento,
    telefono = @telefono,
    email = @email,
    estadolaboral = @estadolaboral,
    fechaactualizacion = @fechaactualizacion,
    actualizadopor = @actualizadopor
WHERE empleadoid = @empleadoid;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("empleadoid", entity.EmpleadoId);
            command.Parameters.AddWithValue("nombres", entity.NombreCompleto.Nombres);
            command.Parameters.AddWithValue("primerapellido", entity.NombreCompleto.PrimerApellido);
            command.Parameters.AddWithValue("segundoapellido", (object?)entity.NombreCompleto.SegundoApellido ?? DBNull.Value);
            command.Parameters.AddWithValue("cinumero", entity.Ci.Numero);
            command.Parameters.AddWithValue("cicomplemento", (object?)entity.Ci.Complemento ?? DBNull.Value);
            command.Parameters.AddWithValue("telefono", entity.Telefono);
            command.Parameters.AddWithValue("email", (object?)entity.Email ?? DBNull.Value);
            command.Parameters.AddWithValue("estadolaboral", entity.EstadoLaboral.ToString());
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
UPDATE empleado SET isdeleted = TRUE, fechaactualizacion = @fechaactualizacion, eliminadopor = @eliminadoPor
WHERE empleadoid = @id;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("fechaactualizacion", DateTime.UtcNow);
            command.Parameters.AddWithValue("eliminadoPor", (object?)eliminadoPor ?? DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }
    }
}
