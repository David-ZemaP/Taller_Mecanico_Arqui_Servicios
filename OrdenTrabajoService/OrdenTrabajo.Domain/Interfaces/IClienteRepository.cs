using Taller_Mecanico_Arqui.Domain.Entities;

namespace Taller_Mecanico_Arqui.Domain.Ports
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<Cliente?> GetByCiAsync(int ci);
        Task<Cliente?> GetByEmailAsync(string email);
    }
}
