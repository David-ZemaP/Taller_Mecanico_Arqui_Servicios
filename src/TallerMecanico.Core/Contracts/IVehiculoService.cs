using TallerMecanico.Core.Models;

namespace TallerMecanico.Core.Contracts;

public interface IVehiculoService
{
    Vehiculo Crear(Vehiculo vehiculo);

    Vehiculo? ObtenerPorId(int id);

    IEnumerable<Vehiculo> ObtenerTodos();

    IEnumerable<Vehiculo> ObtenerPorCliente(int clienteId);

    Vehiculo? Actualizar(int id, Vehiculo vehiculo);

    bool Eliminar(int id, int? usuarioId = null);
}