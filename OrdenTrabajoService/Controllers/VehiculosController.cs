using Microsoft.AspNetCore.Mvc;
using Taller_Mecanico_Arqui.Application.DTOs.Vehiculos;
using Taller_Mecanico_Arqui.Application.UseCases.Vehiculos;
using Taller_Mecanico_Arqui.Domain.Entities;

namespace OrdenTrabajoService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiculosController : ControllerBase
    {
        private readonly CreateVehiculoUseCase _createUseCase;
        private readonly UpdateVehiculoUseCase _updateUseCase;
        private readonly GetVehiculoByIdUseCase _getByIdUseCase;
        private readonly GetAllVehiculosUseCase _getAllUseCase;
        private readonly GetVehiculosByClienteIdUseCase _getByClienteIdUseCase;
        private readonly DeleteVehiculoUseCase _deleteUseCase;

        public VehiculosController(
            CreateVehiculoUseCase createUseCase,
            UpdateVehiculoUseCase updateUseCase,
            GetVehiculoByIdUseCase getByIdUseCase,
            GetAllVehiculosUseCase getAllUseCase,
            GetVehiculosByClienteIdUseCase getByClienteIdUseCase,
            DeleteVehiculoUseCase deleteUseCase)
        {
            _createUseCase = createUseCase;
            _updateUseCase = updateUseCase;
            _getByIdUseCase = getByIdUseCase;
            _getAllUseCase = getAllUseCase;
            _getByClienteIdUseCase = getByClienteIdUseCase;
            _deleteUseCase = deleteUseCase;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vehiculos = await _getAllUseCase.ExecuteAsync();
            return Ok(vehiculos.Select(ToListDto));
        }

        [HttpGet("cliente/{clienteId}")]
        public async Task<IActionResult> GetByClienteId(int clienteId)
        {
            var vehiculos = await _getByClienteIdUseCase.ExecuteAsync(clienteId);
            return Ok(vehiculos.Select(ToListDto));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _getByIdUseCase.ExecuteAsync(id);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            if (result.Value == null)
                return NotFound(new { message = "Vehículo no encontrado." });

            return Ok(ToDetailDto(result.Value));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVehiculoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _createUseCase.ExecuteAsync(dto);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { vehiculoId = result.Value });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVehiculoDto dto)
        {
            if (id != dto.VehiculoId)
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

        private static VehiculoListDto ToListDto(Vehiculo vehiculo)
        {
            return new VehiculoListDto
            {
                VehiculoId = vehiculo.VehiculoId,
                Placa = vehiculo.Placa,
                ClienteNombre = vehiculo.Cliente?.NombreCompleto.ToString() ?? string.Empty,
                Marca = vehiculo.Marca?.Nombre ?? string.Empty,
                Modelo = vehiculo.Modelo?.Nombre ?? string.Empty,
                Color = vehiculo.ColorVehiculo?.Nombre ?? string.Empty,
                Anio = vehiculo.Anio
            };
        }

        private static VehiculoDetalleDto ToDetailDto(Vehiculo vehiculo)
        {
            return new VehiculoDetalleDto
            {
                VehiculoId = vehiculo.VehiculoId,
                ClienteId = vehiculo.ClienteId,
                ClienteNombre = vehiculo.Cliente?.NombreCompleto.ToString() ?? string.Empty,
                Placa = vehiculo.Placa,
                MarcaId = vehiculo.MarcaId,
                Marca = vehiculo.Marca?.Nombre ?? string.Empty,
                ModeloId = vehiculo.ModeloId,
                Modelo = vehiculo.Modelo?.Nombre ?? string.Empty,
                ColorVehiculoId = vehiculo.ColorVehiculoId,
                Color = vehiculo.ColorVehiculo?.Nombre ?? string.Empty,
                Anio = vehiculo.Anio
            };
        }
    }
}
