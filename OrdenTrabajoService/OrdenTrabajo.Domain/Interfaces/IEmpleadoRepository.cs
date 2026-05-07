using Taller_Mecanico_Arqui.Domain.Entities;

namespace Taller_Mecanico_Arqui.Domain.Ports
{
    public interface IEmpleadoRepository : IRepository<Empleado>
    {
        Task<Empleado?> GetByCiAsync(int ci);
    }
}
