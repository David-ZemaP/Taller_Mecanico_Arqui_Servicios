using TallerMecanico.Core.Interfaces;
using TallerMecanico.Core.Models;

namespace TallerMecanico.Services;

public class OrdenTrabajoService : IOrdenTrabajoService
{
    private readonly List<OrdenTrabajo> _ordenes = new();
    private int _nextId = 1;

    public IEnumerable<OrdenTrabajo> ObtenerTodas() => _ordenes.AsReadOnly();

    public IEnumerable<OrdenTrabajo> ObtenerPorCliente(int clienteId) =>
        _ordenes.Where(o => o.ClienteId == clienteId);

    public OrdenTrabajo? ObtenerPorId(int id) =>
        _ordenes.FirstOrDefault(o => o.Id == id);

    public OrdenTrabajo Crear(OrdenTrabajo orden)
    {
        orden.Id = _nextId++;
        orden.FechaCreacion = DateTime.UtcNow;
        orden.Estado = EstadoOrden.Pendiente;
        _ordenes.Add(orden);
        return orden;
    }

    public OrdenTrabajo? Actualizar(int id, OrdenTrabajo orden)
    {
        var existente = _ordenes.FirstOrDefault(o => o.Id == id);
        if (existente is null) return null;

        existente.Descripcion = orden.Descripcion;
        existente.Observaciones = orden.Observaciones;
        existente.Servicios = orden.Servicios;
        return existente;
    }

    public OrdenTrabajo? CambiarEstado(int id, EstadoOrden nuevoEstado)
    {
        var orden = _ordenes.FirstOrDefault(o => o.Id == id);
        if (orden is null) return null;

        orden.Estado = nuevoEstado;
        if (nuevoEstado == EstadoOrden.Completada)
            orden.FechaCompletado = DateTime.UtcNow;

        return orden;
    }

    public bool Eliminar(int id)
    {
        var orden = _ordenes.FirstOrDefault(o => o.Id == id);
        if (orden is null) return false;
        _ordenes.Remove(orden);
        return true;
    }
}
