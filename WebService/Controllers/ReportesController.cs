using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Taller_Mecanico_WebService.Helpers;
using Taller_Mecanico_WebService.Services.Reports;

namespace Taller_Mecanico_WebService.Controllers;

/// <summary>
/// Controller para generación de reportes en PDF y Excel
/// Consume APIs de UsersService y OrdenTrabajoService
/// Genera reportes utilizando PDFReportService, ExcelReportService y ChartService
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPDFReportService _pdfService;
    private readonly IExcelReportService _excelService;
    private readonly IChartService _chartService;
    private readonly AuditInfoHelper _auditHelper;
    private readonly ReportFormatter _formatter;
    private readonly ILogger<ReportesController> _logger;

    private const string USERS_SERVICE_URL = "http://localhost:5000";
    private const string ORDEN_TRABAJO_SERVICE_URL = "http://localhost:5001";

    public ReportesController(
        IHttpClientFactory httpClientFactory,
        IPDFReportService pdfService,
        IExcelReportService excelService,
        IChartService chartService,
        AuditInfoHelper auditHelper,
        ReportFormatter formatter,
        ILogger<ReportesController> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
        _excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));
        _chartService = chartService ?? throw new ArgumentNullException(nameof(chartService));
        _auditHelper = auditHelper ?? throw new ArgumentNullException(nameof(auditHelper));
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Descarga reporte de Clientes y Vehículos en PDF
    /// </summary>
    [HttpPost("clientes-vehiculos-pdf")]
    public async Task<IActionResult> DescargarClientesVehiculosPDF(
        [FromQuery] string? nombreCliente,
        [FromQuery] string? marcaVehiculo)
    {
        try
        {
            _logger.LogInformation($"📥 Solicitando PDF: Clientes y Vehículos");

            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{USERS_SERVICE_URL}/api/reports/cliente/vehiculos?nombreCliente={nombreCliente}&marca={marcaVehiculo}";

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"⚠️ Error al consultar API: {response.StatusCode}");
                return BadRequest("Error al obtener datos del reporte");
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var reporteData = JsonDocument.Parse(jsonContent).RootElement.GetProperty("data");

            var infoAuditoria = _auditHelper.GetAuditInfo();

            var pdfBytes = await _pdfService.GenerarReporteClientesVehiculosAsync(
                reporteData,
                nombreCliente ?? "N/A",
                marcaVehiculo ?? "N/A",
                infoAuditoria);

            _logger.LogInformation($"✅ PDF descargado exitosamente");
            return File(pdfBytes, "application/pdf", $"ReporteClientesVehiculos_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error descargando PDF");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Descarga reporte de Clientes y Vehículos en Excel
    /// </summary>
    [HttpPost("clientes-vehiculos-excel")]
    public async Task<IActionResult> DescargarClientesVehiculosExcel(
        [FromQuery] string? nombreCliente,
        [FromQuery] string? marcaVehiculo)
    {
        try
        {
            _logger.LogInformation($"📥 Solicitando Excel: Clientes y Vehículos");

            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{USERS_SERVICE_URL}/api/reports/cliente/vehiculos?nombreCliente={nombreCliente}&marca={marcaVehiculo}";

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"⚠️ Error al consultar API: {response.StatusCode}");
                return BadRequest("Error al obtener datos del reporte");
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var reporteData = JsonDocument.Parse(jsonContent).RootElement.GetProperty("data");

            var infoAuditoria = _auditHelper.GetAuditInfo();

            var excelBytes = await _excelService.GenerarReporteClientesVehiculosAsync(
                reporteData,
                nombreCliente ?? "N/A",
                marcaVehiculo ?? "N/A",
                infoAuditoria);

            _logger.LogInformation($"✅ Excel descargado exitosamente");
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ReporteClientesVehiculos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error descargando Excel");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Descarga reporte de Analítica de Servicios en PDF
    /// </summary>
    [HttpPost("servicios-pdf")]
    public async Task<IActionResult> DescargarServiciosPDF(
        [FromQuery] DateTime fechaDesde,
        [FromQuery] DateTime fechaHasta)
    {
        try
        {
            _logger.LogInformation($"📥 Solicitando PDF: Analítica de Servicios");

            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{USERS_SERVICE_URL}/api/reports/servicios?fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}";

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"⚠️ Error al consultar API: {response.StatusCode}");
                return BadRequest("Error al obtener datos del reporte");
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var reporteData = JsonDocument.Parse(jsonContent).RootElement.GetProperty("data");

            // Generar gráfico
            var datosGrafico = ExtractDataForChart(reporteData);
            byte[]? graficoImg = null;

            if (datosGrafico.Count > 0)
            {
                graficoImg = await _chartService.GenerarGraficoPastelAsync(
                    datosGrafico,
                    "Distribución de Servicios",
                    600,
                    400);
            }

            var infoAuditoria = _auditHelper.GetAuditInfo();

            var pdfBytes = await _pdfService.GenerarReporteAnalyticaServiciosAsync(
                reporteData,
                fechaDesde,
                fechaHasta,
                graficoImg,
                infoAuditoria);

            _logger.LogInformation($"✅ PDF de Analítica descargado exitosamente");
            return File(pdfBytes, "application/pdf", $"ReporteAnalyticaServicios_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error descargando PDF de Analítica");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Descarga reporte de Analítica de Servicios en Excel
    /// </summary>
    [HttpPost("servicios-excel")]
    public async Task<IActionResult> DescargarServiciosExcel(
        [FromQuery] DateTime fechaDesde,
        [FromQuery] DateTime fechaHasta)
    {
        try
        {
            _logger.LogInformation($"📥 Solicitando Excel: Analítica de Servicios");

            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{USERS_SERVICE_URL}/api/reports/servicios?fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}";

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"⚠️ Error al consultar API: {response.StatusCode}");
                return BadRequest("Error al obtener datos del reporte");
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var reporteData = JsonDocument.Parse(jsonContent).RootElement.GetProperty("data");

            var infoAuditoria = _auditHelper.GetAuditInfo();

            var excelBytes = await _excelService.GenerarReporteAnalyticaServiciosAsync(
                reporteData,
                fechaDesde,
                fechaHasta,
                infoAuditoria);

            _logger.LogInformation($"✅ Excel de Analítica descargado exitosamente");
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ReporteAnalyticaServicios_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error descargando Excel de Analítica");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene datos de reporte de Clientes y Vehículos para visualizar en UI
    /// </summary>
    [HttpGet("clientes-vehiculos-data")]
    public async Task<IActionResult> ObtenerDatosClientesVehiculos(
        [FromQuery] string? nombreCliente,
        [FromQuery] string? marcaVehiculo)
    {
        try
        {
            _logger.LogInformation($"📊 Obteniendo datos: Clientes y Vehículos");

            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{USERS_SERVICE_URL}/api/reports/cliente/vehiculos?nombreCliente={nombreCliente}&marca={marcaVehiculo}";

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Error al obtener datos");
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(jsonContent);

            return Ok(jsonDocument.RootElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo datos");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene datos de reporte de Analítica de Servicios para visualizar en UI
    /// </summary>
    [HttpGet("servicios-data")]
    public async Task<IActionResult> ObtenerDatosServicios(
        [FromQuery] DateTime fechaDesde,
        [FromQuery] DateTime fechaHasta)
    {
        try
        {
            _logger.LogInformation($"📊 Obteniendo datos: Analítica de Servicios");

            var httpClient = _httpClientFactory.CreateClient();
            var url = $"{USERS_SERVICE_URL}/api/reports/servicios?fechaDesde={fechaDesde:yyyy-MM-dd}&fechaHasta={fechaHasta:yyyy-MM-dd}";

            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return BadRequest("Error al obtener datos");
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(jsonContent);

            return Ok(jsonDocument.RootElement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo datos");
            return StatusCode(500, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Extrae datos para gráfico desde respuesta de API
    /// </summary>
    private Dictionary<string, decimal> ExtractDataForChart(JsonElement reporteData)
    {
        var datos = new Dictionary<string, decimal>();

        if (reporteData.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in reporteData.EnumerateArray())
            {
                var nombre = item.GetProperty("nombreServicio").GetString() ?? "N/A";
                var monto = item.GetProperty("totalRecaudado").GetDecimal();
                datos[nombre] = monto;
            }
        }

        return datos;
    }
}
