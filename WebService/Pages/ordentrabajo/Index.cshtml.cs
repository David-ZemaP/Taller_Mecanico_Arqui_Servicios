using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebService.Adapters;
using WebService.DTOs;
using WebService.Models;

namespace WebService.Pages.OrdenTrabajo
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly OrdenTrabajoAdapter _adapter;

        public IndexModel(OrdenTrabajoAdapter adapter)
        {
            _adapter = adapter;
        }

        public IList<OrdenTrabajoListDto> OrdenesTrabajo { get; set; } = new List<OrdenTrabajoListDto>();
        public bool EsMecanico { get; private set; }

        [BindProperty]
        public OrdenTrabajoFormDto FormDto { get; set; } = new();

        public List<SelectListItem> EstadoTrabajoOptions { get; private set; } = new();
        public List<SelectListItem> EstadoPagoOptions { get; private set; } = new();

        public async Task OnGetAsync()
        {
            CargarOpcionesEstado();
            
            var userLevel = GetCurrentLevel();
            EsMecanico = userLevel == NivelAcceso.Parcial;
            
            if (EsMecanico)
            {
                var empleadoIdClaim = User.FindFirst("EmpleadoId");
                if (empleadoIdClaim != null && int.TryParse(empleadoIdClaim.Value, out int empleadoId))
                {
                    OrdenesTrabajo = await _adapter.GetOrdenesByMecanicoAsync(empleadoId);
                }
                else
                {
                    OrdenesTrabajo = new List<OrdenTrabajoListDto>();
                }
            }
            else
            {
                OrdenesTrabajo = await _adapter.GetAllOrdenesAsync();
            }
        }

        private NivelAcceso GetCurrentLevel()
        {
            var claim = User.FindFirst("NivelAcceso");
            return claim != null && Enum.TryParse<NivelAcceso>(claim.Value, out var lvl) ? lvl : NivelAcceso.Parcial;
        }

        public async Task<JsonResult> OnGetOrdenAsync(int id)
        {
            var orden = await _adapter.GetOrdenDetalleAsync(id);
            if (orden is null)
                return new JsonResult(new { error = "Orden no encontrada." }) { StatusCode = 404 };
            return new JsonResult(orden);
        }

        public async Task<JsonResult> OnGetBuscarClientesAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
                return new JsonResult(Array.Empty<object>());
            var clientes = await _adapter.BuscarClientesAsync(term);
            var result = clientes.Select(c => new
            {
                id = c.ClienteId,
                text = $"{c.Nombres} {c.PrimerApellido} - CI: {c.CiNumero}"
            });
            return new JsonResult(result);
        }

        public async Task<JsonResult> OnGetBuscarVehiculosAsync(string term, int? clienteId)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(Array.Empty<object>());
            var result = await _adapter.BuscarVehiculosAsync(term, clienteId);
            return new JsonResult(result);
        }

        public async Task<JsonResult> OnGetBuscarProductosAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(Array.Empty<object>());

            var productos = await _adapter.GetAllProductosAsync();
            var normalized = term.ToLower(CultureInfo.InvariantCulture);
            var result = productos
                .Where(p => p.Nombre.ToLower(CultureInfo.InvariantCulture)
                    .Contains(normalized, StringComparison.OrdinalIgnoreCase))
                .Select(p => new { id = p.ProductoId, text = p.Nombre, precio = p.Precio, stock = p.Stock })
                .Take(15)
                .ToList();
            return new JsonResult(result);
        }

        public async Task<JsonResult> OnGetBuscarServiciosAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(Array.Empty<object>());

            var servicios = await _adapter.GetAllServiciosAsync();
            var normalized = term.ToLower(CultureInfo.InvariantCulture);
            var result = servicios
                .Where(s => s.Nombre.ToLower(CultureInfo.InvariantCulture)
                    .Contains(normalized, StringComparison.OrdinalIgnoreCase))
                .Select(s => new { id = s.ServicioId, text = s.Nombre, precio = s.Precio })
                .Take(15)
                .ToList();
            return new JsonResult(result);
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            CargarOpcionesEstado();

            // Parse JSON fields populated by JavaScript before submitting
            ParseJsonItems();

            if (!ModelState.IsValid)
            {
                OrdenesTrabajo = await _adapter.GetAllOrdenesAsync();
                return Page();
            }

            bool ok;
            string? error;

            if (FormDto.OrdenTrabajoId == 0)
                (ok, error, _) = await _adapter.RegistrarOrdenAsync(FormDto);
            else
                (ok, error) = await _adapter.ActualizarOrdenAsync(FormDto);

            if (!ok)
            {
                ModelState.AddModelError(string.Empty, error ?? "No se pudo guardar la orden de trabajo.");
                OrdenesTrabajo = await _adapter.GetAllOrdenesAsync();
                return Page();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var (ok, error) = await _adapter.AnularOrdenAsync(id);
            if (!ok)
                TempData["ErrorMessage"] = error;
            return RedirectToPage();
        }

        private void ParseJsonItems()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(FormDto.ProductosJson) && FormDto.ProductosJson != "[]")
                    FormDto.Productos = JsonSerializer.Deserialize<List<OrdenTrabajoProductoItemDto>>(
                        FormDto.ProductosJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
            catch { FormDto.Productos = new(); }

            try
            {
                if (!string.IsNullOrWhiteSpace(FormDto.ServiciosJson) && FormDto.ServiciosJson != "[]")
                    FormDto.Servicios = JsonSerializer.Deserialize<List<OrdenTrabajoServicioItemDto>>(
                        FormDto.ServiciosJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }
            catch { FormDto.Servicios = new(); }
        }

        private void CargarOpcionesEstado()
        {
            EstadoTrabajoOptions = new List<string>
            {
                "Recibido", "EnDiagnostico", "EnReparacion",
                "EnEsperaRepuestos", "ListoParaEntrega", "Entregado"
            }
            .Select(e => new SelectListItem(e, e))
            .ToList();

            EstadoPagoOptions = new List<string> { "Pendiente", "Pagado", "Cancelado", "Rechazado" }
                .Select(e => new SelectListItem(e, e))
                .ToList();
        }
    }
}
