using Taller_Mecanico_Users.Domain.Ports;

namespace Taller_Mecanico_Users.UseCases.Empleados
{
    public class CreateEmpleadoUseCase
    {
        private readonly IEmpleadoRepository _repo;
        public CreateEmpleadoUseCase(IEmpleadoRepository repo) => _repo = repo;

        public Task<int> ExecuteAsync(NuevoEmpleadoRecord data) => _repo.CreateAsync(data);
    }
}
