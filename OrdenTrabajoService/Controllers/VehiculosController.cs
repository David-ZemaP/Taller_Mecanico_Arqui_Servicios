using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdenTrabajoService.Application.DTOs.Vehiculo;
using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Interfaces;
using Taller_Mecanico_Users.Domain.Common;

namespace OrdenTrabajoService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VehiculosController : ControllerBase
    {
        private readonly IVehiculoRepository _repository;

        public VehiculosController(IVehiculoRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vehiculos = await _repository.GetAllAsync();
            var dtos = vehiculos
                .Where(v => !v.IsDeleted)
                .Select(v => new VehiculoListDto
                {
                    VehiculoId = v.VehiculoId,
                    Placa = v.Placa,
                    ClienteId = v.ClienteId,
                    ClienteNombre = v.ClienteNombre,
                    Anio = v.Anio,
                    MarcaNombre = v.MarcaNombre,
                    ModeloNombre = v.ModeloNombre,
                    ColorNombre = v.ColorNombre
                });
            return Ok(dtos);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] VehiculoSaveDto dto)
        {
            var validationError = Validate(dto);
            if (validationError is not null)
                return BadRequest(new { error = validationError });

            var entity = Vehiculo.Reconstituir(
                0,
                dto.ClienteId,
                dto.Placa.Trim().ToUpperInvariant(),
                dto.MarcaId,
                dto.ModeloId,
                dto.ColorVehiculoId,
                dto.Anio,
                null,
                false);

            var result = await _repository.AddAsync(entity);
            if (result.IsFailure)
                return BadRequest(new { error = result.ErrorMessage });

            return Ok(new { id = result.Value });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] VehiculoSaveDto dto)
        {
            var validationError = Validate(dto);
            if (validationError is not null)
                return BadRequest(new { error = validationError });

            var entity = Vehiculo.Reconstituir(
                id,
                dto.ClienteId,
                dto.Placa.Trim().ToUpperInvariant(),
                dto.MarcaId,
                dto.ModeloId,
                dto.ColorVehiculoId,
                dto.Anio,
                null,
                false);

            var result = await _repository.UpdateAsync(entity);
            if (result.IsFailure)
            {
                if (result.ErrorCode == ErrorCodes.VehiculoNotFound)
                    return NotFound(new { error = result.ErrorMessage });

                return BadRequest(new { error = result.ErrorMessage });
            }

            return NoContent();
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] string? term, [FromQuery] int? clienteId)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Ok(Array.Empty<VehiculoLookupDto>());

            var vehiculos = await _repository.BuscarPorPlacaAsync(term, clienteId);
            var dtos = vehiculos
                .Select(v => new VehiculoLookupDto { Id = v.VehiculoId, Text = v.Placa })
                .Take(15);
            return Ok(dtos);
        }

        private static string? Validate(VehiculoSaveDto dto)
        {
            if (dto.ClienteId <= 0)
                return "Seleccione un propietario válido.";

            if (string.IsNullOrWhiteSpace(dto.Placa))
                return "La placa es obligatoria.";

            if (dto.MarcaId <= 0)
                return "Seleccione una marca válida.";

            if (dto.ModeloId <= 0)
                return "Seleccione un modelo válido.";

            if (dto.ColorVehiculoId <= 0)
                return "Seleccione un color válido.";

            if (dto.Anio < 1900 || dto.Anio > 2100)
                return "El año debe estar entre 1900 y 2100.";

            return null;
        }
    }

    public class VehiculoSaveDto
    {
        public int ClienteId { get; set; }
        public string Placa { get; set; } = string.Empty;
        public int MarcaId { get; set; }
        public int ModeloId { get; set; }
        public int ColorVehiculoId { get; set; }
        public int Anio { get; set; }
    }
}
