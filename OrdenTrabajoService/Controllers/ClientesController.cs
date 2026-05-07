using Microsoft.AspNetCore.Mvc;
using Taller_Mecanico_Arqui.Application.DTOs.Clientes;
using Taller_Mecanico_Arqui.Application.UseCases.Clientes;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Application.DTOs.OrdenTrabajo;

namespace OrdenTrabajoService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly CreateClienteUseCase _createUseCase;
        private readonly UpdateClienteUseCase _updateUseCase;
        private readonly GetClienteByIdUseCase _getByIdUseCase;
        private readonly GetAllClientesUseCase _getAllUseCase;
        private readonly DeleteClienteUseCase _deleteUseCase;

        public ClientesController(
            CreateClienteUseCase createUseCase,
            UpdateClienteUseCase updateUseCase,
            GetClienteByIdUseCase getByIdUseCase,
            GetAllClientesUseCase getAllUseCase,
            DeleteClienteUseCase deleteUseCase)
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
            var clientes = await _getAllUseCase.ExecuteAsync();
            var dtos = clientes.Select(ToDto);
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _getByIdUseCase.ExecuteAsync(id);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            if (result.Value == null)
                return NotFound(new { message = "Cliente no encontrado." });

            return Ok(ToDetalleDto(result.Value));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateClienteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _createUseCase.ExecuteAsync(dto);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            return CreatedAtAction(nameof(GetById), new { id = result.Value.ClienteId }, new { result.Value.ClienteId });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateClienteDto dto)
        {
            if (id != dto.ClienteId)
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

        private static ClienteListDto ToDto(Cliente c) => new()
        {
            ClienteId = c.ClienteId,
            NombreCompleto = c.NombreCompleto.ToString(),
            Ci = c.Ci.ToString(),
            Telefono = c.Telefono,
            Email = c.Email,
            TipoCliente = c.TipoCliente.ToString(),
            VehiculoCount = c.Vehiculos.Count
        };

        private static ClienteDetalleDto ToDetalleDto(Cliente c) => new()
        {
            ClienteId = c.ClienteId,
            Nombres = c.NombreCompleto.Nombres,
            PrimerApellido = c.NombreCompleto.PrimerApellido,
            SegundoApellido = c.NombreCompleto.SegundoApellido,
            Ci = c.Ci.ToString(),
            Telefono = c.Telefono,
            Email = c.Email,
            TipoCliente = c.TipoCliente.ToString(),
            FechaRegistro = c.FechaRegistro,
            UsuarioLoginId = c.UsuarioLoginId,
            Vehiculos = c.Vehiculos.Select(v => new VehiculoLookupDto
            {
                VehiculoId = v.VehiculoId,
                Placa = v.Placa
            }).ToList()
        };
    }
}
