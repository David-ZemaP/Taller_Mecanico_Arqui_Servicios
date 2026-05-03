using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taller_Mecanico_Arqui.Application.UseCases.Vehiculos;
using Taller_Mecanico_Arqui.Application.DTOs.Vehiculos;
using Taller_Mecanico_Arqui.Application.Common;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Infrastructure.Authorization;
using Taller_Mecanico_Arqui.Domain.Enums;

namespace Taller_Mecanico_Arqui.Pages.Vehiculo
{
    [RequireAccessLevel(NivelAcceso.Parcial)]
    public class IndexModel : PageModel
    {
        private readonly GetAllVehiculosUseCase _getAllUseCase;
        private readonly GetVehiculoByIdUseCase _getByIdUseCase;
        private readonly CreateVehiculoUseCase _createUseCase;
        private readonly UpdateVehiculoUseCase _updateUseCase;
        private readonly DeleteVehiculoUseCase _deleteUseCase;
        private readonly IVehiculoRepository _vehiculoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IRepository<Marca> _marcaRepository;
        private readonly IRepository<Modelo> _modeloRepository;
        private readonly IRepository<ColorVehiculo> _colorRepository;
        private readonly Taller_Mecanico_Arqui.Infrastructure.Services.AuthenticationHelper _authHelper;

        public IndexModel(
            GetAllVehiculosUseCase getAllUseCase,
            GetVehiculoByIdUseCase getByIdUseCase,
            CreateVehiculoUseCase createUseCase,
            UpdateVehiculoUseCase updateUseCase,
            DeleteVehiculoUseCase deleteUseCase,
            IVehiculoRepository vehiculoRepository,
            IClienteRepository clienteRepository,
            IRepository<Marca> marcaRepository,
            IRepository<Modelo> modeloRepository,
            IRepository<ColorVehiculo> colorRepository,
            Taller_Mecanico_Arqui.Infrastructure.Services.AuthenticationHelper authHelper)
        {
            _getAllUseCase = getAllUseCase;
            _getByIdUseCase = getByIdUseCase;
            _createUseCase = createUseCase;
            _updateUseCase = updateUseCase;
            _deleteUseCase = deleteUseCase;
            _vehiculoRepository = vehiculoRepository;
            _clienteRepository = clienteRepository;
            _marcaRepository = marcaRepository;
            _modeloRepository = modeloRepository;
            _colorRepository = colorRepository;
            _authHelper = authHelper;
        }

        public IList<Domain.Entities.Vehiculo> Vehiculos { get; set; } = new List<Domain.Entities.Vehiculo>();
        public bool CanModify => _authHelper.GetCurrentUserAccessLevel() == NivelAcceso.Completo;

        [BindProperty]
        public VehiculoFormDto FormDto { get; set; } = new();

        public List<SelectListItem> MarcasSelect { get; set; } = new();
        public List<SelectListItem> ModelosSelect { get; set; } = new();
        public List<SelectListItem> ColoresSelect { get; set; } = new();

        public async Task OnGetAsync()
        {
            await CargarCatalogosAsync();
            var result = await _getAllUseCase.ExecuteAsync();
            Vehiculos = result.ToList();
        }

        public async Task<JsonResult> OnGetModelosPorMarcaAsync(int marcaId)
        {
            var modelos = (await _modeloRepository.GetAllAsync())
                .Where(m => m.MarcaId == marcaId)
                .OrderBy(m => m.Nombre)
                .Select(m => new { id = m.ModeloId, nombre = m.Nombre })
                .ToList();

            return new JsonResult(modelos);
        }

        public async Task<JsonResult> OnGetVehiculoAsync(int id)
        {
            var vehiculoResult = await _getByIdUseCase.ExecuteAsync(id);
            if (vehiculoResult.IsFailure)
            {
                if (vehiculoResult.ErrorCode == ErrorCodes.VehiculoNotFound)
                    return new JsonResult(new { error = "Vehículo no encontrado." }) { StatusCode = 404 };

                return new JsonResult(new { error = vehiculoResult.ErrorMessage ?? "Error al consultar vehículo." }) { StatusCode = 500 };
            }

            var vehiculo = vehiculoResult.Value!;

            return new JsonResult(new
            {
                vehiculoId = vehiculo.VehiculoId,
                clienteId = vehiculo.ClienteId,
                placa = vehiculo.Placa,
                marcaId = vehiculo.MarcaId,
                marcaNombre = vehiculo.Marca?.Nombre ?? "No disponible",
                modeloId = vehiculo.ModeloId,
                modeloNombre = vehiculo.Modelo?.Nombre ?? "No disponible",
                colorVehiculoId = vehiculo.ColorVehiculoId,
                colorNombre = vehiculo.ColorVehiculo?.Nombre ?? "No disponible",
                anio = vehiculo.Anio,
                clienteNombre = vehiculo.Cliente?.NombreCompleto?.ToString() ?? "No disponible"
            });
        }

        public async Task<JsonResult> OnGetBuscarClientesAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(new List<object>());

            term = term.ToLower(CultureInfo.InvariantCulture);
            var clientes = (await _clienteRepository.GetAllAsync())
                .Where(c => c.NombreCompleto.Nombres.ToLower(CultureInfo.InvariantCulture).Contains(term, StringComparison.OrdinalIgnoreCase) ||
                            c.NombreCompleto.PrimerApellido.ToLower(CultureInfo.InvariantCulture).Contains(term, StringComparison.OrdinalIgnoreCase) ||
                            c.Ci.Numero.ToString(CultureInfo.InvariantCulture).Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(c => new { id = c.ClienteId, text = $"{c.Ci.Numero} - {c.NombreCompleto.Nombres} {c.NombreCompleto.PrimerApellido}" })
                .Take(15)
                .ToList();

            return new JsonResult(clientes);
        }

        public async Task<JsonResult> OnGetBuscarMarcasAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(new List<object>());

            term = term.ToLower(CultureInfo.InvariantCulture);
            var marcas = (await _marcaRepository.GetAllAsync())
                .Where(m => m.Nombre.ToLower(CultureInfo.InvariantCulture).Contains(term, StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.Nombre)
                .Select(m => new { id = m.MarcaId, text = m.Nombre })
                .Take(15)
                .ToList();

            return new JsonResult(marcas);
        }

        public async Task<IActionResult> OnPostSaveAjaxAsync([FromBody] VehiculoFormDto dto)
        {
            if (dto == null)
            {
                return new JsonResult(new { success = false, message = "Datos inválidos" }) { StatusCode = 400 };
            }

            Result saveResult;
            if (dto.VehiculoId == 0)
            {
                saveResult = await _createUseCase.ExecuteAsync(new CreateVehiculoDto
                {
                    ClienteId = dto.ClienteId,
                    Placa = dto.Placa,
                    MarcaId = dto.MarcaId,
                    ModeloId = dto.ModeloId,
                    ColorVehiculoId = dto.ColorVehiculoId,
                    Anio = dto.Anio
                });
            }
            else
            {
                saveResult = await _updateUseCase.ExecuteAsync(new UpdateVehiculoDto
                {
                    VehiculoId = dto.VehiculoId,
                    ClienteId = dto.ClienteId,
                    Placa = dto.Placa,
                    MarcaId = dto.MarcaId,
                    ModeloId = dto.ModeloId,
                    ColorVehiculoId = dto.ColorVehiculoId,
                    Anio = dto.Anio
                });
            }

            if (saveResult.IsFailure)
            {
                return new JsonResult(new { success = false, message = saveResult.ErrorMessage ?? "Error al guardar el vehículo" }) { StatusCode = 500 };
            }

            var savedVehiculo = await _vehiculoRepository.GetAllAsync();
            var vehiculoCreado = savedVehiculo
                .Where(v => !v.IsDeleted && v.Placa == dto.Placa && v.ClienteId == dto.ClienteId)
                .OrderByDescending(v => v.VehiculoId)
                .FirstOrDefault();

            return new JsonResult(new { success = true, vehiculo = new { vehiculoId = vehiculoCreado?.VehiculoId ?? 0, placa = dto.Placa } });
        }

        public async Task<JsonResult> OnGetBuscarModelosAsync(string term, int? marcaId)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(new List<object>());

            term = term.ToLower(CultureInfo.InvariantCulture);
            var modelos = (await _modeloRepository.GetAllAsync())
                .Where(m => (!marcaId.HasValue || m.MarcaId == marcaId.Value) &&
                            m.Nombre.ToLower(CultureInfo.InvariantCulture).Contains(term, StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.Nombre)
                .Select(m => new { id = m.ModeloId, text = m.Nombre })
                .Take(15)
                .ToList();

            return new JsonResult(modelos);
        }

        public async Task<JsonResult> OnGetBuscarColoresAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return new JsonResult(new List<object>());

            term = term.ToLower(CultureInfo.InvariantCulture);
            var colores = (await _colorRepository.GetAllAsync())
                .Where(c => c.Nombre.ToLower(CultureInfo.InvariantCulture).Contains(term, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c => c.Nombre)
                .Select(c => new { id = c.ColorVehiculoId, text = c.Nombre })
                .Take(15)
                .ToList();

            return new JsonResult(colores);
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
                await CargarCatalogosAsync();
                return Page();
            }

            var placaNormalizada = ValidationHelper.NormalizePlate(FormDto.Placa);

            if (FormDto.VehiculoId == 0)
            {
                var placaExiste = await _vehiculoRepository.ExistsByPlacaAsync(placaNormalizada);
                var placaValidation = ValidationHelper.ValidatePlateAvailable(placaExiste);
                if (placaValidation.IsFailure)
                {
                    ModelState.AddModelError("FormDto.Placa", placaValidation.ErrorMessage ?? "Esta placa ya está registrada en el sistema.");
                    await CargarCatalogosAsync();
                    return Page();
                }

                var createResult = await _createUseCase.ExecuteAsync(new CreateVehiculoDto
                {
                    ClienteId = FormDto.ClienteId,
                    Placa = placaNormalizada,
                    MarcaId = FormDto.MarcaId,
                    ModeloId = FormDto.ModeloId,
                    ColorVehiculoId = FormDto.ColorVehiculoId,
                    Anio = FormDto.Anio
                });

                if (createResult.IsFailure)
                {
                    ModelState.AddModelError(string.Empty, createResult.ErrorMessage ?? "No se pudo registrar el vehículo.");
                    await CargarCatalogosAsync();
                    return Page();
                }
            }
            else
            {
                var placaDuplicada = await _vehiculoRepository.ExistsByPlacaExceptAsync(placaNormalizada, FormDto.VehiculoId);
                var placaValidation = ValidationHelper.ValidatePlateAvailable(placaDuplicada);
                if (placaValidation.IsFailure)
                {
                    ModelState.AddModelError("FormDto.Placa", placaValidation.ErrorMessage ?? "Esta placa ya está registrada en el sistema.");
                    await CargarCatalogosAsync();
                    return Page();
                }

                var updateResult = await _updateUseCase.ExecuteAsync(new UpdateVehiculoDto
                {
                    VehiculoId = FormDto.VehiculoId,
                    ClienteId = FormDto.ClienteId,
                    Placa = placaNormalizada,
                    MarcaId = FormDto.MarcaId,
                    ModeloId = FormDto.ModeloId,
                    ColorVehiculoId = FormDto.ColorVehiculoId,
                    Anio = FormDto.Anio
                });

                if (updateResult.IsFailure)
                {
                    if (updateResult.ErrorCode == ErrorCodes.VehiculoNotFound)
                    {
                        TempData["ErrorMessage"] = updateResult.ErrorMessage;
                        return RedirectToPage();
                    }

                    ModelState.AddModelError(string.Empty, updateResult.ErrorMessage ?? "No se pudo actualizar el vehículo.");
                    await CargarCatalogosAsync();
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

            await _deleteUseCase.ExecuteAsync(id);
            return RedirectToPage();
        }

        private async Task CargarCatalogosAsync()
        {
            MarcasSelect = (await _marcaRepository.GetAllAsync())
                .OrderBy(m => m.Nombre)
                .Select(m => new SelectListItem(m.Nombre, m.MarcaId.ToString()))
                .ToList();

            ColoresSelect = (await _colorRepository.GetAllAsync())
                .OrderBy(c => c.Nombre)
                .Select(c => new SelectListItem(c.Nombre, c.ColorVehiculoId.ToString()))
                .ToList();

            if (FormDto.MarcaId > 0)
            {
                ModelosSelect = (await _modeloRepository.GetAllAsync())
                    .Where(m => m.MarcaId == FormDto.MarcaId)
                    .OrderBy(m => m.Nombre)
                    .Select(m => new SelectListItem(m.Nombre, m.ModeloId.ToString()))
                    .ToList();
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
