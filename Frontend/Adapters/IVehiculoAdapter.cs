using System.Net.Http.Json;

namespace Taller_Mecanico_Arqui.Frontend.Adapters;

public interface IVehiculoAdapter
{
    Task<List<VehiculoListDto>> GetAllAsync();
    Task<List<VehiculoListDto>> GetByClienteIdAsync(int clienteId);
    Task<VehiculoDetalleDto?> GetByIdAsync(int id);
    Task<(bool Success, int? VehiculoId, string? Error)> CreateAsync(CreateVehiculoDto dto);
    Task<(bool Success, string? Error)> UpdateAsync(UpdateVehiculoDto dto);
    Task<(bool Success, string? Error)> DeleteAsync(int id);
}

public class VehiculoListDto
{
    public int VehiculoId { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Anio { get; set; }
}

public class VehiculoDetalleDto
{
    public int VehiculoId { get; set; }
    public string Placa { get; set; } = string.Empty;
    public int MarcaId { get; set; }
    public string Marca { get; set; } = string.Empty;
    public int ModeloId { get; set; }
    public string Modelo { get; set; } = string.Empty;
    public int ColorVehiculoId { get; set; }
    public string Color { get; set; } = string.Empty;
    public int Anio { get; set; }
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
}

public class CreateVehiculoDto
{
    public int ClienteId { get; set; }
    public string Placa { get; set; } = string.Empty;
    public int MarcaId { get; set; }
    public int ModeloId { get; set; }
    public int ColorVehiculoId { get; set; }
    public int Anio { get; set; }
}

public class UpdateVehiculoDto
{
    public int VehiculoId { get; set; }
    public int ClienteId { get; set; }
    public string Placa { get; set; } = string.Empty;
    public int MarcaId { get; set; }
    public int ModeloId { get; set; }
    public int ColorVehiculoId { get; set; }
    public int Anio { get; set; }
}