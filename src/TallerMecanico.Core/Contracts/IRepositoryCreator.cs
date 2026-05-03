namespace TallerMecanico.Core.Contracts;

public interface IRepositoryCreator
{
    IClienteRepository CreateClienteRepository();

    IVehiculoRepository CreateVehiculoRepository();

    IProductoRepository CreateProductoRepository();

    IOrdenTrabajoRepository CreateOrdenTrabajoRepository();
}