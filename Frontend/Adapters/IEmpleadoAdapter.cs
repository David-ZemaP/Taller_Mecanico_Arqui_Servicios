using System.Net.Http.Json;

namespace Taller_Mecanico_Arqui.Frontend.Adapters;

public interface IEmpleadoAdapter
{
    Task<List<EmpleadoListDto>> GetAllAsync();
    Task<EmpleadoDetalleDto?> GetByIdAsync(int id);
    Task<(bool Success, int? EmpleadoId, string? Error)> CreateAsync(CreateEmpleadoDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(UpdateEmpleadoDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(int id);
}

public class EmpleadoListDto
{
    public int EmpleadoId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Ci { get; set; } = string.Empty;
    public int Telefono { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string EstadoLaboral { get; set; } = string.Empty;
    public DateTime? FechaContratacion { get; set; }
}

public class EmpleadoDetalleDto
{
    public int EmpleadoId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public string Ci { get; set; } = string.Empty;
    public int Telefono { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string EstadoLaboral { get; set; } = string.Empty;
    public DateTime? FechaContratacion { get; set; }
    public int? UsuarioLoginId { get; set; }
}

public class CreateEmpleadoDto
{
    public string Nombres { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public int CiNumero { get; set; }
    public string? CiComplemento { get; set; }
    public int Telefono { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
}

public class UpdateEmpleadoDto
{
    public int EmpleadoId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public int CiNumero { get; set; }
    public string? CiComplemento { get; set; }
    public int Telefono { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
}