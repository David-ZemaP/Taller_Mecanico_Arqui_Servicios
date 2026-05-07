using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdenTrabajoService.Application.DTOs.Vehiculo;
using OrdenTrabajoService.Domain.Interfaces;

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
    }
}
