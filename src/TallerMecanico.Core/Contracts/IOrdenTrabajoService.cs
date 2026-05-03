using TallerMecanico.Core.Models;

namespace TallerMecanico.Core.Contracts;

public interface IOrdenTrabajoService
{
    OrdenTrabajo Crear(OrdenTrabajo ordenTrabajo, int? usuarioId = null);

    OrdenTrabajo? Actualizar(int id, OrdenTrabajo ordenTrabajo);

    OrdenTrabajo? ObtenerPorId(int id);

    IEnumerable<OrdenTrabajo> ObtenerTodos();

    IEnumerable<OrdenTrabajo> ObtenerPorCliente(int clienteId);

    OrdenTrabajo? CambiarEstado(int id, EstadoOrden estado, int? usuarioId = null);

    OrdenTrabajo? Anular(int id, int usuarioId);
}