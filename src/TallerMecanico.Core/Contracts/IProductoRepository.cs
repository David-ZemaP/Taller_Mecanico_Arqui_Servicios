using TallerMecanico.Core.Models;

namespace TallerMecanico.Core.Contracts;

public interface IProductoRepository
{
    Producto Add(Producto producto);

    Producto? GetById(int id);

    IEnumerable<Producto> GetAll();

    Producto? Update(int id, Producto producto);

    bool UpdateStock(int productoId, int delta);
}