using Taller_Mecanico_Users.Domain.Ports;

namespace Taller_Mecanico_Users.UseCases.Empleados
{
    public class UpdateEmpleadoUseCase
    {
        private readonly IEmpleadoRepository _repository;

        public UpdateEmpleadoUseCase(IEmpleadoRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(int id, NuevoEmpleadoRecord data)
        {
            var existing = await _repository.GetByIdAsync(id)
                ?? throw new InvalidOperationException("Empleado no encontrado.");
            await _repository.UpdateAsync(id, data);
        }
    }
}
