using TallerMecanico.Core.Contracts;
using TallerMecanico.Core.Models;
using TallerMecanico.Services.Infrastructure;

namespace TallerMecanico.Services;

public sealed class ProductoService
{
    private readonly IProductoRepository _repository;

    public ProductoService()
        : this(new InMemoryRepositoryCreator())
    {
    }

    public ProductoService(RepositoryCreator repositoryCreator)
    {
        _repository = repositoryCreator.CreateProductoRepository();
    }

    public IEnumerable<Producto> ObtenerTodos() => _repository!.GetAll();

    public Producto? ObtenerPorId(int id) => _repository!.GetById(id);
}