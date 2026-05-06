namespace Taller_Mecanico_WebService.Services.Reports;

/// <summary>
/// Interfaz para generación de reportes en formato PDF
/// Implementa patrón Strategy para exportación a PDF
/// </summary>
public interface IPDFReportService
{
    /// <summary>
    /// Genera PDF del reporte de Clientes y Vehículos
    /// </summary>
    Task<byte[]> GenerarReporteClientesVehiculosAsync(
        dynamic reporteData,
        string nombreCliente,
        string marcaVehiculo,
        string infoAuditoria);

    /// <summary>
    /// Genera PDF del reporte de Analítica de Servicios
    /// </summary>
    Task<byte[]> GenerarReporteAnalyticaServiciosAsync(
        dynamic reporteData,
        DateTime fechaDesde,
        DateTime fechaHasta,
        byte[] graficoImg,
        string infoAuditoria);
}
