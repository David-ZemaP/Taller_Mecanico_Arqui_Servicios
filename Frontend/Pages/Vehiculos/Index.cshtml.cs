using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Taller_Mecanico_Arqui.Frontend.Adapters;
using Taller_Mecanico_Arqui.Frontend.Authorization;
using System.Linq;

namespace Taller_Mecanico_Arqui.Pages.Vehiculo
{
    [RequireAccessLevel(NivelAcceso.Parcial, NivelAcceso.Parcial, NivelAcceso.Completo)]
    public class IndexModel : PageModel
    {
        private readonly IVehiculoAdapter _vehiculoAdapter;
        private readonly IClienteAdapter _clienteAdapter;
        private readonly ICatalogosVehiculosAdapter _catalogosVehiculosAdapter;

        public IndexModel(
            IVehiculoAdapter vehiculoAdapter,
            IClienteAdapter clienteAdapter,
            ICatalogosVehiculosAdapter catalogosVehiculosAdapter)
        {
            _vehiculoAdapter = vehiculoAdapter;
            _clienteAdapter = clienteAdapter;
            _catalogosVehiculosAdapter = catalogosVehiculosAdapter;
        }

        public List<VehiculoListDto> Vehiculos { get; set; } = new();
        public List<CatalogoMarcaDto> Marcas { get; set; } = new();
        public List<CatalogoModeloDto> Modelos { get; set; } = new();
        public List<CatalogoColorDto> Colores { get; set; } = new();
        public bool CanModify => User.IsInRole("Completo");
        public bool IsCliente => User.IsInRole("Cliente");

        [BindProperty]
        public VehiculoFormDto FormDto { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadPageDataAsync();
        }

        public async Task<JsonResult> OnGetVehiculoAsync(int id)
        {
            var vehiculo = await _vehiculoAdapter.GetByIdAsync(id);
            if (vehiculo == null)
            {
                return new JsonResult(new { error = "Vehículo no encontrado." }) { StatusCode = 404 };
            }

            return new JsonResult(new
            {
                vehiculoId = vehiculo.VehiculoId,
                clienteId = vehiculo.ClienteId,
                placa = vehiculo.Placa,
                marcaId = vehiculo.MarcaId,
                modeloId = vehiculo.ModeloId,
                colorVehiculoId = vehiculo.ColorVehiculoId,
                anio = vehiculo.Anio,
                clienteNombre = vehiculo.ClienteNombre,
                marca = vehiculo.Marca,
                modelo = vehiculo.Modelo,
                color = vehiculo.Color
            });
        }

        public async Task<JsonResult> OnGetBuscarClientesAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(new List<object>());

            term = term.ToLower(CultureInfo.InvariantCulture);
            var clientes = (await _clienteAdapter.GetAllAsync())
                .Where(c => c.NombreCompleto.ToLower(CultureInfo.InvariantCulture).Contains(term, StringComparison.OrdinalIgnoreCase) ||
                            c.Ci.ToLower(CultureInfo.InvariantCulture).Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(c => new { id = c.ClienteId, text = $"{c.Ci} - {c.NombreCompleto}" })
                .Take(15)
                .ToList();

            return new JsonResult(clientes);
        }

        public async Task<IActionResult> OnPostSaveAjaxAsync([FromBody] VehiculoFormDto dto)
        {
            if (dto == null)
            {
                return new JsonResult(new { success = false, message = "Datos inválidos" }) { StatusCode = 400 };
            }

            if (dto.VehiculoId == 0)
            {
                var result = await _vehiculoAdapter.CreateAsync(new CreateVehiculoDto
                {
                    ClienteId = dto.ClienteId,
                    Placa = dto.Placa,
                    MarcaId = dto.MarcaId,
                    ModeloId = dto.ModeloId,
                    ColorVehiculoId = dto.ColorVehiculoId,
                    Anio = dto.Anio
                });

                if (!result.Success)
                {
                    return new JsonResult(new { success = false, message = result.Error ?? "Error al guardar el vehículo" }) { StatusCode = 500 };
                }

                return new JsonResult(new { success = true, vehiculo = new { vehiculoId = result.VehiculoId ?? 0, placa = dto.Placa } });
            }
            else
            {
                var result = await _vehiculoAdapter.UpdateAsync(new UpdateVehiculoDto
                {
                    VehiculoId = dto.VehiculoId,
                    ClienteId = dto.ClienteId,
                    Placa = dto.Placa,
                    MarcaId = dto.MarcaId,
                    ModeloId = dto.ModeloId,
                    ColorVehiculoId = dto.ColorVehiculoId,
                    Anio = dto.Anio
                });

                if (!result.Success)
                {
                    return new JsonResult(new { success = false, message = result.Error ?? "Error al actualizar el vehículo" }) { StatusCode = 500 };
                }

                return new JsonResult(new { success = true });
            }
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!CanModify)
            {
                TempData["ErrorMessage"] = "No tienes permisos para crear o modificar vehículos.";
                return RedirectToPage();
            }

            if (!ModelState.IsValid)
            {
                await LoadPageDataAsync();
                return Page();
            }

            if (FormDto.VehiculoId == 0)
            {
                var result = await _vehiculoAdapter.CreateAsync(new CreateVehiculoDto
                {
                    ClienteId = FormDto.ClienteId,
                    Placa = FormDto.Placa,
                    MarcaId = FormDto.MarcaId,
                    ModeloId = FormDto.ModeloId,
                    ColorVehiculoId = FormDto.ColorVehiculoId,
                    Anio = FormDto.Anio
                });

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, result.Error ?? "No se pudo registrar el vehículo.");
                    await LoadPageDataAsync();
                    return Page();
                }
            }
            else
            {
                var result = await _vehiculoAdapter.UpdateAsync(new UpdateVehiculoDto
                {
                    VehiculoId = FormDto.VehiculoId,
                    ClienteId = FormDto.ClienteId,
                    Placa = FormDto.Placa,
                    MarcaId = FormDto.MarcaId,
                    ModeloId = FormDto.ModeloId,
                    ColorVehiculoId = FormDto.ColorVehiculoId,
                    Anio = FormDto.Anio
                });

                if (!result.Success)
                {
                    await LoadPageDataAsync();
                    ModelState.AddModelError(string.Empty, result.Error ?? "No se pudo actualizar el vehículo.");
                    return Page();
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            if (!CanModify)
            {
                TempData["ErrorMessage"] = "No tienes permisos para eliminar vehículos.";
                return RedirectToPage();
            }

            await _vehiculoAdapter.DeleteAsync(id);
            return RedirectToPage();
        }

        private async Task LoadPageDataAsync()
        {
            Marcas = await _catalogosVehiculosAdapter.GetMarcasAsync();
            Modelos = await _catalogosVehiculosAdapter.GetModelosAsync();
            Colores = await _catalogosVehiculosAdapter.GetColoresAsync();

            if (IsCliente)
            {
                var clienteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(clienteIdClaim) && int.TryParse(clienteIdClaim, out var clienteId))
                {
                    Vehiculos = await _vehiculoAdapter.GetByClienteIdAsync(clienteId);
                }
                else
                {
                    Vehiculos = new List<VehiculoListDto>();
                }
            }
            else
            {
                Vehiculos = await _vehiculoAdapter.GetAllAsync();
            }
        }
    }

    public class VehiculoFormDto
    {
        public int VehiculoId { get; set; }
        public int ClienteId { get; set; }
        public string Placa { get; set; } = string.Empty;
        public int MarcaId { get; set; }
        public int ModeloId { get; set; }
        public int ColorVehiculoId { get; set; }
        public int Anio { get; set; }
    }
}
