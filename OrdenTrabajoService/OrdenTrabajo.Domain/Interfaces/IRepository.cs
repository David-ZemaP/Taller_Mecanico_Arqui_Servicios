using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Taller_Mecanico_Arqui.Domain.Common;

namespace Taller_Mecanico_Arqui.Domain.Ports
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