using Npgsql;
using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Interfaces;
using OrdenTrabajoService.Infrastructure.Persistence;
using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Framework.Persistence;
using Taller_Mecanico_Users.Framework.Services;

namespace OrdenTrabajoService.Infrastructure.Repositories
{
    public class MarcaRepository : IRepository<Marca>
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly OrdenTrabajoQueryService _queryService;
        private readonly IAuthenticationHelper _authHelper;

        public MarcaRepository(ISqlConnectionFactory connectionFactory, OrdenTrabajoQueryService queryService, IAuthenticationHelper authHelper)
        {
            _connectionFactory = connectionFactory;
            _queryService = queryService;
            _authHelper = authHelper;
        }

        public async Task<IEnumerable<Marca>> GetAllAsync()
        {
            await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();
            return _queryService.LoadMarcas(connection);
        }

        public async Task<Result<Marca?>> GetByIdAsync(int id)
        {
            try
            {
                var todas = await GetAllAsync();
                return Result<Marca?>.Success(todas.FirstOrDefault(m => m.MarcaId == id));
            }
            catch (Exception ex)
            {
                return Result<Marca?>.Failure(ErrorCodes.DbError, $"Error al obtener marca: {ex.Message}");
            }
        }

        public async Task<Result<int>> AddAsync(Marca entity)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();
                const string sql = "INSERT INTO marca (nombre) VALUES (@nombre) RETURNING marcaid;";
                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("nombre", entity.Nombre);
                return Result<int>.Success(Convert.ToInt32(await cmd.ExecuteScalarAsync()));
            }
            catch (Exception ex)
            {
                return Result<int>.Failure(ErrorCodes.DbError, $"Error al registrar marca: {ex.Message}");
            }
        }

        public async Task<Result> UpdateAsync(Marca entity)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();
                const string sql = "UPDATE marca SET nombre = @nombre WHERE marcaid = @id;";
                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("nombre", entity.Nombre);
                cmd.Parameters.AddWithValue("id", entity.MarcaId);
                await cmd.ExecuteNonQueryAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorCodes.DbError, $"Error al actualizar marca: {ex.Message}");
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();
                const string sql = "DELETE FROM marca WHERE marcaid = @id;";
                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("id", id);
                await cmd.ExecuteNonQueryAsync();
            }
            catch { }
        }
    }
}
