using TallerMecanico.Core.Models;

namespace TallerMecanico.Core.Interfaces;

public interface IServicioService
{
    IEnumerable<Servicio> ObtenerTodos();
    Servicio? ObtenerPorId(int id);
    Servicio Crear(Servicio servicio);
    Servicio? Actualizar(int id, Servicio servicio);
    bool Eliminar(int id);
}
