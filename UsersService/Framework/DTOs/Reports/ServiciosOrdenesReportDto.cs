namespace Taller_Mecanico_Users.Framework.DTOs.Reports;

/// <summary>
/// DTO para Reporte 2: Servicios + Órdenes
/// Incluye métricas agregadas: cantidad, monto total, porcentaje
/// </summary>
public class ServiciosOrdenesReportDto
{
    public List<ServicioMetricaDto> Servicios { get; set; } = new();
    public decimal TotalBsMonto { get; set; }
    public int TotalOrdenes { get; set; }
    public DateTime GeneradoEn { get; set; } = DateTime.Now;
    public string GeneradoPor { get; set; } = string.Empty;
}

public class ServicioMetricaDto
{
    public int ServicioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int CantidadOrdenes { get; set; }
    public decimal TotalBs { get; set; }
    public decimal Porcentaje { get; set; }  // Porcentaje del monto total
}
