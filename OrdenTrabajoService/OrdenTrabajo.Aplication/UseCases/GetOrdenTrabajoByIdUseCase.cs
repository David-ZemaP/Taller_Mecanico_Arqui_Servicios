using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Interfaces;
using Taller_Mecanico_Users.Domain.Common;

namespace OrdenTrabajoService.Application.UseCases
{
    public class GetOrdenTrabajoByIdUseCase
    {
        private readonly IOrdenTrabajoRepository _repository;

        public GetOrdenTrabajoByIdUseCase(IOrdenTrabajoRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<OrdenTrabajo>> ExecuteAsync(int id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (result.IsFailure)
                return Result<OrdenTrabajo>.Failure(result.ErrorCode!, result.ErrorMessage!);

            if (result.Value == null)
                return Result<OrdenTrabajo>.Failure(ErrorCodes.OrdenTrabajoNotFound,
                    $"Orden de trabajo con ID {id} no encontrada.");

            return Result<OrdenTrabajo>.Success(result.Value);
        }
    }
}

