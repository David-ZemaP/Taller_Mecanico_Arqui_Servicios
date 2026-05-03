using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TallerMecanico.Core.Models;
using TallerMecanico.Services;

namespace Front.Pages;

public class OrdenTrabajoModel : PageModel
{
    private readonly OrdenTrabajoService _ordenTrabajoService;
    private readonly ClienteService _clienteService;
    private readonly VehiculoService _vehiculoService;

    public OrdenTrabajoModel(OrdenTrabajoService ordenTrabajoService, ClienteService clienteService, VehiculoService vehiculoService)
    {
        _ordenTrabajoService = ordenTrabajoService;
        _clienteService = clienteService;
        _vehiculoService = vehiculoService;
    }

    public OrdenTrabajo? Order { get; private set; }

    public string ClienteNombre { get; private set; } = string.Empty;

    public string VehiculoDescripcion { get; private set; } = string.Empty;

    public IActionResult OnGet(int? id)
    {
        if (!id.HasValue)
        {
            return Page();
        }

        Order = _ordenTrabajoService.ObtenerPorId(id.Value);
        if (Order is null)
        {
            return Page();
        }

        var cliente = _clienteService.ObtenerPorId(Order.ClienteId);
        var vehiculo = _vehiculoService.ObtenerPorId(Order.VehiculoId);

        ClienteNombre = cliente is null ? Order.ClienteId.ToString() : $"{cliente.Nombre} {cliente.Apellido}".Trim();
        VehiculoDescripcion = vehiculo is null ? Order.VehiculoId.ToString() : $"{vehiculo.Placa} - {vehiculo.Marca} {vehiculo.Modelo}".Trim();

        return Page();
    }
}