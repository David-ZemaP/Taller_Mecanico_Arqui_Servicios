using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Interfaces;

namespace OrdenTrabajoService.Controllers
{
    [ApiController]
    [Route("api/clientes")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly IRepository<Cliente> _repo;

        public ClientesController(IRepository<Cliente> repo) => _repo = repo;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? term)
        {
            var all = await _repo.GetAllAsync();
            IEnumerable<Cliente> q = all;
            if (!string.IsNullOrWhiteSpace(term))
            {
                var t = term.ToLower();
                q = q.Where(c =>
                    c.Nombre.ToLower().Contains(t) ||
                    c.PrimerApellido.ToLower().Contains(t) ||
                    c.Ci.ToString().Contains(t));
            }
            return Ok(q.Select(ToDto));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var r = await _repo.GetByIdAsync(id);
            if (r.IsFailure) return BadRequest(new { error = r.ErrorMessage });
            if (r.Value is null) return NotFound();
            return Ok(ToDto(r.Value));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaveClienteDto dto)
        {
            var entity = Cliente.Crear(dto.Nombres, dto.PrimerApellido, dto.SegundoApellido,
                dto.CiNumero, dto.CiComplemento, dto.Telefono, dto.Email ?? string.Empty);
            var r = await _repo.AddAsync(entity);
            if (r.IsFailure) return BadRequest(new { error = r.ErrorMessage });
            return CreatedAtAction(nameof(GetById), new { id = r.Value }, new
            {
                clienteId = r.Value,
                nombres = entity.Nombre,
                primerApellido = entity.PrimerApellido,
                segundoApellido = entity.SegundoApellido,
                ciNumero = entity.Ci,
                ciComplemento = entity.CiComplemento,
                telefono = entity.Telefono,
                email = entity.Email,
                fechaRegistro = DateTime.UtcNow.ToString("dd/MM/yyyy")
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] SaveClienteDto dto)
        {
            var r = await _repo.GetByIdAsync(id);
            if (r.IsFailure) return BadRequest(new { error = r.ErrorMessage });
            if (r.Value is null) return NotFound();
            r.Value.Actualizar(dto.Nombres, dto.PrimerApellido, dto.SegundoApellido,
                dto.CiNumero, dto.CiComplemento, dto.Telefono, dto.Email ?? string.Empty);
            var upd = await _repo.UpdateAsync(r.Value);
            if (upd.IsFailure) return BadRequest(new { error = upd.ErrorMessage });
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAsync(id);
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
            email = c.Email,
            fechaRegistro = c.FechaRegistro.ToString("dd/MM/yyyy")
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
