using TallerMecanico.Core.Interfaces;
using TallerMecanico.Core.Models;

namespace TallerMecanico.Services;

public class ServicioService : IServicioService
{
    private readonly List<Servicio> _servicios = new();
    private int _nextId = 1;

    public ServicioService()
    {
        // Datos de ejemplo
        _servicios.AddRange(new[]
        {
            new Servicio { Id = _nextId++, Nombre = "Cambio de Aceite", Descripcion = "Cambio de aceite de motor y filtro", Precio = 350.00m, DuracionEstimadaHoras = 1 },
            new Servicio { Id = _nextId++, Nombre = "Alineación y Balanceo", Descripcion = "Alineación de ruedas y balanceo de llantas", Precio = 500.00m, DuracionEstimadaHoras = 2 },
            new Servicio { Id = _nextId++, Nombre = "Revisión de Frenos", Descripcion = "Revisión y ajuste del sistema de frenos", Precio = 400.00m, DuracionEstimadaHoras = 2 },
            new Servicio { Id = _nextId++, Nombre = "Diagnóstico General", Descripcion = "Diagnóstico electrónico completo del vehículo", Precio = 600.00m, DuracionEstimadaHoras = 1 },
        });
    }

    public IEnumerable<Servicio> ObtenerTodos() => _servicios.AsReadOnly();

    public Servicio? ObtenerPorId(int id) =>
        _servicios.FirstOrDefault(s => s.Id == id);

    public Servicio Crear(Servicio servicio)
    {
        servicio.Id = _nextId++;
        _servicios.Add(servicio);
        return servicio;
    }

    public Servicio? Actualizar(int id, Servicio servicio)
    {
        var existente = _servicios.FirstOrDefault(s => s.Id == id);
        if (existente is null) return null;

        existente.Nombre = servicio.Nombre;
        existente.Descripcion = servicio.Descripcion;
        existente.Precio = servicio.Precio;
        existente.DuracionEstimadaHoras = servicio.DuracionEstimadaHoras;
        return existente;
    }

    public bool Eliminar(int id)
    {
        var servicio = _servicios.FirstOrDefault(s => s.Id == id);
        if (servicio is null) return false;
        _servicios.Remove(servicio);
        return true;
    }
}
