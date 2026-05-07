using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Domain.Services;

namespace Taller_Mecanico_Arqui.Application.UseCases.OrdenTrabajo
{
    public class SetAnulacionOrdenTrabajoUseCase
    {
        private readonly IOrdenTrabajoRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public SetAnulacionOrdenTrabajoUseCase(IOrdenTrabajoRepository repository, ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
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
                
                ordenResult.Value.MarcarEliminado(auditUser);
            }

            return await _repository.SetAnuladoAsync(ordenTrabajoId, anular, auditUser);
        }
    }
}