using Taller_Mecanico_Arqui.Domain.Entities;

namespace Taller_Mecanico_Arqui.Domain.Ports
{
    public interface IServicioRepository : IRepository<Servicio>
    {
        Task<Servicio?> GetByNombreAsync(string nombre);
    }
}
