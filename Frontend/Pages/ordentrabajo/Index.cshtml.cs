using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taller_Mecanico_Arqui.Application.Facades;
using Taller_Mecanico_Arqui.Application.DTOs.OrdenTrabajo;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Enums;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Infrastructure.Authorization;

namespace Taller_Mecanico_Arqui.Pages.ordentrabajo
{
    [RequireAccessLevel(NivelAcceso.Parcial)]
    public class IndexModel : PageModel
    {
        private readonly OrdenTrabajoCreate _ordenTrabajoCreate;
        private readonly OrdenTrabajoAnular _ordenTrabajoAnular;
        private readonly IRepository<Producto> _productoRepository;
        private readonly IRepository<Servicio> _servicioRepository;
        private readonly IClienteRepository _clienteRepository;

        public IndexModel(
            OrdenTrabajoCreate ordenTrabajoCreate,
            OrdenTrabajoAnular ordenTrabajoAnular,
            IRepository<Producto> productoRepository,
            IRepository<Servicio> servicioRepository,
            IClienteRepository clienteRepository)
        {
            _ordenTrabajoCreate = ordenTrabajoCreate;
            _ordenTrabajoAnular = ordenTrabajoAnular;
            _productoRepository = productoRepository;
            _servicioRepository = servicioRepository;
            _clienteRepository = clienteRepository;
        }

        public IList<OrdenTrabajoListDto> OrdenesTrabajo { get; set; } = new List<OrdenTrabajoListDto>();

        [BindProperty]
        public OrdenTrabajoFormDto FormDto { get; set; } = new();

        public List<SelectListItem> EstadoTrabajoOptions { get; private set; } = new();
        public List<SelectListItem> EstadoPagoOptions { get; private set; } = new();

        public async Task OnGetAsync()
        {
            CargarOpcionesEstado();
            OrdenesTrabajo = (await _ordenTrabajoCreate.GetAllAsync()).ToList();
        }

        public async Task<JsonResult> OnGetOrdenAsync(int id)
        {
            var ordenResult = await _ordenTrabajoCreate.GetDetalleAsync(id);
            if (ordenResult.IsFailure)
            {
                if (ordenResult.ErrorCode == ErrorCodes.OrdenTrabajoNotFound)
                    return new JsonResult(new { error = "Orden no encontrada." }) { StatusCode = 404 };

                return new JsonResult(new { error = ordenResult.ErrorMessage ?? "Error al consultar orden." }) { StatusCode = 500 };
            }

            return new JsonResult(ordenResult.Value!);
        }

        public async Task<JsonResult> OnGetBuscarVehiculosAsync(string term, int? clienteId)
        {
            var vehiculos = await _ordenTrabajoCreate.BuscarVehiculosAsync(term, clienteId);
            return new JsonResult(vehiculos);
        }

        public async Task<JsonResult> OnGetBuscarClientesAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(new List<object>());

            term = term.ToLower(CultureInfo.InvariantCulture);

            var clientes = (await _clienteRepository.GetAllAsync())
                .Where(c => !c.IsDeleted &&
                    ((c.NombreCompleto?.ToString() ?? string.Empty)
                        .ToLower(CultureInfo.InvariantCulture)
                        .Contains(term, StringComparison.OrdinalIgnoreCase)
                     || c.Ci.Numero.ToString().Contains(term)))
                .OrderBy(c => c.NombreCompleto?.ToString())
                .Select(c => new { id = c.ClienteId, text = (c.NombreCompleto?.ToString() ?? "Sin nombre") + " - CI: " + c.Ci.Numero })
                .Take(15)
                .ToList();

            return new JsonResult(clientes);
        }

        public async Task<JsonResult> OnGetBuscarProductosAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(new List<object>());

            term = term.ToLower(CultureInfo.InvariantCulture);
            var productos = (await _productoRepository.GetAllAsync())
                .Where(p => p.Nombre.ToLower(CultureInfo.InvariantCulture).Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(p => new
                {
                    id = p.ProductoId,
                    text = p.Nombre,
                    precio = p.Precio,
                    stock = p.Stock
                })
                .Take(15)
                .ToList();

            return new JsonResult(productos);
        }

        public async Task<JsonResult> OnGetBuscarServiciosAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(new List<object>());

            term = term.ToLower(CultureInfo.InvariantCulture);
            var servicios = (await _servicioRepository.GetAllAsync())
                .Where(s => s.Nombre.ToLower(CultureInfo.InvariantCulture).Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(s => new
                {
                    id = s.ServicioId,
                    text = s.Nombre,
                    precio = s.Precio
                })
                .Take(15)
                .ToList();

            return new JsonResult(servicios);
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            CargarOpcionesEstado();

            if (!ModelState.IsValid)
            {
                OrdenesTrabajo = (await _ordenTrabajoCreate.GetAllAsync()).ToList();
                return Page();
            }

            var saveResult = await _ordenTrabajoCreate.SaveAsync(FormDto);
            if (saveResult.IsFailure)
            {
                ModelState.AddModelError(string.Empty, saveResult.ErrorMessage ?? "No se pudo registrar la orden de trabajo.");
                OrdenesTrabajo = (await _ordenTrabajoCreate.GetAllAsync()).ToList();
                return Page();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var deleteResult = await _ordenTrabajoAnular.DeleteAsync(id);
            if (deleteResult.IsFailure)
            {
                TempData["ErrorMessage"] = deleteResult.ErrorMessage;
            }

            return RedirectToPage();
        }

        private void CargarOpcionesEstado()
        {
            EstadoTrabajoOptions = _ordenTrabajoCreate.GetEstadoTrabajoOptions()
                .Select(estado => new SelectListItem(estado, estado))
                .ToList();

            EstadoPagoOptions = _ordenTrabajoCreate.GetEstadoPagoOptions()
                .Select(estado => new SelectListItem(estado, estado))
                .ToList();
        }
    }
}
