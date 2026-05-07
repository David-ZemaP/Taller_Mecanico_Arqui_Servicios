using System.Net.Http.Json;

namespace Taller_Mecanico_Arqui.Frontend.Adapters;

public interface IProductoAdapter
{
    Task<List<ProductoListDto>> GetAllAsync();
    Task<ProductoDetalleDto?> GetByIdAsync(int id);
    Task<(bool Success, int? ProductoId, string? Error)> CreateAsync(CreateProductoDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(UpdateProductoDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(int id);
}

public class ProductoListDto
{
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public bool Activo { get; set; }
}

public class ProductoDetalleDto
{
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public bool Activo { get; set; }
}

public class CreateProductoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
}

public class UpdateProductoDto
{
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
}