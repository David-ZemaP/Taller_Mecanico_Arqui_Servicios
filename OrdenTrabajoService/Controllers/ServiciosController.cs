using Microsoft.AspNetCore.Mvc;
using Taller_Mecanico_Arqui.Application.DTOs.Servicios;
using Taller_Mecanico_Arqui.Application.UseCases.Servicios;
using Taller_Mecanico_Arqui.Domain.Entities;

namespace OrdenTrabajoService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiciosController : ControllerBase
    {
        private readonly CreateServicioUseCase _createUseCase;
        private readonly UpdateServicioUseCase _updateUseCase;
        private readonly GetServicioByIdUseCase _getByIdUseCase;
        private readonly GetAllServiciosUseCase _getAllUseCase;
        private readonly DeleteServicioUseCase _deleteUseCase;

        public ServiciosController(
            CreateServicioUseCase createUseCase,
            UpdateServicioUseCase updateUseCase,
            GetServicioByIdUseCase getByIdUseCase,
            GetAllServiciosUseCase getAllUseCase,
            DeleteServicioUseCase deleteUseCase)
        {
            _createUseCase = createUseCase;
            _updateUseCase = updateUseCase;
            _getByIdUseCase = getByIdUseCase;
            _getAllUseCase = getAllUseCase;
            _deleteUseCase = deleteUseCase;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var servicios = await _getAllUseCase.ExecuteAsync();
            var dtos = servicios.Select(ToDto);
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _getByIdUseCase.ExecuteAsync(id);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            if (result.Value == null)
                return NotFound(new { message = "Servicio no encontrado." });

            return Ok(ToDto(result.Value));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServicioDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _createUseCase.ExecuteAsync(dto);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { servicioId = result.Value });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServicioDto dto)
        {
            if (id != dto.ServicioId)
                return BadRequest(new { message = "El ID de la ruta no coincide con el del cuerpo." });

            var result = await _updateUseCase.ExecuteAsync(dto);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _deleteUseCase.ExecuteAsync(id);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            return NoContent();
        }

        private static ServicioListDto ToDto(Servicio s) => new()
        {
            ServicioId = s.ServicioId,
            Nombre = s.Nombre,
            Precio = s.Precio
        };
    }
}
