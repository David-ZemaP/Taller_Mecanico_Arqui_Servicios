using Taller_Mecanico_Arqui.Domain.Entities;

namespace Taller_Mecanico_Arqui.Domain.Ports
{
    public interface IVehiculoRepository : IRepository<Vehiculo>
    {
        Task<Vehiculo?> GetByPlacaAsync(string placa);
        Task<IEnumerable<Vehiculo>> GetByClienteIdAsync(int clienteId);
    }
}
