using Taller_Mecanico_Users.Domain.Ports;

namespace Taller_Mecanico_Users.UseCases.Empleados
{
    public class DeleteEmpleadoUseCase
    {
        private readonly IEmpleadoRepository _repository;

        public DeleteEmpleadoUseCase(IEmpleadoRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
