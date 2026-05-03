using Npgsql;
using TallerMecanico.Core.Contracts;
using TallerMecanico.Core.Models;

namespace TallerMecanico.Services.Infrastructure.Repositories;

internal sealed class PostgreSqlProductoRepository : IProductoRepository
{
    private readonly PostgreSqlDatabase _database;

    public PostgreSqlProductoRepository(PostgreSqlDatabase database)
    {
        _database = database;
        _database.EnsureSchema();
    }

    public Producto Add(Producto producto)
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
INSERT INTO productos (nombre, descripcion, precio, stock, is_deleted)
VALUES (@nombre, @descripcion, @precio, @stock, FALSE)
RETURNING id;";
        command.Parameters.AddWithValue("nombre", producto.Nombre);
        command.Parameters.AddWithValue("descripcion", (object?)producto.Descripcion ?? DBNull.Value);
        command.Parameters.AddWithValue("precio", producto.Precio);
        command.Parameters.AddWithValue("stock", producto.Stock);

        producto.Id = (int)command.ExecuteScalar()!;
        return producto;
    }

    public Producto? GetById(int id) => QuerySingle("SELECT * FROM productos WHERE id = @id AND is_deleted = FALSE", command => command.Parameters.AddWithValue("id", id));

    public IEnumerable<Producto> GetAll() => QueryMany("SELECT * FROM productos WHERE is_deleted = FALSE ORDER BY id");

    public Producto? Update(int id, Producto producto)
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE productos
SET nombre = @nombre,
    descripcion = @descripcion,
    precio = @precio,
    stock = @stock
WHERE id = @id AND is_deleted = FALSE;
SELECT * FROM productos WHERE id = @id;";
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("nombre", producto.Nombre);
        command.Parameters.AddWithValue("descripcion", (object?)producto.Descripcion ?? DBNull.Value);
        command.Parameters.AddWithValue("precio", producto.Precio);
        command.Parameters.AddWithValue("stock", producto.Stock);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            return PostgreSqlMappings.MapProducto(reader);
        }

        return null;
    }

    public bool UpdateStock(int productoId, int delta)
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
UPDATE productos
SET stock = stock + @delta
WHERE id = @id AND is_deleted = FALSE AND stock + @delta >= 0;";
        command.Parameters.AddWithValue("id", productoId);
        command.Parameters.AddWithValue("delta", delta);

        return command.ExecuteNonQuery() > 0;
    }

    private Producto? QuerySingle(string sql, Action<NpgsqlCommand>? configure = null)
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        configure?.Invoke(command);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return PostgreSqlMappings.MapProducto(reader);
        }

        return null;
    }

    private IEnumerable<Producto> QueryMany(string sql)
    {
        using var connection = _database.OpenConnection();
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        using var reader = command.ExecuteReader();
        var result = new List<Producto>();
        while (reader.Read())
        {
            result.Add(PostgreSqlMappings.MapProducto(reader));
        }

        return result;
    }
}