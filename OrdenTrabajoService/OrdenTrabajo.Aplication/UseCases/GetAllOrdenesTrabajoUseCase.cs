using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Interfaces;

namespace OrdenTrabajoService.Application.UseCases
{
    public class GetAllOrdenesTrabajoUseCase
    {
        private readonly IOrdenTrabajoRepository _repository;

        public GetAllOrdenesTrabajoUseCase(IOrdenTrabajoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<OrdenTrabajo>> ExecuteAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}
