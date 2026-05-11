using System.Collections.Generic;
using System.Threading.Tasks;
using Taller_Mecanico_Users.Domain.Entities;

namespace Taller_Mecanico_Users.Domain.Ports
{
    public interface IRolRepository
    {
        Task<IEnumerable<Rol>> GetAllAsync();
        Task<Rol?> GetByIdAsync(int id);
        Task<Rol?> GetByNombreAsync(string nombre);
    }
}