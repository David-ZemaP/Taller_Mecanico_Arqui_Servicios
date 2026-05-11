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
    public class MarcasController : ControllerBase
    {
        private readonly IRepository<Marca> _repository;

        public MarcasController(IRepository<Marca> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var marcas = await _repository.GetAllAsync();
            var dtos = marcas.Select(m => new MarcaDto { MarcaId = m.MarcaId, Nombre = m.Nombre });
            return Ok(dtos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (result.IsFailure) return BadRequest(new { error = result.ErrorMessage });
            if (result.Value is null) return NotFound();
            return Ok(new MarcaDto { MarcaId = result.Value.MarcaId, Nombre = result.Value.Nombre });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateMarcaDto dto)
        {
            Marca entity;
            try { entity = Marca.Crear(dto.Nombre); }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }

            var result = await _repository.AddAsync(entity);
            if (result.IsFailure) return BadRequest(new { error = result.ErrorMessage });
            return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateMarcaDto dto)
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
