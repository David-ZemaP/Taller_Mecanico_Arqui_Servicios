using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Taller_Mecanico_Arqui.Domain.Ports
{
    public interface IOrdenTrabajoCatalogoRepository
    {
        Task<IEnumerable<OrdenTrabajoCatalogo>> GetByOrdenTrabajoIdAsync(int ordenTrabajoId);
        Task<Result> AddAsync(OrdenTrabajoCatalogo entity);
    }
}