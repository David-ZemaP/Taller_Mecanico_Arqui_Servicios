namespace Taller_Mecanico_Users.Framework.DTOs.Reports;

/// <summary>
/// DTO para Reporte 1: Clientes + Vehículos
/// Estructura anidada: Clientes → Vehículos
/// </summary>
public class ClientesVehiculosReportDto
{
    public List<ClienteReportDto> Clientes { get; set; } = new();
    public DateTime GeneradoEn { get; set; } = DateTime.Now;
    public string GeneradoPor { get; set; } = string.Empty;
}

public class ClienteReportDto
{
    public int ClienteId { get; set; }
    public string CiNit { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public bool Activo { get; set; }
    public List<VehiculoReportDto> Vehiculos { get; set; } = new();
}

public class VehiculoReportDto
{
    public int VehiculoId { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anno { get; set; }
    public bool Activo { get; set; }
}
