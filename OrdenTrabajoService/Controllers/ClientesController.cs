using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Interfaces;

namespace OrdenTrabajoService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly IRepository<Cliente> _repository;

        public ClientesController(IRepository<Cliente> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? term)
        {
            var clientes = await _repository.GetAllAsync();
            IEnumerable<Cliente> query = clientes;

            if (!string.IsNullOrWhiteSpace(term))
            {
                var t = term.ToLowerInvariant();
                query = query.Where(c =>
                    c.Nombre.ToLowerInvariant().Contains(t, StringComparison.OrdinalIgnoreCase) ||
                    c.PrimerApellido.ToLowerInvariant().Contains(t, StringComparison.OrdinalIgnoreCase) ||
                    c.Ci.ToString().Contains(t));
            }

            return Ok(query.Select(c => ToDto(c)));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (result.IsFailure) return BadRequest(new { error = result.ErrorMessage });
            if (result.Value is null) return NotFound();
            return Ok(ToDto(result.Value));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveClienteDto dto)
        {
            var entity = Cliente.Crear(dto.Nombres, dto.PrimerApellido, dto.SegundoApellido, dto.CiNumero, dto.CiComplemento, dto.Telefono, dto.Email ?? string.Empty);
            var result = await _repository.AddAsync(entity);
            if (result.IsFailure) return BadRequest(new { error = result.ErrorMessage });

            return CreatedAtAction(nameof(GetById), new { id = result.Value }, new
            {
                clienteId = result.Value,
                nombres = entity.Nombre,
                primerApellido = entity.PrimerApellido,
                segundoApellido = entity.SegundoApellido,
                ciNumero = entity.Ci,
                ciComplemento = entity.CiComplemento,
                telefono = entity.Telefono,
                email = entity.Email
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] SaveClienteDto dto)
        {
            var getResult = await _repository.GetByIdAsync(id);
            if (getResult.IsFailure) return BadRequest(new { error = getResult.ErrorMessage });
            if (getResult.Value is null) return NotFound();

            getResult.Value.Actualizar(dto.Nombres, dto.PrimerApellido, dto.SegundoApellido, dto.CiNumero, dto.CiComplemento, dto.Telefono, dto.Email ?? string.Empty);
            var result = await _repository.UpdateAsync(getResult.Value);
            if (result.IsFailure) return BadRequest(new { error = result.ErrorMessage });
            return NoContent();
        }

        private static object ToDto(Cliente c) => new
        {
            clienteId = c.ClienteId,
            nombres = c.Nombre,
            primerApellido = c.PrimerApellido,
            segundoApellido = c.SegundoApellido,
            ciNumero = c.Ci,
            ciComplemento = c.CiComplemento,
            telefono = c.Telefono,
            email = c.Email
        };
    }

    public class SaveClienteDto
    {
        public string Nombres { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public int CiNumero { get; set; }
        public string? CiComplemento { get; set; }
        public int Telefono { get; set; }
        public string? Email { get; set; }
    }
}
