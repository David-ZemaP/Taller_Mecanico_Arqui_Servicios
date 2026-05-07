using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taller_Mecanico_Arqui.Frontend.Adapters;
using Taller_Mecanico_Arqui.Frontend.DTOs.OrdenTrabajo;

namespace Taller_Mecanico_Arqui.Pages.ordentrabajo
{
    public class IndexModel : PageModel
    {
        private readonly IOrdenTrabajoAdapter _ordenTrabajoAdapter;
        private readonly IClienteAdapter _clienteAdapter;
        private readonly IProductoAdapter _productoAdapter;
        private readonly IServicioAdapter _servicioAdapter;

        public IndexModel(
            IOrdenTrabajoAdapter ordenTrabajoAdapter,
            IClienteAdapter clienteAdapter,
            IProductoAdapter productoAdapter,
            IServicioAdapter servicioAdapter)
        {
            _ordenTrabajoAdapter = ordenTrabajoAdapter;
            _clienteAdapter = clienteAdapter;
            _productoAdapter = productoAdapter;
            _servicioAdapter = servicioAdapter;
        }

        public IList<OrdenTrabajoListDto> OrdenesTrabajo { get; set; } = new List<OrdenTrabajoListDto>();

        [BindProperty]
        public OrdenTrabajoFormDto FormDto { get; set; } = new();

        public List<SelectListItem> EstadoTrabajoOptions { get; private set; } = new();
        public List<SelectListItem> EstadoPagoOptions { get; private set; } = new();

        public int? NewOrderId { get; set; }

        public async Task OnGetAsync()
        {
            CargarOpcionesEstado();
            OrdenesTrabajo = await _ordenTrabajoAdapter.GetAllAsync();
            if (TempData.TryGetValue("NewOrderId", out var val) && val is int id)
                NewOrderId = id;
        }

        public async Task<JsonResult> OnGetOrdenAsync(int id)
        {
            var orden = await _ordenTrabajoAdapter.GetByIdAsync(id);
            if (orden == null)
            {
                return new JsonResult(new { error = "Orden no encontrada." }) { StatusCode = 404 };
            }

            return new JsonResult(orden);
        }

        public async Task<JsonResult> OnGetBuscarVehiculosAsync(string term, int? clienteId)
        {
            var vehiculos = await _ordenTrabajoAdapter.BuscarVehiculosAsync(term, clienteId);
            return new JsonResult(vehiculos.Select(v => new { id = v.VehiculoId, text = v.Placa }));
        }

        public async Task<JsonResult> OnGetBuscarClientesAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(new List<object>());

            var clientes = await _clienteAdapter.GetAllAsync();
            var lower = term.ToLowerInvariant();
            var resultados = clientes
                .Where(c => c.NombreCompleto.ToLowerInvariant().Contains(lower) || c.Ci.Contains(term))
                .OrderBy(c => c.NombreCompleto)
                .Take(15)
                .Select(c => new { id = c.ClienteId, text = $"{c.NombreCompleto} — CI: {c.Ci}" });

            return new JsonResult(resultados);
        }

        public async Task<JsonResult> OnGetBuscarProductosAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(new List<object>());

            var productos = await _productoAdapter.GetAllAsync();
            var lower = term.ToLowerInvariant();
            var resultados = productos
                .Where(p => p.Activo && p.Nombre.ToLowerInvariant().Contains(lower))
                .OrderBy(p => p.Nombre)
                .Take(15)
                .Select(p => new { id = p.ProductoId, text = p.Nombre, precio = p.Precio, stock = p.Stock });

            return new JsonResult(resultados);
        }

        public async Task<JsonResult> OnGetBuscarServiciosAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(new List<object>());

            var servicios = await _servicioAdapter.GetAllAsync();
            var lower = term.ToLowerInvariant();
            var resultados = servicios
                .Where(s => s.Activo && s.Nombre.ToLowerInvariant().Contains(lower))
                .OrderBy(s => s.Nombre)
                .Take(15)
                .Select(s => new { id = s.ServicioId, text = s.Nombre, precio = s.Precio });

            return new JsonResult(resultados);
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            CargarOpcionesEstado();

            if (!ModelState.IsValid)
            {
                OrdenesTrabajo = await _ordenTrabajoAdapter.GetAllAsync();
                return Page();
            }

            var isNew = FormDto.OrdenTrabajoId == 0;
            var newId = await _ordenTrabajoAdapter.SaveAsync(FormDto);
            if (newId == null)
            {
                ModelState.AddModelError(string.Empty, "No se pudo registrar la orden de trabajo.");
                OrdenesTrabajo = await _ordenTrabajoAdapter.GetAllAsync();
                return Page();
            }

            if (isNew && newId > 0)
                TempData["NewOrderId"] = newId;

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var success = await _ordenTrabajoAdapter.AnularAsync(id);
            if (!success)
            {
                TempData["ErrorMessage"] = "No se pudo anular la orden de trabajo.";
            }

            return RedirectToPage();
        }

        private void CargarOpcionesEstado()
        {
            // Las opciones se cargan de forma síncrona
            // Se pueden cachear o cargar en OnGet y pasar a través de ViewData
        }

        public async Task OnGetCargarOpcionesAsync()
        {
            var estadosTrabajo = await _ordenTrabajoAdapter.GetEstadoTrabajoOptionsAsync();
            var estadosPago = await _ordenTrabajoAdapter.GetEstadoPagoOptionsAsync();

            EstadoTrabajoOptions = estadosTrabajo.Select(e => new SelectListItem(e, e)).ToList();
            EstadoPagoOptions = estadosPago.Select(e => new SelectListItem(e, e)).ToList();
        }
    }
}