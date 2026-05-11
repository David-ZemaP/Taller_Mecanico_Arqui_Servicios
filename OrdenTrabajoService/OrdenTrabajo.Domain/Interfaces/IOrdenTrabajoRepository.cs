using OrdenTrabajoService.Domain.Entities;
using Taller_Mecanico_Users.Domain.Common;

namespace OrdenTrabajoService.Domain.Interfaces
{
    public interface IOrdenTrabajoRepository : IRepository<OrdenTrabajo>
    {
        Task<Result> SetAnuladoAsync(int id, bool anulado);
    }
}

