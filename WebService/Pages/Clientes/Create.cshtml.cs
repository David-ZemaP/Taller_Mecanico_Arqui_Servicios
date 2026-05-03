using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TallerMecanico.Core.Models;
using TallerMecanico.Services;

namespace Front.Pages.Clientes;

public class CreateModel : PageModel
{
    private readonly ClienteService _clienteService;

    public CreateModel(ClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [BindProperty]
    public string Nombre { get; set; } = string.Empty;

    [BindProperty]
    public string Apellido { get; set; } = string.Empty;

    [BindProperty]
    public string? Telefono { get; set; }

    [BindProperty]
    public string? Email { get; set; }

    public IActionResult OnPost()
    {
        var cliente = _clienteService.Crear(new Cliente
        {
            Nombre = Nombre,
            Apellido = Apellido,
            Telefono = Telefono,
            Email = Email
        });

        return RedirectToPage("/Index", new { clienteId = cliente.Id });
    }
}