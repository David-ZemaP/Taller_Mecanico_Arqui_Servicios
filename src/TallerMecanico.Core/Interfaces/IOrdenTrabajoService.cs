using TallerMecanico.Core.Models;

namespace TallerMecanico.Core.Interfaces;

public interface IOrdenTrabajoService
{
    IEnumerable<OrdenTrabajo> ObtenerTodas();
    IEnumerable<OrdenTrabajo> ObtenerPorCliente(int clienteId);
    OrdenTrabajo? ObtenerPorId(int id);
    OrdenTrabajo Crear(OrdenTrabajo orden);
    OrdenTrabajo? Actualizar(int id, OrdenTrabajo orden);
    OrdenTrabajo? CambiarEstado(int id, EstadoOrden nuevoEstado);
    bool Eliminar(int id);
}
