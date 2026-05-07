using Microsoft.AspNetCore.Mvc;
using Taller_Mecanico_Arqui.Application.DTOs.Empleados;
using Taller_Mecanico_Arqui.Application.UseCases.Empleados;
using Taller_Mecanico_Arqui.Domain.Entities;

namespace OrdenTrabajoService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmpleadosController : ControllerBase
    {
        private readonly CreateEmpleadoUseCase _createUseCase;
        private readonly UpdateEmpleadoUseCase _updateUseCase;
        private readonly GetEmpleadoByIdUseCase _getByIdUseCase;
        private readonly GetAllEmpleadosUseCase _getAllUseCase;
        private readonly DeleteEmpleadoUseCase _deleteUseCase;

        public EmpleadosController(
            CreateEmpleadoUseCase createUseCase,
            UpdateEmpleadoUseCase updateUseCase,
            GetEmpleadoByIdUseCase getByIdUseCase,
            GetAllEmpleadosUseCase getAllUseCase,
            DeleteEmpleadoUseCase deleteUseCase)
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
            var empleados = await _getAllUseCase.ExecuteAsync();
            var dtos = empleados.Select(ToDto);
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _getByIdUseCase.ExecuteAsync(id);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            if (result.Value == null)
                return NotFound(new { message = "Empleado no encontrado." });

            return Ok(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmpleadoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _createUseCase.ExecuteAsync(dto);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            return CreatedAtAction(nameof(GetById), new { id = result.Value.EmpleadoId }, new { result.Value.EmpleadoId });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateEmpleadoDto dto)
        {
            if (id != dto.EmpleadoId)
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

        private static EmpleadoListDto ToDto(Empleado e) => new()
        {
            EmpleadoId = e.EmpleadoId,
            NombreCompleto = e.NombreCompleto.ToString(),
            Ci = e.Ci.ToString(),
            Telefono = e.Telefono,
            Email = e.Email,
            TipoEmpleado = "Empleado",
            EstadoLaboral = e.EstadoLaboral.ToString(),
            FechaContratacion = e.FechaContratacion
        };
    }
}
