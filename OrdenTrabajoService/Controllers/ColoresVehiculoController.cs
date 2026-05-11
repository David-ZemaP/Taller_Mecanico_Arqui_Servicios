using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdenTrabajoService.Application.DTOs.Catalogo;
using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Interfaces;

namespace OrdenTrabajoService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ColoresVehiculoController : ControllerBase
    {
        private readonly IRepository<ColorVehiculo> _repository;

        public ColoresVehiculoController(IRepository<ColorVehiculo> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var colores = await _repository.GetAllAsync();
            var dtos = colores.Select(c => new ColorVehiculoDto { ColorVehiculoId = c.ColorVehiculoId, Nombre = c.Nombre });
            return Ok(dtos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (result.IsFailure) return BadRequest(new { error = result.ErrorMessage });
            if (result.Value is null) return NotFound();
            return Ok(new ColorVehiculoDto { ColorVehiculoId = result.Value.ColorVehiculoId, Nombre = result.Value.Nombre });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateColorVehiculoDto dto)
        {
            ColorVehiculo entity;
            try { entity = ColorVehiculo.Crear(dto.Nombre); }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }

            var result = await _repository.AddAsync(entity);
            if (result.IsFailure) return BadRequest(new { error = result.ErrorMessage });
            return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateColorVehiculoDto dto)
        {
            var getResult = await _repository.GetByIdAsync(id);
            if (getResult.IsFailure) return BadRequest(new { error = getResult.ErrorMessage });
            if (getResult.Value is null) return NotFound();

            try { getResult.Value.ActualizarNombre(dto.Nombre); }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }

            var result = await _repository.UpdateAsync(getResult.Value);
            if (result.IsFailure) return BadRequest(new { error = result.ErrorMessage });
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}

