using Taller_Mecanico_Arqui.Application.UseCases.OrdenTrabajo;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Ports;

namespace Taller_Mecanico_Arqui.Application.Facades
{
    public class OrdenTrabajoAnular
    {
        private readonly SetAnulacionOrdenTrabajoUseCase _setAnulacionUseCase;
        private readonly IOrdenTrabajoRepository _ordenTrabajoRepository;
        private readonly UpdateProductStocks _updateProductStocks;

        public OrdenTrabajoAnular(
            SetAnulacionOrdenTrabajoUseCase setAnulacionUseCase,
            IOrdenTrabajoRepository ordenTrabajoRepository,
            UpdateProductStocks updateProductStocks)
        {
            _setAnulacionUseCase = setAnulacionUseCase;
            _ordenTrabajoRepository = ordenTrabajoRepository;
            _updateProductStocks = updateProductStocks;
        }

        public async Task<Result> AnularProcesoPrincipalAsync(int ordenTrabajoId, bool anular)
        {
            if (!anular)
            {
                return await _setAnulacionUseCase.ExecuteAsync(ordenTrabajoId, anular: false);
            }

            var ordenResult = await _ordenTrabajoRepository.GetByIdAsync(ordenTrabajoId);
            if (ordenResult.IsFailure)
            {
                return Result.Failure(
                    ordenResult.ErrorCode ?? ErrorCodes.DbError,
                    ordenResult.ErrorMessage ?? "Error al consultar orden de trabajo.");
            }

            var orden = ordenResult.Value;
            if (orden == null)
            {
                return Result.Failure(ErrorCodes.OrdenTrabajoNotFound, $"Orden de trabajo con ID {ordenTrabajoId} no encontrada");
            }

            if (orden.IsDeleted)
            {
                return Result.Success();
            }

            var stockResult = await _updateProductStocks.RestoreAsync(orden.ProductosUsados);
            if (stockResult.IsFailure)
            {
                return stockResult;
            }

            return await _setAnulacionUseCase.ExecuteAsync(ordenTrabajoId, anular: true);
        }

        public Task<Result> DeleteAsync(int id)
            => AnularProcesoPrincipalAsync(id, anular: true);
    }
}