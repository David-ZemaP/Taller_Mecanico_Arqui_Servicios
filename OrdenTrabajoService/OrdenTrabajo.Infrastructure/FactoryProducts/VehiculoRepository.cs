using Npgsql;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Infrastructure.Persistence;
using Taller_Mecanico_Arqui.Infrastructure.Services;

namespace Taller_Mecanico_Arqui.Infrastructure.Persistence.Repositories;

public class VehiculoRepository : IVehiculoRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly SqlEntityQueryService _queryService;
    private readonly AuthenticationHelper _authHelper;

    public VehiculoRepository(ISqlConnectionFactory connectionFactory, SqlEntityQueryService queryService, AuthenticationHelper authHelper)
    {
        _connectionFactory = connectionFactory;
        _queryService = queryService;
        _authHelper = authHelper;
    }

    public async Task<IEnumerable<Vehiculo>> GetAllAsync()
    {
        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync();
        return _queryService.LoadVehiculos(connection);
    }

    public async Task<Result<Vehiculo?>> GetByIdAsync(int id)
    {
        try
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            var vehiculo = _queryService.LoadVehiculos(connection).FirstOrDefault(v => v.VehiculoId == id);
            return Result<Vehiculo?>.Success(vehiculo);
        }
        catch (Exception ex)
        {
            return Result<Vehiculo?>.Failure(ErrorCodes.DbError, ex.Message);
        }
    }

    public Task<Result<int>> AddAsync(Vehiculo entity) =>
        Task.FromResult(Result<int>.Failure(ErrorCodes.ValidationInvalidValue, "Operación no soportada en este servicio."));

    public Task<Result> UpdateAsync(Vehiculo entity) =>
        Task.FromResult(Result.Failure(ErrorCodes.ValidationInvalidValue, "Operación no soportada en este servicio."));

    public Task DeleteAsync(int id) => Task.CompletedTask;
}
