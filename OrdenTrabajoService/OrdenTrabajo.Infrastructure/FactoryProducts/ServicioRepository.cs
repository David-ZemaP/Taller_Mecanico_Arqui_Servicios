using Npgsql;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Infrastructure.Persistence;
using Taller_Mecanico_Arqui.Infrastructure.Services;

namespace Taller_Mecanico_Arqui.Infrastructure.Persistence.Repositories;

public class ServicioRepository : IRepository<Servicio>
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly SqlEntityQueryService _queryService;
    private readonly AuthenticationHelper _authHelper;

    public ServicioRepository(ISqlConnectionFactory connectionFactory, SqlEntityQueryService queryService, AuthenticationHelper authHelper)
    {
        _connectionFactory = connectionFactory;
        _queryService = queryService;
        _authHelper = authHelper;
    }

    public async Task<IEnumerable<Servicio>> GetAllAsync()
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        return _queryService.LoadServicios(connection);
    }

    public async Task<Result<Servicio?>> GetByIdAsync(int id)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            var servicio = _queryService.LoadServicios(connection).FirstOrDefault(s => s.ServicioId == id);
            return Result<Servicio?>.Success(servicio);
        }
        catch (Exception ex)
        {
            return Result<Servicio?>.Failure(ErrorCodes.DbError, ex.Message);
        }
    }

    public async Task<Result<int>> AddAsync(Servicio entity)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            const string sql = "INSERT INTO servicio (nombre, precio) VALUES (@nombre, @precio) RETURNING servicioid;";
            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("nombre", entity.Nombre);
            cmd.Parameters.AddWithValue("precio", entity.Precio);
            var result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return Result<int>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(ErrorCodes.DbError, ex.Message);
        }
    }

    public async Task<Result> UpdateAsync(Servicio entity)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            const string sql = "UPDATE servicio SET nombre=@nombre, precio=@precio WHERE servicioid=@id;";
            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("id", entity.ServicioId);
            cmd.Parameters.AddWithValue("nombre", entity.Nombre);
            cmd.Parameters.AddWithValue("precio", entity.Precio);
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
        const string sql = "UPDATE servicio SET isdeleted=TRUE WHERE servicioid=@id;";
        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }
}
