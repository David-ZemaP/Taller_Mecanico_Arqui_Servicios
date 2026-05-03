using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Common;

namespace Taller_Mecanico_Arqui.Domain.Ports
{
    public interface IOrdenTrabajoRepository : IRepository<OrdenTrabajo>
    {
        Task<Result> SetAnuladoAsync(int id, bool anulado);
    }
}
