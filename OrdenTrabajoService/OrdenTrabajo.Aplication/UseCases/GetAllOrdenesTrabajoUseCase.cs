using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Ports;

namespace Taller_Mecanico_Arqui.Application.UseCases.OrdenTrabajo
{
    public class GetAllOrdenesTrabajoUseCase
    {
        private readonly IOrdenTrabajoRepository _repository;

        public GetAllOrdenesTrabajoUseCase(IOrdenTrabajoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Domain.Entities.OrdenTrabajo>> ExecuteAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}
