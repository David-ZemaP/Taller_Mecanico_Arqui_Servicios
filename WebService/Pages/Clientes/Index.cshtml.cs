using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebService.Adapters;
using WebService.DTOs;

namespace WebService.Pages.Clientes
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ClientesAdapter _clientesAdapter;

        public IEnumerable<ClienteDto> Clientes { get; set; } = [];
        public string? ErrorMessage { get; set; }

        public IndexModel(ClientesAdapter clientesAdapter)
        {
            _clientesAdapter = clientesAdapter;
        }

        public async Task OnGetAsync()
        {
            var (ok, clientes, error) = await _clientesAdapter.GetAllClientesAsync();
            if (ok && clientes != null)
            {
                Clientes = clientes;
            }
            else
            {
                ErrorMessage = error ?? "No se pudieron cargar los clientes.";
                Clientes = [];
            }
        }
    }
}
