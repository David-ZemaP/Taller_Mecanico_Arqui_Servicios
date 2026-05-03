using TallerMecanico.Core.Contracts;
using TallerMecanico.Core.Models;
using TallerMecanico.Services.Infrastructure;

namespace TallerMecanico.Services;

public sealed class ClienteService : IClienteService
{
    private readonly IClienteRepository _repository;

    public ClienteService()
        : this(new InMemoryRepositoryCreator())
    {
    }

    public ClienteService(RepositoryCreator repositoryCreator)
    {
        _repository = repositoryCreator.CreateClienteRepository();
    }

    public Cliente Crear(Cliente cliente) => _repository.Add(cliente);

    public Cliente? ObtenerPorId(int id) => _repository.GetById(id);

    public IEnumerable<Cliente> ObtenerTodos() => _repository.GetAll();

    public Cliente? Actualizar(int id, Cliente cliente) => _repository.Update(id, cliente);

    public bool Eliminar(int id, int? usuarioId = null) => _repository.Delete(id, usuarioId);
}