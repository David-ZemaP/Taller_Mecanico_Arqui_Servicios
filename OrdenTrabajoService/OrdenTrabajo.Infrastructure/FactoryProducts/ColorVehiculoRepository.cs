using Npgsql;
using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Interfaces;
using OrdenTrabajoService.Infrastructure.Persistence;
using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Framework.Persistence;
using Taller_Mecanico_Users.Framework.Services;

namespace OrdenTrabajoService.Infrastructure.Repositories
{
    public class ColorVehiculoRepository : IRepository<ColorVehiculo>
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly OrdenTrabajoQueryService _queryService;
        private readonly IAuthenticationHelper _authHelper;

        public ColorVehiculoRepository(ISqlConnectionFactory connectionFactory, OrdenTrabajoQueryService queryService, IAuthenticationHelper authHelper)
        {
            _connectionFactory = connectionFactory;
            _queryService = queryService;
            _authHelper = authHelper;
        }

        public async Task<IEnumerable<ColorVehiculo>> GetAllAsync()
        {
            await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();
            return _queryService.LoadColores(connection);
        }

        public async Task<Result<ColorVehiculo?>> GetByIdAsync(int id)
        {
            try
            {
                var todos = await GetAllAsync();
                return Result<ColorVehiculo?>.Success(todos.FirstOrDefault(c => c.ColorVehiculoId == id));
            }
            catch (Exception ex)
            {
                return Result<ColorVehiculo?>.Failure(ErrorCodes.DbError, $"Error al obtener color: {ex.Message}");
            }
        }

        public async Task<Result<int>> AddAsync(ColorVehiculo entity)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();
                const string sql = "INSERT INTO colorvehiculo (nombre) VALUES (@nombre) RETURNING colorvehiculoid;";
                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("nombre", entity.Nombre);
                return Result<int>.Success(Convert.ToInt32(await cmd.ExecuteScalarAsync()));
            }
            catch (Exception ex)
            {
                return Result<int>.Failure(ErrorCodes.DbError, $"Error al registrar color: {ex.Message}");
            }
        }

        public async Task<Result> UpdateAsync(ColorVehiculo entity)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();
                const string sql = "UPDATE colorvehiculo SET nombre = @nombre WHERE colorvehiculoid = @id;";
                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("nombre", entity.Nombre);
                cmd.Parameters.AddWithValue("id", entity.ColorVehiculoId);
                await cmd.ExecuteNonQueryAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorCodes.DbError, $"Error al actualizar color: {ex.Message}");
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();
                const string sql = "DELETE FROM colorvehiculo WHERE colorvehiculoid = @id;";
                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("id", id);
                await cmd.ExecuteNonQueryAsync();
            }
            catch { }
        }
    }
}
