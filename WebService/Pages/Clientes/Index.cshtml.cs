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
        private readonly OrdenTrabajoAdapter _adapter;

        public IList<ClienteLookupDto> Clientes { get; set; } = [];

        public IndexModel(OrdenTrabajoAdapter adapter)
        {
            _adapter = adapter;
        }

        public async Task OnGetAsync()
        {
            Clientes = await _adapter.GetAllClientesAsync();
        }

        public async Task<JsonResult> OnGetBuscarClientesAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(Array.Empty<object>());

            var clientes = await _adapter.BuscarClientesAsync(term);
            var result = clientes
                .Select(c => new { id = c.ClienteId, text = $"{c.Nombres} {c.PrimerApellido} {c.SegundoApellido} - CI: {c.CiNumero}{c.CiComplemento}" })
                .Take(15)
                .ToList();
            return new JsonResult(result);
        }

        public async Task<JsonResult> OnGetClienteAsync(int id)
        {
            var cliente = await _adapter.GetClienteAsync(id);
            if (cliente is null)
                return new JsonResult(new { error = "Cliente no encontrado." }) { StatusCode = 404 };

            var vehiculos = await _adapter.GetVehiculosByClienteAsync(id);
            return new JsonResult(new
            {
                clienteId = cliente.ClienteId,
                nombres = cliente.Nombres,
                primerApellido = cliente.PrimerApellido,
                segundoApellido = cliente.SegundoApellido,
                ciNumero = cliente.CiNumero,
                ciComplemento = cliente.CiComplemento,
                telefono = cliente.Telefono,
                email = cliente.Email,
                fechaRegistro = cliente.FechaRegistro,
                vehiculos = vehiculos.Select(v => new
                {
                    placa = v.Placa,
                    marca = v.MarcaNombre,
                    modelo = v.ModeloNombre,
                    color = v.ColorNombre,
                    anio = v.Anio
                })
            });
        }

        public async Task<JsonResult> OnPostSaveAjaxAsync([FromBody] ClienteFormDto data)
        {
            if (data is null)
                return new JsonResult(new { success = false, message = "Datos inválidos." });

            var (ok, error, cliente) = await _adapter.SaveClienteAsync(data);
            if (!ok)
                return new JsonResult(new { success = false, message = ParseErrorMessage(error) ?? error }) { StatusCode = 400 };

            return new JsonResult(new { success = true, cliente });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var (ok, error) = await _adapter.DeleteClienteAsync(id);
            if (!ok)
                TempData["ErrorMessage"] = ParseErrorMessage(error) ?? "No se pudo eliminar el cliente.";
            return RedirectToPage();
        }

        private static string? ParseErrorMessage(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("message", out var msg))
                    return msg.GetString();
                if (doc.RootElement.TryGetProperty("error", out var err))
                    return err.GetString();
            }
            catch { }
            return raw;
        }
    }
}
