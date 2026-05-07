using System.Collections.Generic;
using System.Threading.Tasks;
using Taller_Mecanico_Users.Domain.Entities;
using Taller_Mecanico_Users.Domain.Common;

namespace Taller_Mecanico_Users.Domain.Ports
{
    public interface IUsuarioLoginRepository
    {
        Task<IEnumerable<UsuarioLogin>> GetAllAsync();
        Task<UsuarioLogin?> GetByEmailAsync(string email);
        Task<UsuarioLogin?> GetByUsernameAsync(string username);
        Task<Result<UsuarioLogin?>> GetByIdAsync(int id);
        Task<UsuarioLogin?> GetByEmpleadoIdAsync(int empleadoId);
        Task<UsuarioLogin?> GetByClienteIdAsync(int clienteId);
        Task<Result> AddAsync(UsuarioLogin entity);
        Task<Result> UpdateAsync(UsuarioLogin entity);
        Task<Result> DeleteAsync(int id);
    }
}
