using OrdenTrabajoService.Domain.Entities;
using Taller_Mecanico_Users.Domain.Common;

namespace OrdenTrabajoService.Domain.Interfaces
{
    public interface IOrdenTrabajoCatalogoRepository
    {
        Task<IEnumerable<OrdenTrabajoCatalogo>> GetByOrdenTrabajoIdAsync(int ordenTrabajoId);
        Task<Result> AddAsync(OrdenTrabajoCatalogo entity);
    }
}
