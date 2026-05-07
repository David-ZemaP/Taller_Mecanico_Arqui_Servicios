using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebService.Adapters;
using WebService.DTOs;

namespace WebService.Pages.Vehiculos
{
    public class IndexModel : PageModel
    {
        private readonly OrdenTrabajoAdapter _adapter;

        public IndexModel(OrdenTrabajoAdapter adapter)
        {
            _adapter = adapter;
        }

        public IList<VehiculoListDto> Vehiculos { get; set; } = new List<VehiculoListDto>();

        public bool CanModify
        {
            get
            {
                var nivel = User.FindFirst("NivelAcceso")?.Value;
                return nivel == "Completo";
            }
        }

        [BindProperty]
        public VehiculoFormDto FormDto { get; set; } = new();

        public List<SelectListItem> MarcasSelect { get; set; } = new();
        public List<SelectListItem> ModelosSelect { get; set; } = new();
        public List<SelectListItem> ColoresSelect { get; set; } = new();

        public async Task OnGetAsync()
        {
            Vehiculos = await _adapter.GetAllVehiculosAsync();
            await CargarCatalogosAsync();
        }

        public async Task<JsonResult> OnGetVehiculoAsync(int id)
        {
            var vehiculos = await _adapter.GetAllVehiculosAsync();
            var v = vehiculos.FirstOrDefault(x => x.VehiculoId == id);
            if (v is null)
                return new JsonResult(new { error = "Vehículo no encontrado." }) { StatusCode = 404 };

            return new JsonResult(new
            {
                vehiculoId = v.VehiculoId,
                clienteId = v.ClienteId,
                placa = v.Placa,
                marcaNombre = v.MarcaNombre,
                modeloNombre = v.ModeloNombre,
                colorNombre = v.ColorNombre,
                anio = v.Anio,
                clienteNombre = v.ClienteNombre
            });
        }

        public async Task<JsonResult> OnGetModelosPorMarcaAsync(int marcaId)
        {
            var modelos = await _adapter.GetAllModelosAsync();
            var result = modelos
                .Where(m => m.MarcaId == marcaId)
                .OrderBy(m => m.Nombre)
                .Select(m => new { id = m.ModeloId, nombre = m.Nombre })
                .ToList();
            return new JsonResult(result);
        }

        public async Task<JsonResult> OnGetBuscarMarcasAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(Array.Empty<object>());

            var marcas = await _adapter.GetAllMarcasAsync();
            var normalized = term.ToLower(CultureInfo.InvariantCulture);
            var result = marcas
                .Where(m => m.Nombre.ToLower(CultureInfo.InvariantCulture)
                    .Contains(normalized, StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.Nombre)
                .Select(m => new { id = m.MarcaId, text = m.Nombre })
                .Take(15)
                .ToList();
            return new JsonResult(result);
        }

        public async Task<JsonResult> OnGetBuscarModelosAsync(string term, int? marcaId)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(Array.Empty<object>());

            var modelos = await _adapter.GetAllModelosAsync();
            var normalized = term.ToLower(CultureInfo.InvariantCulture);
            var result = modelos
                .Where(m => (!marcaId.HasValue || m.MarcaId == marcaId.Value) &&
                            m.Nombre.ToLower(CultureInfo.InvariantCulture)
                                .Contains(normalized, StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.Nombre)
                .Select(m => new { id = m.ModeloId, text = m.Nombre })
                .Take(15)
                .ToList();
            return new JsonResult(result);
        }

        public async Task<JsonResult> OnGetBuscarColoresAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(Array.Empty<object>());

            var colores = await _adapter.GetAllColoresAsync();
            var normalized = term.ToLower(CultureInfo.InvariantCulture);
            var result = colores
                .Where(c => c.Nombre.ToLower(CultureInfo.InvariantCulture)
                    .Contains(normalized, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c.Nombre)
                .Select(c => new { id = c.ColorVehiculoId, text = c.Nombre })
                .Take(15)
                .ToList();
            return new JsonResult(result);
        }

        public async Task<JsonResult> OnPostSaveAjaxAsync([FromBody] VehiculoFormDto dto)
        {
            if (dto is null)
                return new JsonResult(new { success = false, message = "Datos inválidos." }) { StatusCode = 400 };

            var (ok, error, vehiculoId) = await _adapter.SaveVehiculoAsync(dto);
            if (!ok)
                return new JsonResult(new { success = false, message = error }) { StatusCode = 500 };

            return new JsonResult(new { success = true, vehiculo = new { vehiculoId, placa = dto.Placa } });
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                Vehiculos = await _adapter.GetAllVehiculosAsync();
                await CargarCatalogosAsync();
                return Page();
            }

            var (ok, error, _) = await _adapter.SaveVehiculoAsync(FormDto);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, error ?? "No se pudo guardar el vehículo.");
                Vehiculos = await _adapter.GetAllVehiculosAsync();
                await CargarCatalogosAsync();
                return Page();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _adapter.DeleteVehiculoAsync(id);
            return RedirectToPage();
        }

        private async Task CargarCatalogosAsync()
        {
            var marcas = await _adapter.GetAllMarcasAsync();
            var colores = await _adapter.GetAllColoresAsync();

            MarcasSelect = marcas.OrderBy(m => m.Nombre)
                .Select(m => new SelectListItem(m.Nombre, m.MarcaId.ToString()))
                .ToList();

            ColoresSelect = colores.OrderBy(c => c.Nombre)
                .Select(c => new SelectListItem(c.Nombre, c.ColorVehiculoId.ToString()))
                .ToList();

            if (FormDto.MarcaId > 0)
            {
                var modelos = await _adapter.GetAllModelosAsync();
                ModelosSelect = modelos
                    .Where(m => m.MarcaId == FormDto.MarcaId)
                    .OrderBy(m => m.Nombre)
                    .Select(m => new SelectListItem(m.Nombre, m.ModeloId.ToString()))
                    .ToList();
            }
        }
    }
}
