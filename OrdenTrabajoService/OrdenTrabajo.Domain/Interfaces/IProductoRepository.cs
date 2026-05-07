using Taller_Mecanico_Arqui.Domain.Entities;

namespace Taller_Mecanico_Arqui.Domain.Ports
{
    public interface IProductoRepository : IRepository<Producto>
    {
        Task<Producto?> GetByNombreAsync(string nombre);
    }
}
