using Npgsql;
using OrdenTrabajoService.Infrastructure.Persistence;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Ports;

namespace OrdenTrabajoService.Infrastructure.Repositories
{
    public class ServicioRepository : IServicioRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public ServicioRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Servicio>> GetAllAsync()
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT servicioid, nombre, precio
FROM servicio
ORDER BY nombre;";

            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            var servicios = new List<Servicio>();
            while (await reader.ReadAsync())
            {
                servicios.Add(Servicio.Reconstituir(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetDouble(2)
                ));
            }

            return servicios;
        }

        public async Task<Result<Servicio?>> GetByIdAsync(int id)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT servicioid, nombre, precio
FROM servicio
WHERE servicioid = @id;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("id", id);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return Result<Servicio?>.Success(null);

            var servicio = Servicio.Reconstituir(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDouble(2)
            );

            return Result<Servicio?>.Success(servicio);
        }

        public async Task<Servicio?> GetByNombreAsync(string nombre)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT servicioid, nombre, precio
FROM servicio
WHERE nombre = @nombre;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("nombre", nombre);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return Servicio.Reconstituir(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDouble(2)
            );
        }

        public async Task<Result<int>> AddAsync(Servicio entity)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
INSERT INTO servicio (nombre, precio, creadopor)
VALUES (@nombre, @precio, @creadopor)
RETURNING servicioid;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("nombre", entity.Nombre);
            command.Parameters.AddWithValue("precio", Convert.ToDecimal(entity.Precio));
            command.Parameters.AddWithValue("creadopor", (object?)entity.CreadoPor ?? DBNull.Value);

            var result = await command.ExecuteScalarAsync();
            if (result == null || result == DBNull.Value)
                return Result<int>.Failure(ErrorCodes.DbInsertFailed, "No se pudo registrar el servicio.");

            return Result<int>.Success(Convert.ToInt32(result));
        }

        public async Task<Result> UpdateAsync(Servicio entity)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
UPDATE servicio
SET nombre = @nombre,
    precio = @precio,
    fechaactualizacion = @fechaactualizacion,
    actualizadopor = @actualizadopor
WHERE servicioid = @servicioid;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("servicioid", entity.ServicioId);
            command.Parameters.AddWithValue("nombre", entity.Nombre);
            command.Parameters.AddWithValue("precio", Convert.ToDecimal(entity.Precio));
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
UPDATE servicio SET isdeleted = TRUE, fechaactualizacion = @fechaactualizacion, eliminadopor = @eliminadoPor
WHERE servicioid = @id;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("fechaactualizacion", DateTime.UtcNow);
            command.Parameters.AddWithValue("eliminadoPor", (object?)eliminadoPor ?? DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }
    }
}
