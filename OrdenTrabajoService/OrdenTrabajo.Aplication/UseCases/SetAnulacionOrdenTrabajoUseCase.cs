using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Ports;

namespace Taller_Mecanico_Arqui.Application.UseCases.OrdenTrabajo
{
    public class SetAnulacionOrdenTrabajoUseCase
    {
        private readonly IOrdenTrabajoRepository _repository;

        public SetAnulacionOrdenTrabajoUseCase(IOrdenTrabajoRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result> ExecuteAsync(int ordenTrabajoId, bool anular)
        {
            var ordenResult = await _repository.GetByIdAsync(ordenTrabajoId);

            if (ordenResult.IsFailure)
                return Result.Failure(ordenResult.ErrorCode ?? ErrorCodes.DbError, ordenResult.ErrorMessage ?? "Error al consultar orden de trabajo.");

            if (anular)
            {
                if (ordenResult.Value == null)
                    return Result.Failure(ErrorCodes.OrdenTrabajoNotFound, $"Orden de trabajo con ID {ordenTrabajoId} no encontrada");
            }

            return await _repository.SetAnuladoAsync(ordenTrabajoId, anular);
        }
    }
}