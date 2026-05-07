using Microsoft.AspNetCore.Mvc;
using Taller_Mecanico_Arqui.Application.DTOs.Productos;
using Taller_Mecanico_Arqui.Application.UseCases.Productos;
using Taller_Mecanico_Arqui.Domain.Entities;

namespace OrdenTrabajoService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly CreateProductoUseCase _createUseCase;
        private readonly UpdateProductoUseCase _updateUseCase;
        private readonly GetProductoByIdUseCase _getByIdUseCase;
        private readonly GetAllProductosUseCase _getAllUseCase;
        private readonly DeleteProductoUseCase _deleteUseCase;

        public ProductosController(
            CreateProductoUseCase createUseCase,
            UpdateProductoUseCase updateUseCase,
            GetProductoByIdUseCase getByIdUseCase,
            GetAllProductosUseCase getAllUseCase,
            DeleteProductoUseCase deleteUseCase)
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
            var productos = await _getAllUseCase.ExecuteAsync();
            var dtos = productos.Select(ToDto);
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _getByIdUseCase.ExecuteAsync(id);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            if (result.Value == null)
                return NotFound(new { message = "Producto no encontrado." });

            return Ok(ToDto(result.Value));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _createUseCase.ExecuteAsync(dto);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { productoId = result.Value });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductoDto dto)
        {
            if (id != dto.ProductoId)
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

        private static ProductoListDto ToDto(Producto p) => new()
        {
            ProductoId = p.ProductoId,
            Nombre = p.Nombre,
            Precio = p.Precio,
            Stock = p.Stock
        };
    }
}
