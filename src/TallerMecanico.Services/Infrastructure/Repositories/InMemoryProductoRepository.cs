using TallerMecanico.Core.Contracts;
using TallerMecanico.Core.Models;

namespace TallerMecanico.Services.Infrastructure.Repositories;

internal sealed class InMemoryProductoRepository : IProductoRepository
{
    private readonly InMemoryTallerContext _context;

    public InMemoryProductoRepository(InMemoryTallerContext context)
    {
        _context = context;
    }

    public Producto Add(Producto producto)
    {
        var stored = new Producto
        {
            Id = _context.NextProductoId++,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            Precio = producto.Precio,
            Stock = producto.Stock
        };

        _context.Productos.Add(stored);
        return stored;
    }

    public Producto? GetById(int id) => _context.Productos.FirstOrDefault(producto => producto.Id == id && !producto.IsDeleted);

    public IEnumerable<Producto> GetAll() => _context.Productos.Where(producto => !producto.IsDeleted).ToList();

    public Producto? Update(int id, Producto producto)
    {
        var stored = _context.Productos.FirstOrDefault(item => item.Id == id && !item.IsDeleted);
        if (stored is null)
        {
            return null;
        }

        stored.Nombre = producto.Nombre;
        stored.Descripcion = producto.Descripcion;
        stored.Precio = producto.Precio;
        stored.Stock = producto.Stock;
        return stored;
    }

    public bool UpdateStock(int productoId, int delta)
    {
        var stored = _context.Productos.FirstOrDefault(item => item.Id == productoId && !item.IsDeleted);
        if (stored is null)
        {
            return false;
        }

        if (stored.Stock + delta < 0)
        {
            return false;
        }

        stored.Stock += delta;
        return true;
    }
}