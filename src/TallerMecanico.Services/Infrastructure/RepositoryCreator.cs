using TallerMecanico.Core.Contracts;

namespace TallerMecanico.Services.Infrastructure;

public abstract class RepositoryCreator : IRepositoryCreator
{
    public abstract IClienteRepository CreateClienteRepository();

    public abstract IVehiculoRepository CreateVehiculoRepository();

    public abstract IProductoRepository CreateProductoRepository();

    public abstract IOrdenTrabajoRepository CreateOrdenTrabajoRepository();
}