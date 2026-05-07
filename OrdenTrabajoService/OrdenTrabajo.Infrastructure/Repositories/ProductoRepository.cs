using Npgsql;
using OrdenTrabajoService.Infrastructure.Persistence;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Ports;

namespace OrdenTrabajoService.Infrastructure.Repositories
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public ProductoRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Producto>> GetAllAsync()
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT productoid, nombre, precio, stock
FROM producto
ORDER BY nombre;";

            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            var productos = new List<Producto>();
            while (await reader.ReadAsync())
            {
                productos.Add(Producto.Reconstituir(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetDouble(2),
                    reader.GetInt32(3)
                ));
            }

            return productos;
        }

        public async Task<Result<Producto?>> GetByIdAsync(int id)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT productoid, nombre, precio, stock
FROM producto
WHERE productoid = @id;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("id", id);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return Result<Producto?>.Success(null);

            var producto = Producto.Reconstituir(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDouble(2),
                reader.GetInt32(3)
            );

            return Result<Producto?>.Success(producto);
        }

        public async Task<Producto?> GetByNombreAsync(string nombre)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
SELECT productoid, nombre, precio, stock
FROM producto
WHERE nombre = @nombre;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("nombre", nombre);

            await using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return Producto.Reconstituir(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetDouble(2),
                reader.GetInt32(3)
            );
        }

        public async Task<Result<int>> AddAsync(Producto entity)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
INSERT INTO producto (nombre, precio, stock, creadopor)
VALUES (@nombre, @precio, @stock, @creadopor)
RETURNING productoid;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("nombre", entity.Nombre);
            command.Parameters.AddWithValue("precio", Convert.ToDecimal(entity.Precio));
            command.Parameters.AddWithValue("stock", entity.Stock);
            command.Parameters.AddWithValue("creadopor", (object?)entity.CreadoPor ?? DBNull.Value);

            var result = await command.ExecuteScalarAsync();
            if (result == null || result == DBNull.Value)
                return Result<int>.Failure(ErrorCodes.DbInsertFailed, "No se pudo registrar el producto.");

            return Result<int>.Success(Convert.ToInt32(result));
        }

        public async Task<Result> UpdateAsync(Producto entity)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = @"
UPDATE producto
SET nombre = @nombre,
    precio = @precio,
    stock = @stock,
    fechaactualizacion = @fechaactualizacion,
    actualizadopor = @actualizadopor
WHERE productoid = @productoid;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("productoid", entity.ProductoId);
            command.Parameters.AddWithValue("nombre", entity.Nombre);
            command.Parameters.AddWithValue("precio", Convert.ToDecimal(entity.Precio));
            command.Parameters.AddWithValue("stock", entity.Stock);
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
UPDATE producto SET isdeleted = TRUE, fechaactualizacion = @fechaactualizacion, eliminadopor = @eliminadoPor
WHERE productoid = @id;";

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("id", id);
            command.Parameters.AddWithValue("fechaactualizacion", DateTime.UtcNow);
            command.Parameters.AddWithValue("eliminadoPor", (object?)eliminadoPor ?? DBNull.Value);

            await command.ExecuteNonQueryAsync();
        }
    }
}
