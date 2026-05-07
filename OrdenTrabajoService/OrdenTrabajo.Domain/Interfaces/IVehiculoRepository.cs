using OrdenTrabajoService.Domain.Entities;

namespace OrdenTrabajoService.Domain.Interfaces
{
    public interface IVehiculoRepository : IRepository<Vehiculo>
    {
        Task<IEnumerable<Vehiculo>> BuscarPorPlacaAsync(string term, int? clienteId = null);
    }
}
