using TallerMecanico.Core.Models;
using TallerMecanico.Services.Infrastructure;

namespace TallerMecanico.Services;

public sealed class ServicioCatalogoService
{
    private readonly List<Servicio> _servicios = new()
    {
        new Servicio { Id = 1, Nombre = "Cambio de aceite", Precio = 350m },
        new Servicio { Id = 2, Nombre = "Alineación", Precio = 500m },
        new Servicio { Id = 3, Nombre = "Diagnóstico eléctrico", Precio = 700m }
    };

    public IEnumerable<Servicio> ObtenerTodos() => _servicios.Where(servicio => !servicio.IsDeleted).ToList();

    public Servicio? ObtenerPorId(int id) => _servicios.FirstOrDefault(servicio => servicio.Id == id && !servicio.IsDeleted);
}