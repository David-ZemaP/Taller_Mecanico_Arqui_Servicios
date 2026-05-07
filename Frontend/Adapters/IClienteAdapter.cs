using System.Net.Http.Json;
using Taller_Mecanico_Arqui.Frontend.DTOs;
using Taller_Mecanico_Arqui.Frontend.DTOs.OrdenTrabajo;

namespace Taller_Mecanico_Arqui.Frontend.Adapters;

public interface IClienteAdapter
{
    Task<List<ClienteListDto>> GetAllAsync();
    Task<ClienteDetalleDto?> GetByIdAsync(int id);
    Task<(bool Success, int? ClienteId, string? Error)> CreateAsync(CreateClienteDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(UpdateClienteDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(int id);
}

public class ClienteListDto
{
    public int ClienteId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Ci { get; set; } = string.Empty;
    public int Telefono { get; set; }
    public string? Email { get; set; }
    public string TipoCliente { get; set; } = string.Empty;
    public int VehiculoCount { get; set; }
}

public class ClienteDetalleDto
{
    public int ClienteId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public string Ci { get; set; } = string.Empty;
    public int Telefono { get; set; }
    public string? Email { get; set; }
    public string TipoCliente { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
    public int? UsuarioLoginId { get; set; }
    public List<VehiculoLookupDto> Vehiculos { get; set; } = new();
}

public class CreateClienteDto
{
    public string Nombres { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public int CiNumero { get; set; }
    public string? CiComplemento { get; set; }
    public int Telefono { get; set; }
    public string Email { get; set; } = string.Empty;
    public string TipoCliente { get; set; } = "Regular";
}

public class UpdateClienteDto
{
    public int ClienteId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public int CiNumero { get; set; }
    public string? CiComplemento { get; set; }
    public int Telefono { get; set; }
    public string Email { get; set; } = string.Empty;
    public string TipoCliente { get; set; } = "Regular";
}