using TallerMecanico.Core.Contracts;
using TallerMecanico.Services.Infrastructure.Repositories;

namespace TallerMecanico.Services.Infrastructure;

public sealed class InMemoryRepositoryCreator : RepositoryCreator
{
    private readonly InMemoryTallerContext _context = new();
    private readonly Lazy<UpdateProductStocks> _updateProductStocks;

    public InMemoryRepositoryCreator()
    {
        Seed();
        _updateProductStocks = new Lazy<UpdateProductStocks>(() => new UpdateProductStocks(CreateProductoRepository()));
    }

    public override IClienteRepository CreateClienteRepository() => new InMemoryClienteRepository(_context);

    public override IVehiculoRepository CreateVehiculoRepository() => new InMemoryVehiculoRepository(_context);

    public override IProductoRepository CreateProductoRepository() => new InMemoryProductoRepository(_context);

    public override IOrdenTrabajoRepository CreateOrdenTrabajoRepository() => new OrdenTrabajoRepository(_context, _updateProductStocks.Value);

    private void Seed()
    {
        if (_context.Productos.Count == 0)
        {
            CreateProductoRepository().Add(new Core.Models.Producto { Nombre = "Aceite sintético", Precio = 350m, Stock = 25 });
            CreateProductoRepository().Add(new Core.Models.Producto { Nombre = "Filtro de aceite", Precio = 180m, Stock = 20 });
            CreateProductoRepository().Add(new Core.Models.Producto { Nombre = "Bujías", Precio = 220m, Stock = 40 });
        }

        if (_context.Clientes.Count == 0)
        {
            CreateClienteRepository().Add(new Core.Models.Cliente { Nombre = "Cliente", Apellido = "Demo" });
        }
    }
}