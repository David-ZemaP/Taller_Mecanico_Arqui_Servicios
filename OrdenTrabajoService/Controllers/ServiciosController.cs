using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdenTrabajoService.Application.DTOs.Servicio;
using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Interfaces;

namespace OrdenTrabajoService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServiciosController : ControllerBase
    {
        private readonly IRepository<Servicio> _repository;

        public ServiciosController(IRepository<Servicio> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var servicios = await _repository.GetAllAsync();
            var dtos = servicios.Select(s => new ServicioDto
            {
                ServicioId = s.ServicioId,
                Nombre = s.Nombre,
                Precio = s.Precio
            });
            return Ok(dtos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (result.IsFailure)
                return BadRequest(new { error = result.ErrorMessage });
            if (result.Value is null)
                return NotFound();
            var s = result.Value;
            return Ok(new ServicioDto { ServicioId = s.ServicioId, Nombre = s.Nombre, Precio = s.Precio });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServicioDto dto)
        {
            Servicio entity;
            try { entity = Servicio.Crear(dto.Nombre, dto.Precio); }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }

            var result = await _repository.AddAsync(entity);
            if (result.IsFailure)
                return BadRequest(new { error = result.ErrorMessage });
            return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServicioDto dto)
        {
            var getResult = await _repository.GetByIdAsync(id);
            if (getResult.IsFailure) return BadRequest(new { error = getResult.ErrorMessage });
            if (getResult.Value is null) return NotFound();

            try { getResult.Value.ActualizarDatos(dto.Nombre, dto.Precio); }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }

            var result = await _repository.UpdateAsync(getResult.Value);
            if (result.IsFailure)
                return BadRequest(new { error = result.ErrorMessage });
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

