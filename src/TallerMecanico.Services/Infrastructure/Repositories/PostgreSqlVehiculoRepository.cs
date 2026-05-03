using Npgsql;
using TallerMecanico.Core.Contracts;
using TallerMecanico.Core.Models;

namespace TallerMecanico.Services.Infrastructure.Repositories;

internal sealed class PostgreSqlVehiculoRepository : IVehiculoRepository
{
    private readonly PostgreSqlDatabase _database;

    public PostgreSqlVehiculoRepository(PostgreSqlDatabase database)
    {
        _database = database;
        _database.EnsureSchema();
    }

    public Vehiculo Add(Vehiculo vehiculo)
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO vehiculos (cliente_id, marca, modelo, anio, placa, color, is_deleted, created_at, created_by_user_id)
VALUES (@cliente_id, @marca, @modelo, @anio, @placa, @color, FALSE, NOW(), @created_by_user_id)
RETURNING id, created_at;";
        command.Parameters.AddWithValue("cliente_id", vehiculo.ClienteId);
        command.Parameters.AddWithValue("marca", vehiculo.Marca);
        command.Parameters.AddWithValue("modelo", vehiculo.Modelo);
        command.Parameters.AddWithValue("anio", vehiculo.Anio);
        command.Parameters.AddWithValue("placa", vehiculo.Placa);
        command.Parameters.AddWithValue("color", (object?)vehiculo.Color ?? DBNull.Value);
        command.Parameters.AddWithValue("created_by_user_id", (object?)vehiculo.CreatedByUserId ?? DBNull.Value);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            vehiculo.Id = reader.GetInt32(reader.GetOrdinal("id"));
            vehiculo.CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
        }

        return vehiculo;
    }

    public Vehiculo? GetById(int id) => QuerySingle("SELECT * FROM vehiculos WHERE id = @id AND is_deleted = FALSE", command => command.Parameters.AddWithValue("id", id));

    public IEnumerable<Vehiculo> GetAll() => QueryMany("SELECT * FROM vehiculos WHERE is_deleted = FALSE ORDER BY id");

    public IEnumerable<Vehiculo> GetByCliente(int clienteId) => QueryMany("SELECT * FROM vehiculos WHERE cliente_id = @cliente_id AND is_deleted = FALSE ORDER BY id", command => command.Parameters.AddWithValue("cliente_id", clienteId));

    public Vehiculo? Update(int id, Vehiculo vehiculo)
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE vehiculos
SET cliente_id = @cliente_id,
    marca = @marca,
    modelo = @modelo,
    anio = @anio,
    placa = @placa,
    color = @color
WHERE id = @id AND is_deleted = FALSE;
SELECT * FROM vehiculos WHERE id = @id;";
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("cliente_id", vehiculo.ClienteId);
        command.Parameters.AddWithValue("marca", vehiculo.Marca);
        command.Parameters.AddWithValue("modelo", vehiculo.Modelo);
        command.Parameters.AddWithValue("anio", vehiculo.Anio);
        command.Parameters.AddWithValue("placa", vehiculo.Placa);
        command.Parameters.AddWithValue("color", (object?)vehiculo.Color ?? DBNull.Value);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            return PostgreSqlMappings.MapVehiculo(reader);
        }

        return null;
    }

    public bool Delete(int id, int? usuarioId = null)
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE vehiculos
SET is_deleted = TRUE,
    deleted_at = NOW(),
    deleted_by_user_id = @deleted_by_user_id
WHERE id = @id AND is_deleted = FALSE;";
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("deleted_by_user_id", (object?)usuarioId ?? DBNull.Value);

        return command.ExecuteNonQuery() > 0;
    }

    private Vehiculo? QuerySingle(string sql, Action<NpgsqlCommand>? configure = null)
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        configure?.Invoke(command);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return PostgreSqlMappings.MapVehiculo(reader);
        }

        return null;
    }

    private IEnumerable<Vehiculo> QueryMany(string sql, Action<NpgsqlCommand>? configure = null)
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        configure?.Invoke(command);

        using var reader = command.ExecuteReader();
        var result = new List<Vehiculo>();
        while (reader.Read())
        {
            result.Add(PostgreSqlMappings.MapVehiculo(reader));
        }

        return result;
    }
}