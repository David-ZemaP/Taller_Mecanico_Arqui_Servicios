using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Taller_Mecanico_WebService.Services.Reports;
using Taller_Mecanico_WebService.Helpers;

namespace Taller_Mecanico_WebService.Pages.Reportes;

/// <summary>
/// PageModel para Reporte de Clientes y Vehículos
/// Maneja búsqueda, visualización y descarga de reportes
/// </summary>
public class ClientesVehiculosModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IPDFReportService _pdfService;
    private readonly IExcelReportService _excelService;
    private readonly AuditInfoHelper _auditHelper;
    private readonly ILogger<ClientesVehiculosModel> _logger;

    private const string API_BASE_URL = "http://localhost:5297/api/reports";

    public ClientesVehiculosModel(
        IHttpClientFactory httpClientFactory,
        IPDFReportService pdfService,
        IExcelReportService excelService,
        AuditInfoHelper auditHelper,
        ILogger<ClientesVehiculosModel> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
        _excelService = excelService ?? throw new ArgumentNullException(nameof(excelService));
        _auditHelper = auditHelper ?? throw new ArgumentNullException(nameof(auditHelper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [BindProperty(SupportsGet = true)]
    public string? NombreCliente { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? MarcaVehiculo { get; set; }

    public List<ClienteViewModel>? Clientes { get; set; }
    public string? ErrorMessage { get; set; }
    public bool BusquedaRealizada { get; set; }
    public int TotalVehiculos { get; set; }

    /// <summary>
    /// Carga inicial de la página
    /// </summary>
    public async Task OnGetAsync()
    {
        _logger.LogInformation("📄 Cargando página Clientes y Vehículos");
        
        if (!string.IsNullOrEmpty(NombreCliente) || !string.IsNullOrEmpty(MarcaVehiculo))
        {
            await CargarDatos();
        }
    }

    /// <summary>
    /// Handler para búsqueda
    /// </summary>
    public async Task OnPostBuscarAsync()
    {
        _logger.LogInformation($"🔍 Buscando: cliente={NombreCliente}, marca={MarcaVehiculo}");
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

            await CargarDatos();
            if (Clientes == null || Clientes.Count == 0)
            {
                ErrorMessage = "No hay datos para descargar";
                return Page();
            }

            var datosReporte = new { clientes = Clientes };
            var infoAuditoria = _auditHelper.GetAuditInfo();

            var pdfBytes = await _pdfService.GenerarReporteClientesVehiculosAsync(
                datosReporte,
                NombreCliente ?? "N/A",
                MarcaVehiculo ?? "N/A",
                infoAuditoria);

            return File(pdfBytes, "application/pdf", 
                $"ReporteClientesVehiculos_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
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

            await CargarDatos();
            if (Clientes == null || Clientes.Count == 0)
            {
                ErrorMessage = "No hay datos para descargar";
                return Page();
            }

            var datosReporte = new { clientes = Clientes };
            var infoAuditoria = _auditHelper.GetAuditInfo();

            var excelBytes = await _excelService.GenerarReporteClientesVehiculosAsync(
                datosReporte,
                NombreCliente ?? "N/A",
                MarcaVehiculo ?? "N/A",
                infoAuditoria);

            return File(excelBytes, 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ReporteClientesVehiculos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
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
            
            var url = $"{API_BASE_URL}/cliente/vehiculos";
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(NombreCliente))
                queryParams.Add($"nombreCliente={Uri.EscapeDataString(NombreCliente)}");
            
            if (!string.IsNullOrEmpty(MarcaVehiculo))
                queryParams.Add($"marca={Uri.EscapeDataString(MarcaVehiculo)}");

            if (queryParams.Count > 0)
                url += "?" + string.Join("&", queryParams);

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

            Clientes = new List<ClienteViewModel>();
            TotalVehiculos = 0;

            if (jsonDocument.RootElement.TryGetProperty("data", out var dataElement))
            {
                if (dataElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var clienteElement in dataElement.EnumerateArray())
                    {
                        var cliente = ParseClienteFromJson(clienteElement);
                        Clientes.Add(cliente);
                        TotalVehiculos += cliente.Vehiculos?.Count ?? 0;
                    }
                }
            }

            _logger.LogInformation($"✅ Se cargaron {Clientes.Count} clientes con {TotalVehiculos} vehículos");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error cargando datos");
            ErrorMessage = $"Error cargando datos: {ex.Message}";
            Clientes = new List<ClienteViewModel>();
        }
    }

    /// <summary>
    /// Parsea elemento JSON a modelo ClienteViewModel
    /// </summary>
    private ClienteViewModel ParseClienteFromJson(JsonElement element)
    {
        var cliente = new ClienteViewModel
        {
            Cedula = element.TryGetProperty("cedula", out var cedula) ? cedula.GetString() : "N/A",
            NombreCompleto = element.TryGetProperty("nombreCompleto", out var nombre) ? nombre.GetString() : "N/A",
            Vehiculos = new List<VehiculoViewModel>()
        };

        if (element.TryGetProperty("vehiculos", out var vehiculosElement) && 
            vehiculosElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var vehiculoElement in vehiculosElement.EnumerateArray())
            {
                var vehiculo = new VehiculoViewModel
                {
                    Placa = vehiculoElement.TryGetProperty("placa", out var placa) ? placa.GetString() : "N/A",
                    Marca = vehiculoElement.TryGetProperty("marca", out var marca) ? marca.GetString() : "N/A",
                    Modelo = vehiculoElement.TryGetProperty("modelo", out var modelo) ? modelo.GetString() : "N/A",
                    Anio = vehiculoElement.TryGetProperty("anio", out var anio) ? anio.GetInt32() : 0,
                    Estado = vehiculoElement.TryGetProperty("estado", out var estado) ? estado.GetString() : "Activo"
                };

                cliente.Vehiculos.Add(vehiculo);
            }
        }

        return cliente;
    }
}

/// <summary>
/// View Model para Cliente
/// </summary>
public class ClienteViewModel
{
    public string? Cedula { get; set; }
    public string? NombreCompleto { get; set; }
    public List<VehiculoViewModel>? Vehiculos { get; set; }
}

/// <summary>
/// View Model para Vehículo
/// </summary>
public class VehiculoViewModel
{
    public string? Placa { get; set; }
    public string? Marca { get; set; }
    public string? Modelo { get; set; }
    public int Anio { get; set; }
    public string? Estado { get; set; }
}
