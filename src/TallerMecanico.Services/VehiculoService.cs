using TallerMecanico.Core.Contracts;
using TallerMecanico.Core.Models;
using TallerMecanico.Services.Infrastructure;

namespace TallerMecanico.Services;

public sealed class VehiculoService : IVehiculoService
{
    private readonly IVehiculoRepository _repository;

    public VehiculoService()
        : this(new InMemoryRepositoryCreator())
    {
    }

    public VehiculoService(RepositoryCreator repositoryCreator)
    {
        _repository = repositoryCreator.CreateVehiculoRepository();
    }

    public Vehiculo Crear(Vehiculo vehiculo) => _repository.Add(vehiculo);

    public Vehiculo? ObtenerPorId(int id) => _repository.GetById(id);

    public IEnumerable<Vehiculo> ObtenerTodos() => _repository.GetAll();

    public IEnumerable<Vehiculo> ObtenerPorCliente(int clienteId) => _repository.GetByCliente(clienteId);

    public Vehiculo? Actualizar(int id, Vehiculo vehiculo) => _repository.Update(id, vehiculo);

    public bool Eliminar(int id, int? usuarioId = null) => _repository.Delete(id, usuarioId);
}