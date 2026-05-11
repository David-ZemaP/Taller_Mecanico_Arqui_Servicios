using OrdenTrabajoService.Domain.Interfaces;
using Taller_Mecanico_Users.Domain.Common;

namespace OrdenTrabajoService.Application.UseCases
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
                return Result.Failure(ordenResult.ErrorCode!, ordenResult.ErrorMessage!);

            if (anular && ordenResult.Value == null)
                return Result.Failure(ErrorCodes.OrdenTrabajoNotFound,
                    $"Orden de trabajo con ID {ordenTrabajoId} no encontrada.");

            return await _repository.SetAnuladoAsync(ordenTrabajoId, anular);
        }
    }
}

