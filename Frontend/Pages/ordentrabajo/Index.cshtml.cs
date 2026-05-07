using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taller_Mecanico_Arqui.Frontend.Adapters;
using Taller_Mecanico_Arqui.Frontend.Authorization;
using Taller_Mecanico_Arqui.Frontend.DTOs.OrdenTrabajo;

namespace Taller_Mecanico_Arqui.Pages.ordentrabajo
{
    [RequireAccessLevel(NivelAcceso.Parcial, NivelAcceso.Parcial, NivelAcceso.Completo)]
    public class IndexModel : PageModel
    {
        private readonly IOrdenTrabajoAdapter _ordenTrabajoAdapter;

        public IndexModel(IOrdenTrabajoAdapter ordenTrabajoAdapter)
        {
            _ordenTrabajoAdapter = ordenTrabajoAdapter;
        }

        public IList<OrdenTrabajoListDto> OrdenesTrabajo { get; set; } = new List<OrdenTrabajoListDto>();

        [BindProperty]
        public OrdenTrabajoFormDto FormDto { get; set; } = new();

        public List<SelectListItem> EstadoTrabajoOptions { get; private set; } = new();
        public List<SelectListItem> EstadoPagoOptions { get; private set; } = new();

        public async Task OnGetAsync()
        {
            CargarOpcionesEstado();
            OrdenesTrabajo = await _ordenTrabajoAdapter.GetAllAsync();
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
            // TODO: Implementar cuando esté el adapter de Clientes o usar endpoint del OrdenTrabajoService
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(new List<object>());

            // Placeholder: buscar clientes via OrdenTrabajoService si tiene endpoint
            // Por ahora retornar lista vacía hasta tener el endpoint
            return new JsonResult(new List<object>());
        }

        public async Task<JsonResult> OnGetBuscarProductosAsync(string term)
        {
            // TODO: Implementar cuando esté el adapter de Productos o endpoint en API
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(new List<object>());

            return new JsonResult(new List<object>());
        }

        public async Task<JsonResult> OnGetBuscarServiciosAsync(string term)
        {
            // TODO: Implementar cuando esté el adapter de Servicios o endpoint en API
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(new List<object>());

            return new JsonResult(new List<object>());
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            CargarOpcionesEstado();

            if (!ModelState.IsValid)
            {
                OrdenesTrabajo = await _ordenTrabajoAdapter.GetAllAsync();
                return Page();
            }

            // Parsear los JSON de productos y servicios
            if (!string.IsNullOrEmpty(FormDto.ProductosJson))
            {
                try
                {
                    // Los productos ya vienen en el DTO, no necesita parseo adicional
                }
                catch
                {
                    // Si falla el parsing, continuar sin productos
                }
            }

            var success = await _ordenTrabajoAdapter.SaveAsync(FormDto);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "No se pudo registrar la orden de trabajo.");
                OrdenesTrabajo = await _ordenTrabajoAdapter.GetAllAsync();
                return Page();
            }

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