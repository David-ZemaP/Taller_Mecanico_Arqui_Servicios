using Npgsql;
using TallerMecanico.Core.Contracts;
using TallerMecanico.Core.Models;

namespace TallerMecanico.Services.Infrastructure.Repositories;

internal sealed class PostgreSqlClienteRepository : IClienteRepository
{
    private readonly PostgreSqlDatabase _database;

    public PostgreSqlClienteRepository(PostgreSqlDatabase database)
    {
        _database = database;
        _database.EnsureSchema();
    }

    public Cliente Add(Cliente cliente)
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO clientes (nombre, apellido, telefono, email, is_deleted, created_at, created_by_user_id)
VALUES (@nombre, @apellido, @telefono, @email, FALSE, NOW(), @created_by_user_id)
RETURNING id, created_at;";
        command.Parameters.AddWithValue("nombre", cliente.Nombre);
        command.Parameters.AddWithValue("apellido", cliente.Apellido);
        command.Parameters.AddWithValue("telefono", (object?)cliente.Telefono ?? DBNull.Value);
        command.Parameters.AddWithValue("email", (object?)cliente.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("created_by_user_id", (object?)cliente.CreatedByUserId ?? DBNull.Value);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            cliente.Id = reader.GetInt32(reader.GetOrdinal("id"));
            cliente.CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
        }

        return cliente;
    }

    public Cliente? GetById(int id) => QuerySingle("SELECT * FROM clientes WHERE id = @id AND is_deleted = FALSE", command => command.Parameters.AddWithValue("id", id));

    public IEnumerable<Cliente> GetAll() => QueryMany("SELECT * FROM clientes WHERE is_deleted = FALSE ORDER BY id");

    public Cliente? Update(int id, Cliente cliente)
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE clientes
SET nombre = @nombre,
    apellido = @apellido,
    telefono = @telefono,
    email = @email
WHERE id = @id AND is_deleted = FALSE;
SELECT * FROM clientes WHERE id = @id;";
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("nombre", cliente.Nombre);
        command.Parameters.AddWithValue("apellido", cliente.Apellido);
        command.Parameters.AddWithValue("telefono", (object?)cliente.Telefono ?? DBNull.Value);
        command.Parameters.AddWithValue("email", (object?)cliente.Email ?? DBNull.Value);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            return PostgreSqlMappings.MapCliente(reader);
        }

        return null;
    }

    public bool Delete(int id, int? usuarioId = null)
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE clientes
SET is_deleted = TRUE,
    deleted_at = NOW(),
    deleted_by_user_id = @deleted_by_user_id
WHERE id = @id AND is_deleted = FALSE;";
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("deleted_by_user_id", (object?)usuarioId ?? DBNull.Value);

        return command.ExecuteNonQuery() > 0;
    }

    private Cliente? QuerySingle(string sql, Action<NpgsqlCommand>? configure = null)
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        configure?.Invoke(command);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return PostgreSqlMappings.MapCliente(reader);
        }

        return null;
    }

    private IEnumerable<Cliente> QueryMany(string sql)
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        using var reader = command.ExecuteReader();
        var result = new List<Cliente>();
        while (reader.Read())
        {
            result.Add(PostgreSqlMappings.MapCliente(reader));
        }

        return result;
    }
}