using Taller_Mecanico_Arqui.Application.UseCases.OrdenTrabajo;
using Taller_Mecanico_Arqui.Domain.Common;

namespace Taller_Mecanico_Arqui.Application.Facades
{
    public class OrdenTrabajoAnular
    {
        private readonly SetAnulacionOrdenTrabajoUseCase _setAnulacionUseCase;

        public OrdenTrabajoAnular(SetAnulacionOrdenTrabajoUseCase setAnulacionUseCase)
        {
            _setAnulacionUseCase = setAnulacionUseCase;
        }

        public async Task<Result> ExecuteAsync(int ordenTrabajoId)
        {
            return await _setAnulacionUseCase.ExecuteAsync(ordenTrabajoId, true);
        }

        public async Task<Result> ReactivarAsync(int ordenTrabajoId)
        {
            return await _setAnulacionUseCase.ExecuteAsync(ordenTrabajoId, false);
        }

        public async Task<Result> AnularProcesoPrincipalAsync(int ordenTrabajoId, bool anular)
        {
            return await _setAnulacionUseCase.ExecuteAsync(ordenTrabajoId, anular);
        }
    }
}