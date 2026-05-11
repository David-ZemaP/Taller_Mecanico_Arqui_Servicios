using Npgsql;
using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Interfaces;
using OrdenTrabajoService.Infrastructure.Persistence;
using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Framework.Persistence;
using Taller_Mecanico_Users.Framework.Services;

namespace OrdenTrabajoService.Infrastructure.Repositories
{
    public class ProductoRepository : IRepository<Producto>
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly OrdenTrabajoQueryService _queryService;
        private readonly IAuthenticationHelper _authHelper;

        public ProductoRepository(
            ISqlConnectionFactory connectionFactory,
            OrdenTrabajoQueryService queryService,
            IAuthenticationHelper authHelper)
        {
            _connectionFactory = connectionFactory;
            _queryService = queryService;
            _authHelper = authHelper;
        }

        public async Task<IEnumerable<Producto>> GetAllAsync()
        {
            await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();
            return _queryService.LoadProductos(connection);
        }

        public async Task<Result<Producto?>> GetByIdAsync(int id)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();

                const string sql = @"
SELECT productoid, nombre, precio, stock, isdeleted
FROM producto WHERE productoid = @id AND NOT isdeleted;";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("id", id);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    return Result<Producto?>.Success(null);

                return Result<Producto?>.Success(Producto.Reconstituir(
                    reader.GetInt32(reader.GetOrdinal("productoid")),
                    reader.GetString(reader.GetOrdinal("nombre")),
                    Convert.ToDouble(reader.GetDecimal(reader.GetOrdinal("precio"))),
                    reader.GetInt32(reader.GetOrdinal("stock"))));
            }
            catch (Exception ex)
            {
                return Result<Producto?>.Failure(ErrorCodes.DbError,
                    $"Error al obtener producto: {ex.Message}");
            }
        }

        public async Task<Result<int>> AddAsync(Producto entity)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();

                var actor = _authHelper.GetCurrentAuditActor();
                const string sql = @"
INSERT INTO producto (nombre, precio, stock, creadopor)
VALUES (@nombre, @precio, @stock, @actor)
RETURNING productoid;";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("nombre", entity.Nombre);
                cmd.Parameters.AddWithValue("precio", Convert.ToDecimal(entity.Precio));
                cmd.Parameters.AddWithValue("stock", entity.Stock);
                cmd.Parameters.AddWithValue("actor", actor);

                var id = await cmd.ExecuteScalarAsync();
                return Result<int>.Success(Convert.ToInt32(id));
            }
            catch (Exception ex)
            {
                return Result<int>.Failure(ErrorCodes.DbError,
                    $"Error al registrar producto: {ex.Message}");
            }
        }

        public async Task<Result> UpdateAsync(Producto entity)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();

                var actor = _authHelper.GetCurrentAuditActor();
                const string sql = @"
UPDATE producto
SET nombre = @nombre, precio = @precio, stock = @stock,
    actualizadopor = @actor, fechaactualizacion = @fecha
WHERE productoid = @id AND NOT isdeleted;";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("nombre", entity.Nombre);
                cmd.Parameters.AddWithValue("precio", Convert.ToDecimal(entity.Precio));
                cmd.Parameters.AddWithValue("stock", entity.Stock);
                cmd.Parameters.AddWithValue("actor", actor);
                cmd.Parameters.AddWithValue("fecha", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("id", entity.ProductoId);

                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    return Result.Failure(ErrorCodes.ProductoNotFound,
                        $"Producto con ID {entity.ProductoId} no encontrado.");
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorCodes.DbError,
                    $"Error al actualizar producto: {ex.Message}");
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();

                var actor = _authHelper.GetCurrentAuditActor();
                const string sql = @"
UPDATE producto
SET isdeleted = TRUE, eliminadopor = @actor, fechaactualizacion = @fecha
WHERE productoid = @id;";

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
