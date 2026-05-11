using Npgsql;
using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Interfaces;
using OrdenTrabajoService.Infrastructure.Persistence;
using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Application.Persistence;
using Taller_Mecanico_Users.Application.Services;

namespace OrdenTrabajoService.Infrastructure.Repositories
{
    public class ModeloRepository : IRepository<Modelo>
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly OrdenTrabajoQueryService _queryService;
        private readonly IAuthenticationHelper _authHelper;

        public ModeloRepository(ISqlConnectionFactory connectionFactory, OrdenTrabajoQueryService queryService, IAuthenticationHelper authHelper)
        {
            _connectionFactory = connectionFactory;
            _queryService = queryService;
            _authHelper = authHelper;
        }

        public async Task<IEnumerable<Modelo>> GetAllAsync()
        {
            await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();
            return _queryService.LoadModelos(connection);
        }

        public async Task<Result<Modelo?>> GetByIdAsync(int id)
        {
            try
            {
                var todos = await GetAllAsync();
                return Result<Modelo?>.Success(todos.FirstOrDefault(m => m.ModeloId == id));
            }
            catch (Exception ex)
            {
                return Result<Modelo?>.Failure(ErrorCodes.DbError, $"Error al obtener modelo: {ex.Message}");
            }
        }

        public async Task<Result<int>> AddAsync(Modelo entity)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();
                const string sql = "INSERT INTO modelo (marcaid, nombre) VALUES (@marcaid, @nombre) RETURNING modeloid;";
                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("marcaid", entity.MarcaId);
                cmd.Parameters.AddWithValue("nombre", entity.Nombre);
                return Result<int>.Success(Convert.ToInt32(await cmd.ExecuteScalarAsync()));
            }
            catch (Exception ex)
            {
                return Result<int>.Failure(ErrorCodes.DbError, $"Error al registrar modelo: {ex.Message}");
            }
        }

        public async Task<Result> UpdateAsync(Modelo entity)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();
                const string sql = "UPDATE modelo SET nombre = @nombre WHERE modeloid = @id;";
                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("nombre", entity.Nombre);
                cmd.Parameters.AddWithValue("id", entity.ModeloId);
                await cmd.ExecuteNonQueryAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorCodes.DbError, $"Error al actualizar modelo: {ex.Message}");
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();
                const string sql = "DELETE FROM modelo WHERE modeloid = @id;";
                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("id", id);
                await cmd.ExecuteNonQueryAsync();
            }
            catch { }
        }
    }
}

