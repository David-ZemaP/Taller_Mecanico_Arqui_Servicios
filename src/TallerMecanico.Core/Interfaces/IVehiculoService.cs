using TallerMecanico.Core.Models;

namespace TallerMecanico.Core.Interfaces;

public interface IVehiculoService
{
    IEnumerable<Vehiculo> ObtenerTodos();
    IEnumerable<Vehiculo> ObtenerPorCliente(int clienteId);
    Vehiculo? ObtenerPorId(int id);
    Vehiculo Crear(Vehiculo vehiculo);
    Vehiculo? Actualizar(int id, Vehiculo vehiculo);
    bool Eliminar(int id);
}
