using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taller_Mecanico_Users.Application.Services;
using Taller_Mecanico_Users.Domain.Ports;
using Taller_Mecanico_Users.UseCases.Empleados;

namespace Taller_Mecanico_Users.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Gerente,Administrador,Empleado")]
    public class EmpleadoController : ControllerBase
    {
        private readonly GetEmpleadosUseCase _getEmpleados;
        private readonly CreateEmpleadoUseCase _createEmpleado;
        private readonly UpdateEmpleadoUseCase _updateEmpleado;
        private readonly DeleteEmpleadoUseCase _deleteEmpleado;
        private readonly IEmpleadoRepository _repository;
        private readonly IAuthenticationHelper _authHelper;

        public EmpleadoController(
            GetEmpleadosUseCase getEmpleados,
            CreateEmpleadoUseCase createEmpleado,
            UpdateEmpleadoUseCase updateEmpleado,
            DeleteEmpleadoUseCase deleteEmpleado,
            IEmpleadoRepository repository,
            IAuthenticationHelper authHelper)
        {
            _getEmpleados = getEmpleados;
            _createEmpleado = createEmpleado;
            _updateEmpleado = updateEmpleado;
            _deleteEmpleado = deleteEmpleado;
            _repository = repository;
            _authHelper = authHelper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var empleados = await _getEmpleados.ExecuteAsync();
            return Ok(empleados.Select(ToDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var empleado = await _repository.GetByIdAsync(id);
            if (empleado is null)
                return NotFound(new { message = "Empleado no encontrado." });
            return Ok(ToDto(empleado));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmpleadoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.PrimerApellido))
                return BadRequest(new { message = "Nombre y primer apellido son obligatorios." });

            if (request.Ci <= 0)
                return BadRequest(new { message = "El CI debe ser un número válido." });

            var actor = _authHelper.GetCurrentAuditActor();
            var data = BuildRecord(request, actor);

            try
            {
                var newId = await _createEmpleado.ExecuteAsync(data);
                return CreatedAtAction(nameof(GetAll), new { id = newId }, new { EmpleadoId = newId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateEmpleadoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Nombre) || string.IsNullOrWhiteSpace(request.PrimerApellido))
                return BadRequest(new { message = "Nombre y primer apellido son obligatorios." });

            var actor = _authHelper.GetCurrentAuditActor();
            var data = BuildRecord(request, actor);

            try
            {
                await _updateEmpleado.ExecuteAsync(id, data);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _deleteEmpleado.ExecuteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private static NuevoEmpleadoRecord BuildRecord(CreateEmpleadoRequest r, string? actor) => new(
            r.Nombre.Trim(),
            r.PrimerApellido.Trim(),
            r.SegundoApellido?.Trim(),
            r.Ci,
            r.CiComplemento?.Trim(),
            r.Telefono,
            r.Email?.Trim(),
            r.FechaContratacion == default ? DateTime.UtcNow : r.FechaContratacion,
            string.IsNullOrWhiteSpace(r.TipoEmpleado) ? "Mecanico" : r.TipoEmpleado,
            string.IsNullOrWhiteSpace(r.EstadoLaboral) ? "Activo" : r.EstadoLaboral,
            r.Especialidad?.Trim(),
            r.SalarioPorHora,
            r.SalarioMensual,
            r.NivelAcceso?.Trim(),
            actor
        );

        private static EmpleadoDto ToDto(EmpleadoRecord e) => new()
        {
            EmpleadoId = e.EmpleadoId,
            Nombre = e.Nombre,
            PrimerApellido = e.PrimerApellido,
            SegundoApellido = e.SegundoApellido,
            Ci = e.Ci,
            CiComplemento = e.CiComplemento,
            Telefono = e.Telefono,
            Email = e.Email,
            FechaContratacion = e.FechaContratacion,
            TipoEmpleado = e.TipoEmpleado,
            EstadoLaboral = e.EstadoLaboral,
            Especialidad = e.Especialidad,
            SalarioPorHora = e.SalarioPorHora,
            SalarioMensual = e.SalarioMensual,
            NivelAcceso = e.NivelAcceso
        };
    }

    public class CreateEmpleadoRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public int Ci { get; set; }
        public string? CiComplemento { get; set; }
        public int Telefono { get; set; }
        public string? Email { get; set; }
        public DateTime FechaContratacion { get; set; }
        public string TipoEmpleado { get; set; } = "Mecanico";
        public string EstadoLaboral { get; set; } = "Activo";
        public string? Especialidad { get; set; }
        public decimal? SalarioPorHora { get; set; }
        public decimal? SalarioMensual { get; set; }
        public string? NivelAcceso { get; set; }
    }

    public class EmpleadoDto
    {
        public int EmpleadoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public int Ci { get; set; }
        public string? CiComplemento { get; set; }
        public int Telefono { get; set; }
        public string? Email { get; set; }
        public DateTime FechaContratacion { get; set; }
        public string TipoEmpleado { get; set; } = string.Empty;
        public string EstadoLaboral { get; set; } = string.Empty;
        public string? Especialidad { get; set; }
        public decimal? SalarioPorHora { get; set; }
        public decimal? SalarioMensual { get; set; }
        public string? NivelAcceso { get; set; }
    }
}
