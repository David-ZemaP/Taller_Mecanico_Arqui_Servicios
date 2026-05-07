using Npgsql;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Infrastructure.Persistence;
using Taller_Mecanico_Arqui.Infrastructure.Services;

namespace Taller_Mecanico_Arqui.Infrastructure.Persistence.Repositories;

public class ProductoRepository : IRepository<Producto>
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly SqlEntityQueryService _queryService;
    private readonly AuthenticationHelper _authHelper;

    public ProductoRepository(ISqlConnectionFactory connectionFactory, SqlEntityQueryService queryService, AuthenticationHelper authHelper)
    {
        _connectionFactory = connectionFactory;
        _queryService = queryService;
        _authHelper = authHelper;
    }

    public async Task<IEnumerable<Producto>> GetAllAsync()
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        return _queryService.LoadProductos(connection);
    }

    public async Task<Result<Producto?>> GetByIdAsync(int id)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            var producto = _queryService.LoadProductos(connection).FirstOrDefault(p => p.ProductoId == id);
            return Result<Producto?>.Success(producto);
        }
        catch (Exception ex)
        {
            return Result<Producto?>.Failure(ErrorCodes.DbError, ex.Message);
        }
    }

    public async Task<Result<int>> AddAsync(Producto entity)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            const string sql = "INSERT INTO producto (nombre, precio, stock, activo) VALUES (@nombre, @precio, @stock, TRUE) RETURNING productoid;";
            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("nombre", entity.Nombre);
            cmd.Parameters.AddWithValue("precio", entity.Precio);
            cmd.Parameters.AddWithValue("stock", entity.Stock);
            var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return Result<int>.Success(id);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(ErrorCodes.DbError, ex.Message);
        }
    }

    public async Task<Result> UpdateAsync(Producto entity)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            const string sql = "UPDATE producto SET nombre=@nombre, precio=@precio, stock=@stock WHERE productoid=@id;";
            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("id", entity.ProductoId);
            cmd.Parameters.AddWithValue("nombre", entity.Nombre);
            cmd.Parameters.AddWithValue("precio", entity.Precio);
            cmd.Parameters.AddWithValue("stock", entity.Stock);
            await cmd.ExecuteNonQueryAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ErrorCodes.DbError, ex.Message);
        }
    }

    public async Task DeleteAsync(int id)
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        const string sql = "UPDATE producto SET activo=FALSE WHERE productoid=@id;";
        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }
}
