namespace Taller_Mecanico_WebService.Services.Reports;

/// <summary>
/// Interfaz para generación de reportes en formato Excel
/// Implementa patrón Strategy para exportación a Excel
/// </summary>
public interface IExcelReportService
{
    /// <summary>
    /// Genera Excel del reporte de Clientes y Vehículos
    /// </summary>
    Task<byte[]> GenerarReporteClientesVehiculosAsync(
        dynamic reporteData,
        string nombreCliente,
        string marcaVehiculo,
        string infoAuditoria);

    /// <summary>
    /// Genera Excel del reporte de Analítica de Servicios
    /// </summary>
    Task<byte[]> GenerarReporteAnalyticaServiciosAsync(
        dynamic reporteData,
        DateTime fechaDesde,
        DateTime fechaHasta,
        string infoAuditoria);
}
