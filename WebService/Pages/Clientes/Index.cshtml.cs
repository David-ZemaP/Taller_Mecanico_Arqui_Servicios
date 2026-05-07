using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebService.Adapters;
using WebService.DTOs;

namespace WebService.Pages.Clientes
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ClientesAdapter _clientesAdapter;
        private readonly OrdenTrabajoAdapter _ordenTrabajoAdapter;

        public IEnumerable<ClienteDto> Clientes { get; set; } = [];
        public string? ErrorMessage { get; set; }

        public IndexModel(ClientesAdapter clientesAdapter, OrdenTrabajoAdapter ordenTrabajoAdapter)
        {
            _clientesAdapter = clientesAdapter;
            _ordenTrabajoAdapter = ordenTrabajoAdapter;
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
                ErrorMessage = ParseErrorMessage(error) ?? "No se pudieron cargar los clientes.";
                Clientes = [];
            }
        }

        public async Task<JsonResult> OnGetClienteAsync(int id)
        {
            var cliente = await _ordenTrabajoAdapter.GetClienteAsync(id);
            if (cliente is null)
                return new JsonResult(new { error = "Cliente no encontrado." }) { StatusCode = 404 };
            return new JsonResult(cliente);
        }

        public async Task<JsonResult> OnPostSaveAjaxAsync([FromBody] ClienteFormDto data)
        {
            if (data is null)
                return new JsonResult(new { success = false, message = "Datos inválidos." });

            var (ok, error, cliente) = await _ordenTrabajoAdapter.SaveClienteAsync(data);
            if (!ok)
                return new JsonResult(new { success = false, message = ParseErrorMessage(error) ?? error }) { StatusCode = 400 };

            return new JsonResult(new { success = true, cliente });
        }

        private static string? ParseErrorMessage(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("message", out var msg))
                    return msg.GetString();
            }
            catch { }
            return raw;
        }
    }
}
