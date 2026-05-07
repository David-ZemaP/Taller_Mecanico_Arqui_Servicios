using OrdenTrabajoService.Application.UseCases;
using Taller_Mecanico_Users.Domain.Common;

namespace OrdenTrabajoService.Application.Facades
{
    public class OrdenTrabajoAnular
    {
        private readonly SetAnulacionOrdenTrabajoUseCase _setAnulacionUseCase;

        public OrdenTrabajoAnular(SetAnulacionOrdenTrabajoUseCase setAnulacionUseCase)
        {
            _setAnulacionUseCase = setAnulacionUseCase;
        }

        public Task<Result> AnularProcesoPrincipalAsync(int ordenTrabajoId, bool anular)
            => _setAnulacionUseCase.ExecuteAsync(ordenTrabajoId, anular);

        public Task<Result> DeleteAsync(int id)
            => AnularProcesoPrincipalAsync(id, anular: true);
    }
}
