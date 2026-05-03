using TallerMecanico.Core.Models;
using TallerMecanico.Services.Infrastructure;

namespace TallerMecanico.Services.Facades;

public sealed class OrdenTrabajoCreate
{
    private readonly RepositoryCreator _repositoryCreator;

    public OrdenTrabajoCreate(RepositoryCreator repositoryCreator)
    {
        _repositoryCreator = repositoryCreator;
    }

    public OrdenTrabajo Execute(OrdenTrabajo ordenTrabajo, int? usuarioId = null)
    {
        var repository = _repositoryCreator.CreateOrdenTrabajoRepository();
        return repository.Add(ordenTrabajo, usuarioId);
    }
}