using TallerMecanico.Core.Models;

namespace TallerMecanico.Core.Contracts;

public interface IClienteService
{
    Cliente Crear(Cliente cliente);

    Cliente? ObtenerPorId(int id);

    IEnumerable<Cliente> ObtenerTodos();

    Cliente? Actualizar(int id, Cliente cliente);

    bool Eliminar(int id, int? usuarioId = null);
}