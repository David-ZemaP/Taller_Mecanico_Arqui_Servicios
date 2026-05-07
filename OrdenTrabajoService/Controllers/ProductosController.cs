using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdenTrabajoService.Application.DTOs.Producto;
using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Interfaces;

namespace OrdenTrabajoService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductosController : ControllerBase
    {
        private readonly IRepository<Producto> _repository;

        public ProductosController(IRepository<Producto> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var productos = await _repository.GetAllAsync();
            var dtos = productos
                .Where(p => !p.IsDeleted)
                .Select(p => new ProductoDto
                {
                    ProductoId = p.ProductoId,
                    Nombre = p.Nombre,
                    Precio = p.Precio,
                    Stock = p.Stock
                });
            return Ok(dtos);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (result.IsFailure)
                return BadRequest(new { error = result.ErrorMessage });
            if (result.Value is null || result.Value.IsDeleted)
                return NotFound();
            var p = result.Value;
            return Ok(new ProductoDto { ProductoId = p.ProductoId, Nombre = p.Nombre, Precio = p.Precio, Stock = p.Stock });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductoDto dto)
        {
            Producto entity;
            try { entity = Producto.Crear(dto.Nombre, dto.Precio, dto.Stock); }
            catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }

            var result = await _repository.AddAsync(entity);
            if (result.IsFailure)
                return BadRequest(new { error = result.ErrorMessage });
            return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductoDto dto)
        {
            var getResult = await _repository.GetByIdAsync(id);
            if (getResult.IsFailure) return BadRequest(new { error = getResult.ErrorMessage });
            if (getResult.Value is null || getResult.Value.IsDeleted) return NotFound();

            try { getResult.Value.ActualizarDatos(dto.Nombre, dto.Precio, dto.Stock); }
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
