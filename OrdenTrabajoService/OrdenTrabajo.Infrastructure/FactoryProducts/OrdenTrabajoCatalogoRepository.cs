using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Infrastructure.Persistence;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Infrastructure.Services;

namespace Taller_Mecanico_Arqui.Infrastructure.Persistence.Repositories
{
    public class OrdenTrabajoCatalogoRepository : IOrdenTrabajoCatalogoRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly SqlEntityQueryService _queryService;
        private readonly AuthenticationHelper _authHelper;

        public OrdenTrabajoCatalogoRepository(
            ISqlConnectionFactory connectionFactory,
            SqlEntityQueryService queryService,
            AuthenticationHelper authHelper)
        {
            _connectionFactory = connectionFactory;
            _queryService = queryService;
            _authHelper = authHelper;
        }

        public async Task<IEnumerable<OrdenTrabajoCatalogo>> GetByOrdenTrabajoIdAsync(int ordenTrabajoId)
        {
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var todosCatalogos = _queryService.LoadOrdenTrabajoCatalogos(connection);
            return todosCatalogos.Where(c => c.OrdenTrabajoId == ordenTrabajoId).ToList();
        }

        public async Task<Result> AddAsync(OrdenTrabajoCatalogo entity)
        {
            try
            {
                await using var connection = _connectionFactory.CreateConnection();
                await connection.OpenAsync();

                var sql = @"
INSERT INTO ordentrabajocatalogo (ordentrabajoid, productoid, cantidadutilizada, preciounitario, creadopor, fecharegistro)
VALUES (@ordentrabajoid, @productoid, @cantidadutilizada, @preciounitario, @creadopor, @fecharegistro);";

                var actorAuditoria = _authHelper.GetCurrentAuditActor();

                await using var command = new Npgsql.NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("ordentrabajoid", entity.OrdenTrabajoId);
                command.Parameters.AddWithValue("productoid", entity.ProductoId);
                command.Parameters.AddWithValue("cantidadutilizada", entity.CantidadUtilizada);
                command.Parameters.AddWithValue("preciounitario", entity.PrecioUnitario);
                command.Parameters.AddWithValue("creadopor", actorAuditoria);
                command.Parameters.AddWithValue("fecharegistro", entity.FechaRegistro);

                await command.ExecuteNonQueryAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(
                    ErrorCodes.DbError,
                    $"Error al registrar el catálogo de la orden de trabajo: {ex.Message}");
            }
        }
    }
}