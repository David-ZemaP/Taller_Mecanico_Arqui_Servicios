namespace Taller_Mecanico_WebService.Services.Reports;

/// <summary>
/// Interfaz para generación de gráficos estadísticos
/// Implementa patrón Strategy para múltiples tipos de gráficos
/// </summary>
public interface IChartService
{
    /// <summary>
    /// Genera gráfico de pastel (pie chart) de distribución de servicios
    /// </summary>
    Task<byte[]> GenerarGraficoPastelAsync(
        Dictionary<string, decimal> datos,
        string titulo,
        int ancho = 600,
        int alto = 400);

    /// <summary>
    /// Genera gráfico de barras de servicios
    /// </summary>
    Task<byte[]> GenerarGraficoBarrasAsync(
        Dictionary<string, decimal> datos,
        string titulo,
        string ejeX,
        string ejeY,
        int ancho = 600,
        int alto = 400);

    /// <summary>
    /// Genera gráfico de líneas de tendencia temporal
    /// </summary>
    Task<byte[]> GenerarGraficoLineasAsync(
        Dictionary<string, decimal> datos,
        string titulo,
        string ejeX,
        string ejeY,
        int ancho = 600,
        int alto = 400);
}
