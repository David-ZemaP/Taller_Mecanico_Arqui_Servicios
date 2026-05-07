using Taller_Mecanico_Users.Domain.Ports;

namespace Taller_Mecanico_Users.UseCases.Empleados
{
    public class GetEmpleadosUseCase
    {
        private readonly IEmpleadoRepository _repo;
        public GetEmpleadosUseCase(IEmpleadoRepository repo) => _repo = repo;
        public Task<IEnumerable<EmpleadoRecord>> ExecuteAsync() => _repo.GetAllAsync();
    }
}
