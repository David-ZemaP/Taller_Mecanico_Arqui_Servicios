using TallerMecanico.Core.Models;

namespace TallerMecanico.Core.Contracts;

public interface IOrdenTrabajoRepository
{
    OrdenTrabajo Add(OrdenTrabajo ordenTrabajo, int? usuarioId = null);

    OrdenTrabajo? GetById(int id);

    IEnumerable<OrdenTrabajo> GetAll();

    IEnumerable<OrdenTrabajo> GetByCliente(int clienteId);

    OrdenTrabajo? Update(int id, OrdenTrabajo ordenTrabajo);

    OrdenTrabajo? CambiarEstado(int id, EstadoOrden estado, int? usuarioId = null);

    OrdenTrabajo? Anular(int id, int usuarioId);
}