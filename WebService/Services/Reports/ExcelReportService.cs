using OfficeOpenXml;
using OfficeOpenXml.Style;
using Taller_Mecanico_WebService.Helpers;

namespace Taller_Mecanico_WebService.Services.Reports;

public class ExcelReportService : IExcelReportService
{
    private readonly ILogger<ExcelReportService> _logger;
    private readonly ReportFormatter _formatter;
    private readonly AuditInfoHelper _auditHelper;

    public ExcelReportService(ILogger<ExcelReportService> logger, ReportFormatter formatter, AuditInfoHelper auditHelper)
    {
        _logger = logger;
        _formatter = formatter;
        _auditHelper = auditHelper;
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<byte[]> GenerarReporteClientesVehiculosAsync(
        dynamic reporteData,
        string nombreCliente,
        string marcaVehiculo,
        string infoAuditoria)
    {
        await Task.Yield();
        try
        {
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Clientes y Vehículos");

            var teal = System.Drawing.Color.FromArgb(32, 201, 151);
            var darkBlue = System.Drawing.Color.FromArgb(41, 128, 185);
            var white = System.Drawing.Color.White;

            // Fila 1: Título
            ws.Cells["A1:H1"].Merge = true;
            ws.Cells["A1"].Value = "REPORTE DE CLIENTES Y VEHÍCULOS";
            SetHeaderStyle(ws.Cells["A1:H1"], darkBlue, white, 14);

            // Fila 2: Filtros
            ws.Cells["A2:H2"].Merge = true;
            ws.Cells["A2"].Value = $"Filtros: Nombre/CI: {nombreCliente ?? "Todos"} | Marca: {marcaVehiculo ?? "Todas"}";
            ws.Cells["A2"].Style.Font.Italic = true;
            ws.Cells["A2"].Style.Font.Size = 10;

            // Fila 4: Cabeceras
            var headers = new[] { "Nro.", "CI/NIT", "Nombre Cliente", "Placa", "Marca", "Modelo", "Año", "Estado" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[4, i + 1].Value = headers[i];
                SetHeaderStyle(ws.Cells[4, i + 1], darkBlue, white, 10);
            }

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
                            ws.Cells[fila, 1].Value = nro;
                            ws.Cells[fila, 2].Value = (string?)(cliente.cedula ?? "N/A");
                            ws.Cells[fila, 3].Value = (string?)(cliente.nombreCompleto ?? "N/A");
                            ws.Cells[fila, 4].Value = (string?)(vehiculo.placa ?? "N/A");
                            ws.Cells[fila, 5].Value = (string?)(vehiculo.marca ?? "N/A");
                            ws.Cells[fila, 6].Value = (string?)(vehiculo.modelo ?? "N/A");
                            ws.Cells[fila, 7].Value = vehiculo.anio;
                            ws.Cells[fila, 8].Value = (string?)(vehiculo.estado ?? "Activo");
                            ApplyRowStyle(ws, fila, 8, fila % 2 == 0);
                            fila++;
                            nro++;
                        }
                    }
                    else
                    {
                        ws.Cells[fila, 1].Value = nro;
                        ws.Cells[fila, 2].Value = (string?)(cliente.cedula ?? "N/A");
                        ws.Cells[fila, 3].Value = (string?)(cliente.nombreCompleto ?? "N/A");
                        ApplyRowStyle(ws, fila, 8, fila % 2 == 0);
                        fila++;
                        nro++;
                    }
                }
            }

            // Totales
            int totalClientes = reporteData?.clientes?.Count ?? 0;
            fila++;
            ws.Cells[fila, 1].Value = $"Total clientes: {totalClientes}";
            ws.Cells[fila, 1].Style.Font.Bold = true;

            // Auditoría
            fila += 2;
            ws.Cells[fila, 1].Value = infoAuditoria;
            ws.Cells[fila, 1].Style.Font.Size = 9;
            ws.Cells[fila, 1].Style.Font.Italic = true;

            ws.Column(1).Width = 8; ws.Column(2).Width = 15; ws.Column(3).Width = 22;
            ws.Column(4).Width = 12; ws.Column(5).Width = 12; ws.Column(6).Width = 15;
            ws.Column(7).Width = 8; ws.Column(8).Width = 12;

            _logger.LogInformation("Excel Clientes-Vehículos generado");
            return package.GetAsByteArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando Excel Clientes-Vehículos");
            throw;
        }
    }

    public async Task<byte[]> GenerarReporteAnalyticaServiciosAsync(
        dynamic reporteData,
        DateTime fechaDesde,
        DateTime fechaHasta,
        string infoAuditoria)
    {
        await Task.Yield();
        try
        {
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Servicios");

            var darkBlue = System.Drawing.Color.FromArgb(41, 128, 185);
            var white = System.Drawing.Color.White;
            var purple = System.Drawing.Color.FromArgb(32, 201, 151);

            ws.Cells["A1:D1"].Merge = true;
            ws.Cells["A1"].Value = "ANALÍTICA DE INGRESOS POR SERVICIOS";
            SetHeaderStyle(ws.Cells["A1:D1"], darkBlue, white, 14);

            ws.Cells["A2:D2"].Merge = true;
            ws.Cells["A2"].Value = $"Período: {_formatter.FormatFecha(fechaDesde)} al {_formatter.FormatFecha(fechaHasta)}";
            ws.Cells["A2"].Style.Font.Italic = true;

            var headers = new[] { "Nombre del Servicio", "Cantidad Atendida", "Total Recaudado (Bs.)", "Porcentaje" };
            for (int i = 0; i < headers.Length; i++)
            {
                ws.Cells[4, i + 1].Value = headers[i];
                SetHeaderStyle(ws.Cells[4, i + 1], darkBlue, white, 10);
            }

            int fila = 5;
            decimal totalRecaudado = 0;
            int totalAtendidas = 0;

            if (reporteData?.servicios != null)
            {
                decimal sumaTotal = 0;
                foreach (var s in reporteData.servicios)
                    sumaTotal += (decimal)(s.totalRecaudado ?? 0);

                foreach (var servicio in reporteData.servicios)
                {
                    decimal monto = servicio.totalRecaudado ?? 0;
                    int cantidad = servicio.cantidadAtendida ?? 0;
                    decimal porcentaje = sumaTotal > 0 ? Math.Round(monto / sumaTotal * 100, 2) : 0;

                    ws.Cells[fila, 1].Value = (string?)(servicio.nombreServicio ?? "N/A");
                    ws.Cells[fila, 2].Value = cantidad;
                    ws.Cells[fila, 3].Value = (double)monto;
                    ws.Cells[fila, 3].Style.Numberformat.Format = "\"Bs. \"#,##0.00";
                    ws.Cells[fila, 4].Value = $"{porcentaje:F2}%";
                    ws.Cells[fila, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    ApplyRowStyle(ws, fila, 4, fila % 2 == 0);

                    totalRecaudado += monto;
                    totalAtendidas += cantidad;
                    fila++;
                }
            }

            // Fila de totales
            fila++;
            ws.Cells[fila, 1].Value = "TOTAL RECAUDADO";
            ws.Cells[fila, 2].Value = totalAtendidas;
            ws.Cells[fila, 3].Value = (double)totalRecaudado;
            ws.Cells[fila, 3].Style.Numberformat.Format = "\"Bs. \"#,##0.00";
            for (int c = 1; c <= 4; c++)
            {
                ws.Cells[fila, c].Style.Font.Bold = true;
                ws.Cells[fila, c].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[fila, c].Style.Fill.BackgroundColor.SetColor(purple);
                ws.Cells[fila, c].Style.Font.Color.SetColor(white);
            }

            fila += 2;
            ws.Cells[fila, 1].Value = infoAuditoria;
            ws.Cells[fila, 1].Style.Font.Size = 9;
            ws.Cells[fila, 1].Style.Font.Italic = true;

            ws.Column(1).Width = 28; ws.Column(2).Width = 18; ws.Column(3).Width = 20; ws.Column(4).Width = 15;

            _logger.LogInformation("Excel Servicios generado");
            return package.GetAsByteArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando Excel Servicios");
            throw;
        }
    }

    private static void SetHeaderStyle(ExcelRange cells, System.Drawing.Color bgColor, System.Drawing.Color fontColor, int fontSize)
    {
        cells.Style.Fill.PatternType = ExcelFillStyle.Solid;
        cells.Style.Fill.BackgroundColor.SetColor(bgColor);
        cells.Style.Font.Bold = true;
        cells.Style.Font.Color.SetColor(fontColor);
        cells.Style.Font.Size = fontSize;
        cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
    }

    private static void ApplyRowStyle(ExcelWorksheet ws, int row, int cols, bool alternate)
    {
        if (!alternate) return;
        var altColor = System.Drawing.Color.FromArgb(240, 240, 248);
        for (int c = 1; c <= cols; c++)
        {
            ws.Cells[row, c].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[row, c].Style.Fill.BackgroundColor.SetColor(altColor);
        }
    }
}
