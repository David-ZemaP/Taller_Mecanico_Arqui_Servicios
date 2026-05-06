using OfficeOpenXml;
using System.IO;

namespace Taller_Mecanico_WebService.Services.Reports;

/// <summary>
/// Servicio para generar reportes en formato Excel con EPPlus
/// Incluye estilos corporativos y auditoría
/// </summary>
public class ExcelReportService : IExcelReportService
{
    private readonly ILogger<ExcelReportService> _logger;
    private readonly ReportFormatter _formatter;
    private readonly AuditInfoHelper _auditHelper;

    public ExcelReportService(ILogger<ExcelReportService> logger, ReportFormatter formatter, AuditInfoHelper auditHelper)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        _auditHelper = auditHelper ?? throw new ArgumentNullException(nameof(auditHelper));

        // Licencia de EPPlus
        EPLicenseProvider.SetLicense("FREE_LICENSE");
    }

    /// <summary>
    /// Genera reporte Excel de Clientes y Vehículos
    /// </summary>
    public async Task<byte[]> GenerarReporteClientesVehiculosAsync(
        dynamic reporteData,
        string nombreCliente,
        string marcaVehiculo,
        string infoAuditoria)
    {
        try
        {
            _logger.LogInformation($"📗 Generando Excel: Reporte Clientes y Vehículos");

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Clientes y Vehículos");

                // Estilos
                var headerStyle = worksheet.Cells["A1:H1"].Style;
                headerStyle.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerStyle.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(41, 128, 185));
                headerStyle.Font.Bold = true;
                headerStyle.Font.Color.SetColor(System.Drawing.Color.White);
                headerStyle.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                // Encabezado
                worksheet.Cells["A1"].Value = "REPORTE DE CLIENTES Y VEHÍCULOS";
                worksheet.Cells["A1:H1"].Merge = true;
                worksheet.Cells["A1"].Style.Font.Size = 14;
                worksheet.Cells["A1"].Style.Font.Bold = true;

                // Filtros
                worksheet.Cells["A2"].Value = $"Filtros: Cliente={nombreCliente ?? "N/A"} | Marca={marcaVehiculo ?? "N/A"}";
                worksheet.Cells["A2:H2"].Merge = true;
                worksheet.Cells["A2"].Style.Font.Size = 10;

                // Encabezados columnas
                var headers = new[] { "Nro.", "CI/NIT", "Nombre Cliente", "Placa", "Marca", "Modelo", "Año", "Estado" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[4, i + 1].Value = headers[i];
                }

                ApplyHeaderStyle(worksheet, 4, headers.Length);

                // Datos
                int fila = 5;
                int nro = 1;
                if (reporteData?.clientes != null)
                {
                    foreach (var cliente in reporteData.clientes)
                    {
                        if (cliente.vehiculos != null && cliente.vehiculos.Count > 0)
                        {
                            foreach (var vehiculo in cliente.vehiculos)
                            {
                                worksheet.Cells[fila, 1].Value = nro;
                                worksheet.Cells[fila, 2].Value = cliente.cedula ?? "N/A";
                                worksheet.Cells[fila, 3].Value = cliente.nombreCompleto ?? "N/A";
                                worksheet.Cells[fila, 4].Value = vehiculo.placa ?? "N/A";
                                worksheet.Cells[fila, 5].Value = vehiculo.marca ?? "N/A";
                                worksheet.Cells[fila, 6].Value = vehiculo.modelo ?? "N/A";
                                worksheet.Cells[fila, 7].Value = vehiculo.anio ?? "N/A";
                                worksheet.Cells[fila, 8].Value = vehiculo.estado ?? "Activo";
                                fila++;
                                nro++;
                            }
                        }
                        else
                        {
                            worksheet.Cells[fila, 1].Value = nro;
                            worksheet.Cells[fila, 2].Value = cliente.cedula ?? "N/A";
                            worksheet.Cells[fila, 3].Value = cliente.nombreCompleto ?? "N/A";
                            fila++;
                            nro++;
                        }
                    }
                }

                // Totales
                fila += 1;
                var totalClientes = reporteData?.clientes?.Count ?? 0;
                var totalVehiculos = reporteData?.clientes?.Sum(c => c.vehiculos?.Count ?? 0) ?? 0;

                worksheet.Cells[fila, 2].Value = $"TOTAL CLIENTES: {totalClientes}";
                worksheet.Cells[fila, 5].Value = $"TOTAL VEHÍCULOS: {totalVehiculos}";
                worksheet.Cells[fila, 2].Style.Font.Bold = true;
                worksheet.Cells[fila, 5].Style.Font.Bold = true;

                // Auditoría
                fila += 2;
                worksheet.Cells[fila, 1].Value = infoAuditoria;
                worksheet.Cells[fila, 1].Style.Font.Size = 9;

                // Ajustar ancho columnas
                worksheet.Columns[1].Width = 8;
                worksheet.Columns[2].Width = 15;
                worksheet.Columns[3].Width = 20;
                worksheet.Columns[4].Width = 12;
                worksheet.Columns[5].Width = 12;
                worksheet.Columns[6].Width = 15;
                worksheet.Columns[7].Width = 8;
                worksheet.Columns[8].Width = 12;

                _logger.LogInformation($"✅ Excel de Clientes y Vehículos generado exitosamente");
                return package.GetAsByteArray();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando Excel de Clientes y Vehículos");
            throw;
        }
    }

    /// <summary>
    /// Genera reporte Excel de Analítica de Servicios
    /// </summary>
    public async Task<byte[]> GenerarReporteAnalyticaServiciosAsync(
        dynamic reporteData,
        DateTime fechaDesde,
        DateTime fechaHasta,
        string infoAuditoria)
    {
        try
        {
            _logger.LogInformation($"📗 Generando Excel: Reporte Analítica de Servicios");

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Analítica de Servicios");

                // Encabezado
                worksheet.Cells["A1"].Value = "ANALÍTICA DE INGRESOS POR SERVICIOS";
                worksheet.Cells["A1:D1"].Merge = true;
                worksheet.Cells["A1"].Style.Font.Size = 14;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(41, 128, 185));
                worksheet.Cells["A1"].Style.Font.Color.SetColor(System.Drawing.Color.White);

                // Rango de fechas
                worksheet.Cells["A2"].Value = $"Período: {_formatter.FormatFecha(fechaDesde)} al {_formatter.FormatFecha(fechaHasta)}";
                worksheet.Cells["A2:D2"].Merge = true;
                worksheet.Cells["A2"].Style.Font.Size = 10;

                // Encabezados de columnas
                var headers = new[] { "Nombre del Servicio", "Cantidad Atendida", "Total Recaudado", "Porcentaje" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[4, i + 1].Value = headers[i];
                }

                ApplyHeaderStyle(worksheet, 4, headers.Length);

                // Datos
                int fila = 5;
                decimal totalRecaudado = 0;
                int totalAtendidas = 0;

                if (reporteData?.servicios != null)
                {
                    var sumaTotal = reporteData.servicios.Sum(s => s.totalRecaudado ?? 0);

                    foreach (var servicio in reporteData.servicios)
                    {
                        var monto = servicio.totalRecaudado ?? 0;
                        var cantidad = servicio.cantidadAtendida ?? 0;
                        var porcentaje = sumaTotal > 0 ? (monto / sumaTotal * 100) : 0;

                        worksheet.Cells[fila, 1].Value = servicio.nombreServicio ?? "N/A";
                        worksheet.Cells[fila, 2].Value = cantidad;
                        worksheet.Cells[fila, 3].Value = monto;
                        worksheet.Cells[fila, 4].Value = $"{porcentaje:F2}%";

                        worksheet.Cells[fila, 2].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                        worksheet.Cells[fila, 3].Style.NumberFormat = "\"Bs. \"#,##0.00";
                        worksheet.Cells[fila, 4].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                        totalRecaudado += monto;
                        totalAtendidas += cantidad;
                        fila++;
                    }
                }

                // Fila de totales
                fila++;
                worksheet.Cells[fila, 1].Value = "TOTAL RECAUDADO";
                worksheet.Cells[fila, 1].Style.Font.Bold = true;
                worksheet.Cells[fila, 1].Style.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(142, 68, 173));
                worksheet.Cells[fila, 1].Style.Font.Color.SetColor(System.Drawing.Color.White);

                worksheet.Cells[fila, 2].Value = totalAtendidas;
                worksheet.Cells[fila, 2].Style.Font.Bold = true;
                worksheet.Cells[fila, 2].Style.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(142, 68, 173));
                worksheet.Cells[fila, 2].Style.Font.Color.SetColor(System.Drawing.Color.White);

                worksheet.Cells[fila, 3].Value = totalRecaudado;
                worksheet.Cells[fila, 3].Style.Font.Bold = true;
                worksheet.Cells[fila, 3].Style.NumberFormat = "\"Bs. \"#,##0.00";
                worksheet.Cells[fila, 3].Style.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(142, 68, 173));
                worksheet.Cells[fila, 3].Style.Font.Color.SetColor(System.Drawing.Color.White);

                // Auditoría
                fila += 2;
                worksheet.Cells[fila, 1].Value = infoAuditoria;
                worksheet.Cells[fila, 1].Style.Font.Size = 9;

                // Ajustar ancho de columnas
                worksheet.Columns[1].Width = 25;
                worksheet.Columns[2].Width = 18;
                worksheet.Columns[3].Width = 18;
                worksheet.Columns[4].Width = 15;

                _logger.LogInformation($"✅ Excel de Analítica generado exitosamente");
                return package.GetAsByteArray();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando Excel de Analítica de Servicios");
            throw;
        }
    }

    /// <summary>
    /// Aplica estilos de encabezado a las celdas
    /// </summary>
    private void ApplyHeaderStyle(ExcelWorksheet worksheet, int fila, int columnas)
    {
        for (int i = 1; i <= columnas; i++)
        {
            var cell = worksheet.Cells[fila, i];
            cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(41, 128, 185));
            cell.Style.Font.Bold = true;
            cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
            cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        }
    }
}
