namespace TallerMecanico.Services.Facades;

public sealed class OrdenTrabajoAnular
{
    private readonly Infrastructure.RepositoryCreator _repositoryCreator;

    public OrdenTrabajoAnular(Infrastructure.RepositoryCreator repositoryCreator)
    {
        _repositoryCreator = repositoryCreator;
    }

    public Core.Models.OrdenTrabajo? Execute(int ordenTrabajoId, int usuarioId)
    {
        var repository = _repositoryCreator.CreateOrdenTrabajoRepository();
        return repository.Anular(ordenTrabajoId, usuarioId);
    }
}