using iTextSharp.text;
using iTextSharp.text.pdf;
using Taller_Mecanico_WebService.Helpers;

namespace Taller_Mecanico_WebService.Services.Reports;

public class PDFReportService : IPDFReportService
{
    private readonly ILogger<PDFReportService> _logger;
    private readonly ReportFormatter _formatter;
    private readonly AuditInfoHelper _auditHelper;

    private static readonly BaseColor ColorAzul = new BaseColor(41, 128, 185);
    private static readonly BaseColor ColorBlanco = new BaseColor(255, 255, 255);
    private static readonly BaseColor ColorGrisClaro = new BaseColor(236, 240, 241);
    private static readonly BaseColor ColorNegro = new BaseColor(0, 0, 0);

    public PDFReportService(ILogger<PDFReportService> logger, ReportFormatter formatter, AuditInfoHelper auditHelper)
    {
        _logger = logger;
        _formatter = formatter;
        _auditHelper = auditHelper;
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
            using var ms = new MemoryStream();
            var doc = new Document(PageSize.A4, 30, 30, 40, 50);
            var writer = PdfWriter.GetInstance(doc, ms);
            doc.Open();

            // Título con fondo azul usando tabla 1 celda
            var tblTitulo = new PdfPTable(1) { WidthPercentage = 100, SpacingAfter = 5 };
            var celdaTitulo = new PdfPCell(new Phrase("REPORTE DE CLIENTES Y VEHÍCULOS",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, ColorBlanco)))
            {
                BackgroundColor = ColorAzul,
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 8,
                Border = Rectangle.NO_BORDER
            };
            tblTitulo.AddCell(celdaTitulo);
            doc.Add(tblTitulo);

            // Filtros
            doc.Add(new Paragraph($"Filtros: Nombre/CI: {nombreCliente ?? "Todos"} | Marca: {marcaVehiculo ?? "Todas"}",
                FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 10))
            { SpacingAfter = 10 });

            // Tabla principal
            var table = new PdfPTable(8) { WidthPercentage = 100, SpacingBefore = 5 };
            table.SetWidths(new float[] { 5, 12, 18, 11, 11, 12, 8, 10 });

            string[] headers = { "Nro.", "CI/NIT", "Nombre Cliente", "Placa", "Marca", "Modelo", "Año", "Estado" };
            foreach (var h in headers)
            {
                table.AddCell(new PdfPCell(new Phrase(h, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, ColorBlanco)))
                {
                    BackgroundColor = ColorAzul,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5
                });
            }

            int nro = 1;
            bool altRow = false;
            if (reporteData?.clientes != null)
            {
                foreach (var cliente in reporteData.clientes)
                {
                    var bg = altRow ? ColorGrisClaro : ColorBlanco;
                    if (cliente.vehiculos != null && cliente.vehiculos.Count > 0)
                    {
                        bool first = true;
                        foreach (var v in cliente.vehiculos)
                        {
                            if (first)
                            {
                                AddCell(table, nro.ToString(), bg, Element.ALIGN_CENTER);
                                AddCell(table, (string?)(cliente.cedula ?? "N/A"), bg);
                                AddCell(table, _formatter.TruncateText((string?)(cliente.nombreCompleto ?? "N/A") ?? "", 22), bg);
                            }
                            else
                            {
                                AddCell(table, "", bg, Element.ALIGN_CENTER);
                                AddCell(table, "", bg);
                                AddCell(table, "", bg);
                            }
                            AddCell(table, (string?)(v.placa ?? "N/A"), bg);
                            AddCell(table, (string?)(v.marca ?? "N/A"), bg);
                            AddCell(table, (string?)(v.modelo ?? "N/A"), bg);
                            AddCell(table, v.anio?.ToString() ?? "N/A", bg, Element.ALIGN_CENTER);
                            AddCell(table, (string?)(v.estado ?? "Activo"), bg, Element.ALIGN_CENTER);
                            first = false;
                            nro++;
                        }
                    }
                    else
                    {
                        AddCell(table, nro.ToString(), bg, Element.ALIGN_CENTER);
                        AddCell(table, (string?)(cliente.cedula ?? "N/A"), bg);
                        AddCell(table, _formatter.TruncateText((string?)(cliente.nombreCompleto ?? "N/A") ?? "", 22), bg);
                        for (int i = 0; i < 5; i++) AddCell(table, "-", bg, Element.ALIGN_CENTER);
                        nro++;
                    }
                    altRow = !altRow;
                }
            }
            doc.Add(table);

            int totalClientes = reporteData?.clientes?.Count ?? 0;
            doc.Add(new Paragraph($"\nTotal clientes: {totalClientes}",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10)) { SpacingBefore = 8 });

            doc.Add(new Paragraph(infoAuditoria,
                FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8)) { SpacingBefore = 12 });

            doc.Close();
            _logger.LogInformation("PDF Clientes-Vehículos generado");
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando PDF Clientes-Vehículos");
            throw;
        }
    }

    public async Task<byte[]> GenerarReporteAnalyticaServiciosAsync(
        dynamic reporteData,
        DateTime fechaDesde,
        DateTime fechaHasta,
        byte[] graficoImg,
        string infoAuditoria)
    {
        await Task.Yield();
        try
        {
            using var ms = new MemoryStream();
            var doc = new Document(PageSize.A4, 30, 30, 40, 50);
            var writer = PdfWriter.GetInstance(doc, ms);
            doc.Open();

            // Título
            var tblTitulo = new PdfPTable(1) { WidthPercentage = 100, SpacingAfter = 5 };
            tblTitulo.AddCell(new PdfPCell(new Phrase("ANALÍTICA DE INGRESOS POR SERVICIOS",
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, ColorBlanco)))
            {
                BackgroundColor = ColorAzul,
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 8,
                Border = Rectangle.NO_BORDER
            });
            doc.Add(tblTitulo);

            doc.Add(new Paragraph(
                $"Período: {_formatter.FormatFecha(fechaDesde)} al {_formatter.FormatFecha(fechaHasta)}",
                FontFactory.GetFont(FontFactory.HELVETICA, 11)) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 15 });

            // Gráfico (PDF generado por ChartService)
            if (graficoImg != null && graficoImg.Length > 0)
            {
                var img = Image.GetInstance(graficoImg);
                img.ScaleToFit(400f, 280f);
                img.Alignment = Element.ALIGN_CENTER;
                doc.Add(img);
                doc.Add(new Paragraph("\n"));
            }

            // Tabla
            var table = new PdfPTable(4) { WidthPercentage = 100, SpacingBefore = 10 };
            table.SetWidths(new float[] { 40, 18, 22, 20 });

            foreach (var h in new[] { "Nombre del Servicio", "Cant. Atendida", "Total Recaudado", "Porcentaje" })
            {
                table.AddCell(new PdfPCell(new Phrase(h, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, ColorBlanco)))
                {
                    BackgroundColor = ColorAzul,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5
                });
            }

            decimal totalRecaudado = 0;
            int totalAtendidas = 0;
            decimal sumaTotal = 0;

            if (reporteData?.servicios != null)
            {
                foreach (var s in reporteData.servicios)
                    sumaTotal += (decimal)(s.TotalRecaudado ?? s.totalRecaudado ?? 0);

                bool altRow = false;
                foreach (var servicio in reporteData.servicios)
                {
                    decimal monto = servicio.TotalRecaudado ?? servicio.totalRecaudado ?? 0;
                    int cantidad = servicio.CantidadAtendida ?? servicio.cantidadAtendida ?? 0;
                    decimal pct = sumaTotal > 0 ? Math.Round(monto / sumaTotal * 100, 2) : 0;
                    var bg = altRow ? ColorGrisClaro : ColorBlanco;

                    AddCell(table, (string?)(servicio.NombreServicio ?? servicio.nombreServicio ?? "N/A"), bg);
                    AddCell(table, cantidad.ToString(), bg, Element.ALIGN_CENTER);
                    AddCell(table, _formatter.FormatMoneda(monto), bg, Element.ALIGN_RIGHT);
                    AddCell(table, $"{pct:F2}%", bg, Element.ALIGN_CENTER);

                    totalRecaudado += monto;
                    totalAtendidas += cantidad;
                    altRow = !altRow;
                }
            }

            // Fila total
            var colorTeal = new BaseColor(32, 201, 151);
            foreach (var val in new[] { "TOTAL", totalAtendidas.ToString(), _formatter.FormatMoneda(totalRecaudado), "100%" })
            {
                table.AddCell(new PdfPCell(new Phrase(val, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, ColorBlanco)))
                {
                    BackgroundColor = colorTeal,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5
                });
            }

            doc.Add(table);

            doc.Add(new Paragraph(infoAuditoria,
                FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8)) { SpacingBefore = 15, Alignment = Element.ALIGN_CENTER });

            doc.Close();
            _logger.LogInformation("PDF Analítica Servicios generado");
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando PDF Analítica Servicios");
            throw;
        }
    }

    private static void AddCell(PdfPTable table, string text, BaseColor bg, int alignment = Element.ALIGN_LEFT)
    {
        table.AddCell(new PdfPCell(new Phrase(text, FontFactory.GetFont(FontFactory.HELVETICA, 8)))
        {
            BackgroundColor = bg,
            HorizontalAlignment = alignment,
            Padding = 4
        });
    }
}
