using Taller_Mecanico_Arqui.Application.UseCases.OrdenTrabajo;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Ports;

namespace Taller_Mecanico_Arqui.Application.Facades
{
    public class OrdenTrabajoAnular
    {
        private readonly SetAnulacionOrdenTrabajoUseCase _setAnulacionUseCase;

        public OrdenTrabajoAnular(
            SetAnulacionOrdenTrabajoUseCase setAnulacionUseCase)
        {
            _setAnulacionUseCase = setAnulacionUseCase;
        }

        public async Task<Result> AnularProcesoPrincipalAsync(int ordenTrabajoId, bool anular)
        {
            return await _setAnulacionUseCase.ExecuteAsync(ordenTrabajoId, anular);
        }

        public Task<Result> DeleteAsync(int id)
            => AnularProcesoPrincipalAsync(id, anular: true);
    }
}