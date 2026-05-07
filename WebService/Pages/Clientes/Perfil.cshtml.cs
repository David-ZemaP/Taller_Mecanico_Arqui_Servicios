using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebService.Adapters;
using WebService.DTOs;

namespace WebService.Pages.Clientes
{
    [Authorize]
    public class PerfilModel : PageModel
    {
        private readonly ClientesAdapter _clientesAdapter;

        public ClienteDto? Cliente { get; set; }
        public string? ErrorMessage { get; set; }

        public PerfilModel(ClientesAdapter clientesAdapter)
        {
            _clientesAdapter = clientesAdapter;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var (ok, cliente, error) = await _clientesAdapter.GetClienteByIdAsync(id);
            if (ok && cliente != null)
            {
                Cliente = cliente;
                return Page();
            }
            else
            {
                ErrorMessage = error ?? "No se pudo cargar el cliente.";
                return RedirectToPage("Index");
            }
        }
    }
}
