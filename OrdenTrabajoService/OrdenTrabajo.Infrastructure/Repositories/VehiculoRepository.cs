using Npgsql;
using OrdenTrabajoService.Infrastructure.Persistence;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Domain.ValueObjects;

namespace OrdenTrabajoService.Infrastructure.Repositories
{
    public class VehiculoRepository : IVehiculoRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public VehiculoRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Vehiculo>> GetAllAsync()
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT v.vehiculoid, v.clienteid, v.placa, v.marcaid, v.modeloid, v.colorvehiculoid, v.anio,
       v.fechaactualizacion, v.isdeleted,
       c.nombre, c.primerapellido, c.segundoapellido, c.ci, c.cicomplemento, c.telefono, c.email,
       c.fecharegistro, c.tipocliente, c.usuariologinid, c.fechaactualizacion, c.isdeleted,
       m.nombre AS marcanombre,
       mo.nombre AS modelonombre,
       cv.nombre AS colornombre
FROM vehiculo v
LEFT JOIN cliente c ON c.clienteid = v.clienteid
LEFT JOIN marca m ON m.marcaid = v.marcaid
LEFT JOIN modelo mo ON mo.modeloid = v.modeloid
LEFT JOIN colorvehiculo cv ON cv.colorvehiculoid = v.colorvehiculoid
WHERE v.isdeleted = FALSE
ORDER BY v.placa;";

            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            var vehiculos = new List<Vehiculo>();
            while (await reader.ReadAsync())
            {
                vehiculos.Add(MapVehiculo(reader));
            }

            return vehiculos;
        }

        public async Task<Result<Vehiculo?>> GetByIdAsync(int id)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT v.vehiculoid, v.clienteid, v.placa, v.marcaid, v.modeloid, v.colorvehiculoid, v.anio,
       v.fechaactualizacion, v.isdeleted,
       c.nombre, c.primerapellido, c.segundoapellido, c.ci, c.cicomplemento, c.telefono, c.email,
       c.fecharegistro, c.tipocliente, c.usuariologinid, c.fechaactualizacion, c.isdeleted,
       m.nombre AS marcanombre,
       mo.nombre AS modelonombre,
       cv.nombre AS colornombre
FROM vehiculo v
LEFT JOIN cliente c ON c.clienteid = v.clienteid
LEFT JOIN marca m ON m.marcaid = v.marcaid
LEFT JOIN modelo mo ON mo.modeloid = v.modeloid
LEFT JOIN colorvehiculo cv ON cv.colorvehiculoid = v.colorvehiculoid
WHERE v.vehiculoid = @id AND v.isdeleted = FALSE;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("id", id);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return Result<Vehiculo?>.Success(null);

            var vehiculo = MapVehiculo(reader);

            return Result<Vehiculo?>.Success(vehiculo);
        }

        public async Task<Vehiculo?> GetByPlacaAsync(string placa)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT v.vehiculoid, v.clienteid, v.placa, v.marcaid, v.modeloid, v.colorvehiculoid, v.anio,
       v.fechaactualizacion, v.isdeleted,
       c.nombre, c.primerapellido, c.segundoapellido, c.ci, c.cicomplemento, c.telefono, c.email,
       c.fecharegistro, c.tipocliente, c.usuariologinid, c.fechaactualizacion, c.isdeleted,
       m.nombre AS marcanombre,
       mo.nombre AS modelonombre,
       cv.nombre AS colornombre
FROM vehiculo v
LEFT JOIN cliente c ON c.clienteid = v.clienteid
LEFT JOIN marca m ON m.marcaid = v.marcaid
LEFT JOIN modelo mo ON mo.modeloid = v.modeloid
LEFT JOIN colorvehiculo cv ON cv.colorvehiculoid = v.colorvehiculoid
WHERE v.placa = @placa AND v.isdeleted = FALSE;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("placa", placa);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return MapVehiculo(reader);
        }

        public async Task<IEnumerable<Vehiculo>> GetByClienteIdAsync(int clienteId)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT v.vehiculoid, v.clienteid, v.placa, v.marcaid, v.modeloid, v.colorvehiculoid, v.anio,
       v.fechaactualizacion, v.isdeleted,
       c.nombre, c.primerapellido, c.segundoapellido, c.ci, c.cicomplemento, c.telefono, c.email,
       c.fecharegistro, c.tipocliente, c.usuariologinid, c.fechaactualizacion, c.isdeleted,
       m.nombre AS marcanombre,
       mo.nombre AS modelonombre,
       cv.nombre AS colornombre
FROM vehiculo v
LEFT JOIN cliente c ON c.clienteid = v.clienteid
LEFT JOIN marca m ON m.marcaid = v.marcaid
LEFT JOIN modelo mo ON mo.modeloid = v.modeloid
LEFT JOIN colorvehiculo cv ON cv.colorvehiculoid = v.colorvehiculoid
WHERE v.clienteid = @clienteid AND v.isdeleted = FALSE
ORDER BY v.placa;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("clienteid", clienteId);

            var vehiculos = new List<Vehiculo>();
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                vehiculos.Add(MapVehiculo(reader));
            }

            return vehiculos;
        }

        public async Task<Result<int>> AddAsync(Vehiculo entity)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
INSERT INTO vehiculo (clienteid, placa, marcaid, modeloid, colorvehiculoid, anio, isdeleted, creadopor)
VALUES (@clienteid, @placa, @marcaid, @modeloid, @colorvehiculoid, @anio, FALSE, @creadopor)
RETURNING vehiculoid;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("clienteid", entity.ClienteId);
            command.Parameters.AddWithValue("placa", entity.Placa);
            command.Parameters.AddWithValue("marcaid", entity.MarcaId);
            command.Parameters.AddWithValue("modeloid", entity.ModeloId);
            command.Parameters.AddWithValue("colorvehiculoid", entity.ColorVehiculoId);
            command.Parameters.AddWithValue("anio", entity.Anio);
            command.Parameters.AddWithValue("creadopor", (object?)entity.CreadoPor ?? DBNull.Value);

            var result = await command.ExecuteScalarAsync();
            if (result == null || result == DBNull.Value)
                return Result<int>.Failure(ErrorCodes.DbInsertFailed, "No se pudo registrar el vehículo.");

            return Result<int>.Success(Convert.ToInt32(result));
        }

        public async Task<Result> UpdateAsync(Vehiculo entity)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
UPDATE vehiculo
SET clienteid = @clienteid,
    placa = @placa,
    marcaid = @marcaid,
    modeloid = @modeloid,
    colorvehiculoid = @colorvehiculoid,
    anio = @anio,
    fechaactualizacion = @fechaactualizacion,
    actualizadopor = @actualizadopor
WHERE vehiculoid = @vehiculoid;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("vehiculoid", entity.VehiculoId);
            command.Parameters.AddWithValue("clienteid", entity.ClienteId);
            command.Parameters.AddWithValue("placa", entity.Placa);
            command.Parameters.AddWithValue("marcaid", entity.MarcaId);
            command.Parameters.AddWithValue("modeloid", entity.ModeloId);
            command.Parameters.AddWithValue("colorvehiculoid", entity.ColorVehiculoId);
            command.Parameters.AddWithValue("anio", entity.Anio);
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
UPDATE vehiculo SET isdeleted = TRUE, fechaactualizacion = @fechaactualizacion, eliminadopor = @eliminadoPor
WHERE vehiculoid = @id;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("fechaactualizacion", DateTime.UtcNow);
            command.Parameters.AddWithValue("eliminadoPor", (object?)eliminadoPor ?? DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }

        private static Vehiculo MapVehiculo(NpgsqlDataReader reader)
        {
            var vehiculo = Vehiculo.Reconstituir(
                reader.GetInt32(0),
                reader.GetInt32(1),
                reader.GetString(2),
                reader.GetInt32(3),
                reader.GetInt32(4),
                reader.GetInt32(5),
                reader.GetInt32(6),
                reader.IsDBNull(7) ? null : reader.GetDateTime(7),
                reader.GetBoolean(8));

            vehiculo.SetNavigationProperties(
                BuildCliente(reader, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20),
                BuildMarca(reader, 21, 3),
                BuildModelo(reader, 22, 4, 3),
                BuildColorVehiculo(reader, 23, 5));

            return vehiculo;
        }

        private static Cliente? BuildCliente(
            NpgsqlDataReader reader,
            int nombresOrdinal,
            int primerApellidoOrdinal,
            int segundoApellidoOrdinal,
            int ciOrdinal,
            int ciComplementoOrdinal,
            int telefonoOrdinal,
            int emailOrdinal,
            int fechaRegistroOrdinal,
            int tipoClienteOrdinal,
            int usuarioLoginIdOrdinal,
            int fechaActualizacionOrdinal,
            int isDeletedOrdinal)
        {
            if (reader.IsDBNull(nombresOrdinal) || reader.IsDBNull(primerApellidoOrdinal) || reader.IsDBNull(ciOrdinal) || reader.IsDBNull(telefonoOrdinal) || reader.IsDBNull(emailOrdinal) || reader.IsDBNull(fechaRegistroOrdinal) || reader.IsDBNull(tipoClienteOrdinal))
            {
                return null;
            }

            var nombreCompletoResult = NombreCompleto.Crear(
                reader.GetString(nombresOrdinal),
                reader.GetString(primerApellidoOrdinal),
                reader.IsDBNull(segundoApellidoOrdinal) ? null : reader.GetString(segundoApellidoOrdinal));
            if (nombreCompletoResult.IsFailure)
            {
                return null;
            }

            var documentoResult = DocumentoIdentidad.Crear(
                reader.GetInt32(ciOrdinal),
                reader.IsDBNull(ciComplementoOrdinal) ? null : reader.GetString(ciComplementoOrdinal));
            if (documentoResult.IsFailure)
            {
                return null;
            }

            if (!Enum.TryParse<Taller_Mecanico_Arqui.Domain.Enums.TipoCliente>(reader.GetString(tipoClienteOrdinal), true, out var tipoCliente))
            {
                tipoCliente = Taller_Mecanico_Arqui.Domain.Enums.TipoCliente.Regular;
            }

            return Cliente.Reconstituir(
                reader.GetInt32(1),
                nombreCompletoResult.Value,
                documentoResult.Value,
                reader.GetInt32(telefonoOrdinal),
                reader.GetString(emailOrdinal),
                reader.GetDateTime(fechaRegistroOrdinal),
                tipoCliente,
                reader.IsDBNull(usuarioLoginIdOrdinal) ? null : reader.GetInt32(usuarioLoginIdOrdinal),
                reader.IsDBNull(fechaActualizacionOrdinal) ? null : reader.GetDateTime(fechaActualizacionOrdinal),
                reader.GetBoolean(isDeletedOrdinal));
        }

        private static Marca? BuildMarca(NpgsqlDataReader reader, int nombreOrdinal, int marcaIdOrdinal)
        {
            return reader.IsDBNull(nombreOrdinal)
                ? null
                : Marca.Reconstituir(reader.GetInt32(marcaIdOrdinal), reader.GetString(nombreOrdinal));
        }

        private static Modelo? BuildModelo(NpgsqlDataReader reader, int nombreOrdinal, int modeloIdOrdinal, int marcaIdOrdinal)
        {
            return reader.IsDBNull(nombreOrdinal)
                ? null
                : Modelo.Reconstituir(reader.GetInt32(modeloIdOrdinal), reader.GetInt32(marcaIdOrdinal), reader.GetString(nombreOrdinal));
        }

        private static ColorVehiculo? BuildColorVehiculo(NpgsqlDataReader reader, int nombreOrdinal, int colorIdOrdinal)
        {
            return reader.IsDBNull(nombreOrdinal)
                ? null
                : ColorVehiculo.Reconstituir(reader.GetInt32(colorIdOrdinal), reader.GetString(nombreOrdinal));
        }
    }
}
