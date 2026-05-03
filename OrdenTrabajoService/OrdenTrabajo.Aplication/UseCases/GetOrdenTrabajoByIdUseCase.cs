using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Domain.Common;

namespace Taller_Mecanico_Arqui.Application.UseCases.OrdenTrabajo
{
    public class GetOrdenTrabajoByIdUseCase
    {
        private readonly IOrdenTrabajoRepository _repository;

        public GetOrdenTrabajoByIdUseCase(IOrdenTrabajoRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<Domain.Entities.OrdenTrabajo>> ExecuteAsync(int id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (result.IsFailure)
                return Result<Domain.Entities.OrdenTrabajo>.Failure(result.ErrorCode ?? ErrorCodes.DbError, result.ErrorMessage ?? "Error al consultar orden de trabajo.");

            if (result.Value == null)
                return Result<Domain.Entities.OrdenTrabajo>.Failure(ErrorCodes.OrdenTrabajoNotFound, $"Orden de trabajo con ID {id} no encontrada.");

            return Result<Domain.Entities.OrdenTrabajo>.Success(result.Value);
        }
    }
}
