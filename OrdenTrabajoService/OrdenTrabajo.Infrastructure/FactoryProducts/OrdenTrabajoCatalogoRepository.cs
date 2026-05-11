using Npgsql;
using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Interfaces;
using OrdenTrabajoService.Infrastructure.Persistence;
using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Framework.Persistence;
using Taller_Mecanico_Users.Framework.Services;

namespace OrdenTrabajoService.Infrastructure.Repositories
{
    public class OrdenTrabajoCatalogoRepository : IOrdenTrabajoCatalogoRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly OrdenTrabajoQueryService _queryService;
        private readonly IAuthenticationHelper _authHelper;

        public OrdenTrabajoCatalogoRepository(
            ISqlConnectionFactory connectionFactory,
            OrdenTrabajoQueryService queryService,
            IAuthenticationHelper authHelper)
        {
            _connectionFactory = connectionFactory;
            _queryService = queryService;
            _authHelper = authHelper;
        }

        public async Task<IEnumerable<OrdenTrabajoCatalogo>> GetByOrdenTrabajoIdAsync(int ordenTrabajoId)
        {
            await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();
            var todos = _queryService.LoadOrdenTrabajoCatalogos(connection);
            return todos.Where(c => c.OrdenTrabajoId == ordenTrabajoId).ToList();
        }

        public async Task<Result> AddAsync(OrdenTrabajoCatalogo entity)
        {
            try
            {
                await using var connection = (NpgsqlConnection)_connectionFactory.CreateConnection();
                await connection.OpenAsync();

                var actor = _authHelper.GetCurrentAuditActor();
                const string sql = @"
INSERT INTO ordentrabajocatalogo
    (ordentrabajoid, productoid, cantidadutilizada, preciounitario, creadopor, fecharegistro)
VALUES (@ordenid, @productoid, @cantidad, @precio, @actor, @fecha);";

                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("ordenid", entity.OrdenTrabajoId);
                cmd.Parameters.AddWithValue("productoid", entity.ProductoId);
                cmd.Parameters.AddWithValue("cantidad", entity.CantidadUtilizada);
                cmd.Parameters.AddWithValue("precio", entity.PrecioUnitario);
                cmd.Parameters.AddWithValue("actor", actor);
                cmd.Parameters.AddWithValue("fecha", entity.FechaRegistro);

                await cmd.ExecuteNonQueryAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorCodes.DbError,
                    $"Error al registrar catálogo de orden de trabajo: {ex.Message}");
            }
        }
    }
}
