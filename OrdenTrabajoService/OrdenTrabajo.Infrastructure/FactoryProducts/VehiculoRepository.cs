using System.Globalization;
using Npgsql;
using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Interfaces;
using OrdenTrabajoService.Infrastructure.Persistence;
using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Application.Persistence;
using Taller_Mecanico_Users.Application.Services;

namespace OrdenTrabajoService.Infrastructure.Repositories
{
    public class VehiculoRepository : IVehiculoRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly OrdenTrabajoQueryService _queryService;
        private readonly IAuthenticationHelper _authHelper;

        public VehiculoRepository(
            ISqlConnectionFactory connectionFactory,
            OrdenTrabajoQueryService queryService,
            IAuthenticationHelper authHelper)
        {
            _connectionFactory = connectionFactory;
            _queryService = queryService;
            _authHelper = authHelper;
        }

        public async Task<IEnumerable<Vehiculo>> GetAllAsync()
        {
            await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();
            return _queryService.LoadVehiculos(connection);
        }

        public async Task<IEnumerable<Vehiculo>> BuscarPorPlacaAsync(string term, int? clienteId = null)
        {
            var todos = await GetAllAsync();
            var normalized = term.ToLower(CultureInfo.InvariantCulture);
            var query = todos.Where(v =>
                !v.IsDeleted &&
                v.Placa.ToLower(CultureInfo.InvariantCulture).Contains(normalized, StringComparison.OrdinalIgnoreCase));

            if (clienteId.HasValue && clienteId.Value > 0)
                query = query.Where(v => v.ClienteId == clienteId.Value);

            return query.ToList();
        }

        public async Task<Result<Vehiculo?>> GetByIdAsync(int id)
        {
            try
            {
                var todos = await GetAllAsync();
                return Result<Vehiculo?>.Success(todos.FirstOrDefault(v => v.VehiculoId == id));
            }
            catch (Exception ex)
            {
                return Result<Vehiculo?>.Failure(ErrorCodes.DbError,
                    $"Error al obtener vehículo: {ex.Message}");
            }
        }

        public async Task<Result<int>> AddAsync(Vehiculo entity)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();

                var actor = _authHelper.GetCurrentAuditActor();
                const string sql = @"
INSERT INTO vehiculo (clienteid, placa, marcaid, modeloid, colorvehiculoid, anio, creadopor)
VALUES (@clienteid, @placa, @marcaid, @modeloid, @colorid, @anio, @actor)
RETURNING vehiculoid;";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("clienteid", entity.ClienteId);
                cmd.Parameters.AddWithValue("placa", entity.Placa);
                cmd.Parameters.AddWithValue("marcaid", entity.MarcaId);
                cmd.Parameters.AddWithValue("modeloid", entity.ModeloId);
                cmd.Parameters.AddWithValue("colorid", entity.ColorVehiculoId);
                cmd.Parameters.AddWithValue("anio", entity.Anio);
                cmd.Parameters.AddWithValue("actor", actor);

                var id = await cmd.ExecuteScalarAsync();
                return Result<int>.Success(Convert.ToInt32(id));
            }
            catch (Exception ex)
            {
                return Result<int>.Failure(ErrorCodes.DbError,
                    $"Error al registrar vehículo: {ex.Message}");
            }
        }

        public async Task<Result> UpdateAsync(Vehiculo entity)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();

                var actor = _authHelper.GetCurrentAuditActor();
                const string sql = @"
UPDATE vehiculo
SET placa = @placa, marcaid = @marcaid, modeloid = @modeloid,
    colorvehiculoid = @colorid, anio = @anio,
    actualizadopor = @actor, fechaactualizacion = @fecha
WHERE vehiculoid = @id AND NOT isdeleted;";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("placa", entity.Placa);
                cmd.Parameters.AddWithValue("marcaid", entity.MarcaId);
                cmd.Parameters.AddWithValue("modeloid", entity.ModeloId);
                cmd.Parameters.AddWithValue("colorid", entity.ColorVehiculoId);
                cmd.Parameters.AddWithValue("anio", entity.Anio);
                cmd.Parameters.AddWithValue("actor", actor);
                cmd.Parameters.AddWithValue("fecha", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("id", entity.VehiculoId);

                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    return Result.Failure(ErrorCodes.VehiculoNotFound,
                        $"Vehículo con ID {entity.VehiculoId} no encontrado.");
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorCodes.DbError,
                    $"Error al actualizar vehículo: {ex.Message}");
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();

                var actor = _authHelper.GetCurrentAuditActor();
                const string sql = "UPDATE vehiculo SET isdeleted = TRUE, eliminadopor = @actor WHERE vehiculoid = @id;";
                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("actor", actor);
                cmd.Parameters.AddWithValue("id", id);
                await cmd.ExecuteNonQueryAsync();
            }
            catch { }
        }
    }
}

