using Taller_Mecanico_Arqui.Application.Facades;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Domain.Services;

namespace Taller_Mecanico_Arqui.Application.UseCases.OrdenTrabajo
{
    public class SetAnulacionOrdenTrabajoUseCase
    {
        private readonly IOrdenTrabajoRepository _repository;
        private readonly ICurrentUserService _currentUser;
        private readonly UpdateProductStocks _updateStocks;

        public SetAnulacionOrdenTrabajoUseCase(
            IOrdenTrabajoRepository repository,
            ICurrentUserService currentUser,
            UpdateProductStocks updateStocks)
        {
            _repository = repository;
            _currentUser = currentUser;
            _updateStocks = updateStocks;
        }

        public async Task<Result> ExecuteAsync(int ordenTrabajoId, bool anular)
        {
            var ordenResult = await _repository.GetByIdAsync(ordenTrabajoId);

            if (ordenResult.IsFailure)
                return Result.Failure(ordenResult.ErrorCode ?? ErrorCodes.DbError, ordenResult.ErrorMessage ?? "Error al consultar orden de trabajo.");

            string? auditUser = _currentUser.GetCurrentUserId() ?? _currentUser.GetCurrentUserEmail() ?? "system";

            if (anular)
            {
                if (ordenResult.Value == null)
                    return Result.Failure(ErrorCodes.OrdenTrabajoNotFound, $"Orden de trabajo con ID {ordenTrabajoId} no encontrada");

                // Restaurar stock de productos consumidos en la orden
                var productos = ordenResult.Value.ProductosUsados;
                if (productos != null && productos.Any())
                {
                    var restoreResult = await _updateStocks.RestoreAsync(productos);
                    if (restoreResult.IsFailure)
                        return restoreResult;
                }

                ordenResult.Value.MarcarEliminado(auditUser);
            }

            return await _repository.SetAnuladoAsync(ordenTrabajoId, anular, auditUser);
        }
    }
}