using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdenTrabajoService.Application.DTOs.OrdenTrabajo;
using OrdenTrabajoService.Application.Facades;

namespace OrdenTrabajoService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdenesTrabajoController : ControllerBase
    {
        private readonly OrdenTrabajoCreate _facade;
        private readonly OrdenTrabajoAnular _anularFacade;

        public OrdenesTrabajoController(OrdenTrabajoCreate facade, OrdenTrabajoAnular anularFacade)
        {
            _facade = facade;
            _anularFacade = anularFacade;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _facade.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _facade.GetDetalleAsync(id);
            if (result.IsFailure)
                return NotFound(new { error = result.ErrorMessage });
            return Ok(result.Value);
        }

        [HttpGet("vehiculos/buscar")]
        public async Task<IActionResult> BuscarVehiculos([FromQuery] string? term, [FromQuery] int? clienteId)
        {
            var result = await _facade.BuscarVehiculosAsync(term, clienteId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrdenTrabajoDto dto)
        {
            var result = await _facade.RegistrarAsync(dto);
            if (result.IsFailure)
                return BadRequest(new { error = result.ErrorMessage });
            return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOrdenTrabajoDto dto)
        {
            dto.OrdenTrabajoId = id;
            var result = await _facade.ActualizarAsync(dto);
            if (result.IsFailure)
                return BadRequest(new { error = result.ErrorMessage });
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _anularFacade.DeleteAsync(id);
            if (result.IsFailure)
                return BadRequest(new { error = result.ErrorMessage });
            return NoContent();
        }

        [HttpPut("{id:int}/restaurar")]
        public async Task<IActionResult> Restaurar(int id)
        {
            var result = await _anularFacade.AnularProcesoPrincipalAsync(id, anular: false);
            if (result.IsFailure)
                return BadRequest(new { error = result.ErrorMessage });
            return NoContent();
        }

        [HttpGet("mecanico/{mecanicoId}")]
        public async Task<IActionResult> GetByMecanicoId(int mecanicoId)
        {
            var result = await _facade.GetAllAsync();
            
            // Filtrar órdenes donde el mecánico está asignado
            var ordenesDelMecanico = result
                .Where(o => o.MecanicosAsignados.Contains(mecanicoId))
                .Select(o => new
                {
                    o.OrdenTrabajoId,
                    o.VehiculoId,
                    o.VehiculoPlaca,
                    o.FechaIngreso,
                    o.FechaEntrega,
                    o.EstadoTrabajo,
                    o.EstadoPago,
                    o.Total
                })
                .ToList();
            
            return Ok(ordenesDelMecanico);
        }
    }
}

