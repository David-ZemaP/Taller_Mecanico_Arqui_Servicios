using Taller_Mecanico_Arqui.Domain.Ports;

namespace Taller_Mecanico_Arqui.Infrastructure.Persistence;

public abstract class RepositoryCreator<T> where T : class
{
    public abstract IRepository<T> CreateRepository();
}
