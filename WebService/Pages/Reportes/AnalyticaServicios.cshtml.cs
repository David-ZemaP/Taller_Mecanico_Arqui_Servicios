using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Taller_Mecanico_WebService.Services.Reports;
using Taller_Mecanico_WebService.Helpers;

namespace Taller_Mecanico_WebService.Pages.Reportes;

/// <summary>
/// PageModel para Reporte de Analítica de Servicios
/// Maneja búsqueda, visualización, gráficos y descarga de reportes
/// </summary>
public class AnalyticaServiciosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPDFReportService _pdfService;
    private readonly IExcelReportService _excelService;
    private readonly IChartService _chartService;
    private readonly AuditInfoHelper _auditHelper;
    private readonly ILogger<AnalyticaServiciosModel> _logger;

    private const string API_BASE_URL = "http://localhost:5000/api/reports";

    public AnalyticaServiciosModel(
        IHttpClientFactory httpClientFactory,
        IPDFReportService pdfService,
        IExcelReportService excelService,
        IChartService chartService,
        AuditInfoHelper auditHelper,
        ILogger<AnalyticaServiciosModel> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
        _excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));
        _chartService = chartService ?? throw new ArgumentNullException(nameof(chartService));
        _auditHelper = auditHelper ?? throw new ArgumentNullException(nameof(auditHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [BindProperty(SupportsGet = true)]
    public DateTime? FechaDesde { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FechaHasta { get; set; }

    public List<ServicioViewModel>? Servicios { get; set; }
    public string? ErrorMessage { get; set; }
    public bool BusquedaRealizada { get; set; }
    public string? GraficoBase64 { get; set; }
    
    public decimal TotalRecaudado { get; set; }
    public int TotalServicios { get; set; }
    public int TotalOrdenes { get; set; }
    public decimal PromedioRecaudado { get; set; }

    /// <summary>
    /// Carga inicial de la página
    /// </summary>
    public async Task OnGetAsync()
    {
        _logger.LogInformation("📄 Cargando página Analítica de Servicios");
        
        // Valores por defecto: último mes
        if (FechaHasta == null)
            FechaHasta = DateTime.Now;
        
        if (FechaDesde == null)
            FechaDesde = DateTime.Now.AddMonths(-1);

        if (FechaDesde != null && FechaHasta != null)
        {
            await CargarDatos();
        }
    }

    /// <summary>
    /// Handler para búsqueda/generación de reporte
    /// </summary>
    public async Task OnPostBuscarAsync()
    {
        _logger.LogInformation($"🔍 Buscando: desde={FechaDesde}, hasta={FechaHasta}");
        
        if (FechaDesde == null || FechaHasta == null)
        {
            ErrorMessage = "Debe especificar ambas fechas";
            return;
        }

        await CargarDatos();
    }

    /// <summary>
    /// Handler para descargar PDF
    /// </summary>
    public async Task<IActionResult> OnPostDescargarPDFAsync()
    {
        try
        {
            _logger.LogInformation($"📥 Descargando PDF");

            if (FechaDesde == null || FechaHasta == null)
            {
                ErrorMessage = "Debe especificar ambas fechas";
                return Page();
            }

            await CargarDatos();
            if (Servicios == null || Servicios.Count == 0)
            {
                ErrorMessage = "No hay datos para descargar";
                return Page();
            }

            var datosReporte = new { servicios = Servicios };
            var infoAuditoria = _auditHelper.GetAuditInfo();

            // Generar gráfico
            var datosGrafico = Servicios.ToDictionary(s => s.NombreServicio, s => s.TotalRecaudado);
            var graficoImg = await _chartService.GenerarGraficoPastelAsync(
                datosGrafico,
                "Distribución de Servicios",
                600,
                400);

            var pdfBytes = await _pdfService.GenerarReporteAnalyticaServiciosAsync(
                datosReporte,
                FechaDesde.Value,
                FechaHasta.Value,
                graficoImg,
                infoAuditoria);

            return File(pdfBytes, "application/pdf",
                $"ReporteAnalyticaServicios_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error descargando PDF");
            ErrorMessage = $"Error al descargar PDF: {ex.Message}";
            return Page();
        }
    }

    /// <summary>
    /// Handler para descargar Excel
    /// </summary>
    public async Task<IActionResult> OnPostDescargarExcelAsync()
    {
        try
        {
            _logger.LogInformation($"📥 Descargando Excel");

            if (FechaDesde == null || FechaHasta == null)
            {
                ErrorMessage = "Debe especificar ambas fechas";
                return Page();
            }

            await CargarDatos();
            if (Servicios == null || Servicios.Count == 0)
            {
                ErrorMessage = "No hay datos para descargar";
                return Page();
            }

            var datosReporte = new { servicios = Servicios };
            var infoAuditoria = _auditHelper.GetAuditInfo();

            var excelBytes = await _excelService.GenerarReporteAnalyticaServiciosAsync(
                datosReporte,
                FechaDesde.Value,
                FechaHasta.Value,
                infoAuditoria);

            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ReporteAnalyticaServicios_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error descargando Excel");
            ErrorMessage = $"Error al descargar Excel: {ex.Message}";
            return Page();
        }
    }

    /// <summary>
    /// Carga datos desde la API del UsersService
    /// </summary>
    private async Task CargarDatos()
    {
        try
        {
            BusquedaRealizada = true;
            var httpClient = _httpClientFactory.CreateClient();

            var url = $"{API_BASE_URL}/servicios?fechaDesde={FechaDesde:yyyy-MM-dd}&fechaHasta={FechaHasta:yyyy-MM-dd}";

            _logger.LogInformation($"📡 Llamando API: {url}");

            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                ErrorMessage = $"Error al obtener datos: {response.StatusCode}";
                _logger.LogWarning(ErrorMessage);
                return;
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(jsonContent);

            Servicios = new List<ServicioViewModel>();
            TotalRecaudado = 0;
            TotalServicios = 0;
            TotalOrdenes = 0;

            if (jsonDocument.RootElement.TryGetProperty("data", out var dataElement))
            {
                if (dataElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var servicioElement in dataElement.EnumerateArray())
                    {
                        var servicio = ParseServicioFromJson(servicioElement);
                        Servicios.Add(servicio);
                        TotalRecaudado += servicio.TotalRecaudado;
                        TotalServicios += servicio.CantidadAtendida;
                    }
                }
            }

            // Calcular totales
            if (TotalServicios > 0)
            {
                TotalOrdenes = TotalServicios;
                PromedioRecaudado = TotalRecaudado / TotalOrdenes;
            }

            // Generar gráfico
            if (Servicios.Count > 0)
            {
                var datosGrafico = Servicios.ToDictionary(s => s.NombreServicio, s => s.TotalRecaudado);
                var graficoImg = await _chartService.GenerarGraficoPastelAsync(
                    datosGrafico,
                    "Distribución de Servicios",
                    600,
                    400);

                GraficoBase64 = Convert.ToBase64String(graficoImg);
            }

            _logger.LogInformation($"✅ Se cargaron {Servicios.Count} servicios | Total Recaudado: Bs. {TotalRecaudado:N2}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error cargando datos");
            ErrorMessage = $"Error cargando datos: {ex.Message}";
            Servicios = new List<ServicioViewModel>();
        }
    }

    /// <summary>
    /// Parsea elemento JSON a modelo ServicioViewModel
    /// </summary>
    private ServicioViewModel ParseServicioFromJson(JsonElement element)
    {
        return new ServicioViewModel
        {
            NombreServicio = element.TryGetProperty("nombreServicio", out var nombre) ? nombre.GetString() : "N/A",
            CantidadAtendida = element.TryGetProperty("cantidadAtendida", out var cantidad) ? cantidad.GetInt32() : 0,
            TotalRecaudado = element.TryGetProperty("totalRecaudado", out var total) ? total.GetDecimal() : 0
        };
    }
}

/// <summary>
/// View Model para Servicio
/// </summary>
public class ServicioViewModel
{
    public string? NombreServicio { get; set; }
    public int CantidadAtendida { get; set; }
    public decimal TotalRecaudado { get; set; }
}
