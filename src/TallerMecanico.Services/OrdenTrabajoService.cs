using TallerMecanico.Core.Contracts;
using TallerMecanico.Core.Models;
using TallerMecanico.Services.Facades;
using TallerMecanico.Services.Infrastructure;

namespace TallerMecanico.Services;

public sealed class OrdenTrabajoService : IOrdenTrabajoService
{
    private readonly IRepositoryCreator _repositoryCreator;
    private readonly IOrdenTrabajoRepository _repository;
    private readonly OrdenTrabajoCreate _createFacade;
    private readonly OrdenTrabajoAnular _anularFacade;

    public OrdenTrabajoService()
        : this(new InMemoryRepositoryCreator())
    {
    }

    public OrdenTrabajoService(RepositoryCreator repositoryCreator)
    {
        _repositoryCreator = repositoryCreator;
        _repository = repositoryCreator.CreateOrdenTrabajoRepository();
        _createFacade = new OrdenTrabajoCreate(repositoryCreator);
        _anularFacade = new OrdenTrabajoAnular(repositoryCreator);
    }

    public OrdenTrabajo Crear(OrdenTrabajo ordenTrabajo, int? usuarioId = null)
    {
        ordenTrabajo.Recalcular();
        return _createFacade.Execute(ordenTrabajo, usuarioId);
    }

    public OrdenTrabajo? Actualizar(int id, OrdenTrabajo ordenTrabajo)
    {
        ordenTrabajo.Recalcular();
        return _repository.Update(id, ordenTrabajo);
    }

    public OrdenTrabajo? ObtenerPorId(int id) => _repository.GetById(id);

    public IEnumerable<OrdenTrabajo> ObtenerTodos() => _repository.GetAll();

    public IEnumerable<OrdenTrabajo> ObtenerPorCliente(int clienteId) => _repository.GetByCliente(clienteId);

    public OrdenTrabajo? CambiarEstado(int id, EstadoOrden estado, int? usuarioId = null) => _repository.CambiarEstado(id, estado, usuarioId);

    public OrdenTrabajo? Anular(int id, int usuarioId) => _anularFacade.Execute(id, usuarioId);
}