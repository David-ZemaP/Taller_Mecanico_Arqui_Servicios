using Taller_Mecanico_Users.Domain.Common;

namespace OrdenTrabajoService.Domain.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<Result<T?>> GetByIdAsync(int id);
        Task<Result<int>> AddAsync(T entity);
        Task<Result> UpdateAsync(T entity);
        Task DeleteAsync(int id);
    }
}

