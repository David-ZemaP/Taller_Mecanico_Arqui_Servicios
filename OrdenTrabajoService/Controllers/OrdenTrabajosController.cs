using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taller_Mecanico_Arqui.Application.DTOs.OrdenTrabajo;
using Taller_Mecanico_Arqui.Application.Facades;

namespace OrdenTrabajoService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdenTrabajosController : ControllerBase
    {
        private readonly OrdenTrabajoCreate _ordenTrabajoFacade;
        private readonly OrdenTrabajoAnular _anularFacade;
        private readonly UpdateProductStocks _productStocksFacade;

        public OrdenTrabajosController(
            OrdenTrabajoCreate ordenTrabajoFacade,
            OrdenTrabajoAnular anularFacade,
            UpdateProductStocks productStocksFacade)
        {
            _ordenTrabajoFacade = ordenTrabajoFacade;
            _anularFacade = anularFacade;
            _productStocksFacade = productStocksFacade;
        }

        /// <summary>
        /// Obtiene todas las órdenes de trabajo
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var ordenes = await _ordenTrabajoFacade.GetAllAsync();
            return Ok(ordenes);
        }

        /// <summary>
        /// Obtiene una orden de trabajo por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _ordenTrabajoFacade.GetDetalleAsync(id);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(result.Value);
        }

        /// <summary>
        /// Busca vehículos por placa (para autocompletado)
        /// </summary>
        [HttpGet("buscar-vehiculos")]
        public async Task<IActionResult> BuscarVehiculos([FromQuery] string? term, [FromQuery] int? clienteId)
        {
            var vehiculos = await _ordenTrabajoFacade.BuscarVehiculosAsync(term, clienteId);
            return Ok(vehiculos);
        }

        /// <summary>
        /// Crea o actualiza una orden de trabajo ( Upsert )
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrdenTrabajoFormDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _ordenTrabajoFacade.SaveAsync(dto);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = "Orden de trabajo guardada correctamente." });
        }

        /// <summary>
        /// Actualiza solo el estado de trabajo y pago (flujo simple)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOrdenTrabajoDto dto)
        {
            if (id != dto.OrdenTrabajoId)
                return BadRequest(new { message = "El ID de la ruta no coincide con el del cuerpo." });

            var result = await _ordenTrabajoFacade.RegistrarProcesoPrincipalAsync(new OrdenTrabajoFormDto
            {
                OrdenTrabajoId = dto.OrdenTrabajoId,
                VehiculoId = dto.VehiculoId,
                FechaIngreso = dto.FechaIngreso,
                FechaEntrega = dto.FechaEntrega,
                EstadoVehiculo = dto.EstadoVehiculo,
                EstadoTrabajo = dto.EstadoTrabajo,
                EstadoPago = dto.EstadoPago,
                Total = dto.Total
            });

            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            return NoContent();
        }

        /// <summary>
        /// Anula una orden de trabajo
        /// </summary>
        [HttpPost("{id}/anular")]
        public async Task<IActionResult> Anular(int id)
        {
            var result = await _anularFacade.ExecuteAsync(id);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = "Orden de trabajo anulada correctamente." });
        }

        /// <summary>
        /// Reactiva una orden de trabajo anulada
        /// </summary>
        [HttpPost("{id}/reactivar")]
        public async Task<IActionResult> Reactivar(int id)
        {
            var result = await _anularFacade.ReactivarAsync(id);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = "Orden de trabajo reactivada correctamente." });
        }

        /// <summary>
        /// Opciones para estado de trabajo
        /// </summary>
        [HttpGet("opciones-estado-trabajo")]
        public IActionResult GetEstadoTrabajoOptions()
        {
            return Ok(_ordenTrabajoFacade.GetEstadoTrabajoOptions());
        }

        /// <summary>
        /// Opciones para estado de pago
        /// </summary>
        [HttpGet("opciones-estado-pago")]
        public IActionResult GetEstadoPagoOptions()
        {
            return Ok(_ordenTrabajoFacade.GetEstadoPagoOptions());
        }

        /// <summary>
        /// Actualiza el stock de productos después de usar en una orden
        /// </summary>
        [HttpPost("actualizar-stock")]
        public async Task<IActionResult> UpdateProductStocks([FromBody] List<CreateOrdenTrabajoProductoDto> productos)
        {
            var result = await _productStocksFacade.ExecuteAsync(productos);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = "Stock actualizado correctamente." });
        }
    }
}