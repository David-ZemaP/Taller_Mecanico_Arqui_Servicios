using System.Net.Http.Json;

namespace Taller_Mecanico_Arqui.Frontend.Adapters;

public interface IServicioAdapter
{
    Task<List<ServicioListDto>> GetAllAsync();
    Task<ServicioDetalleDto?> GetByIdAsync(int id);
    Task<(bool Success, int? ServicioId, string? Error)> CreateAsync(CreateServicioDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(UpdateServicioDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(int id);
}

public class ServicioListDto
{
    public int ServicioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; }
}

public class ServicioDetalleDto
{
    public int ServicioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public bool Activo { get; set; }
}

public class CreateServicioDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
}

public class UpdateServicioDto
{
    public int ServicioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
}