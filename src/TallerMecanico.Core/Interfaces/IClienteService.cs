using TallerMecanico.Core.Models;

namespace TallerMecanico.Core.Interfaces;

public interface IClienteService
{
    IEnumerable<Cliente> ObtenerTodos();
    Cliente? ObtenerPorId(int id);
    Cliente Crear(Cliente cliente);
    Cliente? Actualizar(int id, Cliente cliente);
    bool Eliminar(int id);
}
