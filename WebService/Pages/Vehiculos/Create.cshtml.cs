using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TallerMecanico.Core.Models;
using TallerMecanico.Services;

namespace Front.Pages.Vehiculos;

public class CreateModel : PageModel
{
    private readonly VehiculoService _vehiculoService;

    public CreateModel(VehiculoService vehiculoService)
    {
        _vehiculoService = vehiculoService;
    }

    [BindProperty]
    public int ClienteId { get; set; }

    [BindProperty]
    public string Placa { get; set; } = string.Empty;

    [BindProperty]
    public string Marca { get; set; } = string.Empty;

    [BindProperty]
    public string Modelo { get; set; } = string.Empty;

    [BindProperty]
    public int Anio { get; set; }

    [BindProperty]
    public string? Color { get; set; }

    public IActionResult OnPost()
    {
        var vehiculo = _vehiculoService.Crear(new Vehiculo
        {
            ClienteId = ClienteId,
            Placa = Placa,
            Marca = Marca,
            Modelo = Modelo,
            Anio = Anio,
            Color = Color
        });

        return RedirectToPage("/Index", new { vehiculoId = vehiculo.Id });
    }
}