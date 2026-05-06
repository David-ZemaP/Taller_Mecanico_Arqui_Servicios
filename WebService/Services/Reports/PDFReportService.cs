using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace Taller_Mecanico_WebService.Services.Reports;

/// <summary>
/// Servicio para generar reportes en formato PDF con iTextSharp
/// Incluye estilos corporativos, logo y auditoría
/// </summary>
public class PDFReportService : IPDFReportService
{
    private readonly ILogger<PDFReportService> _logger;
    private readonly ReportFormatter _formatter;
    private readonly AuditInfoHelper _auditHelper;

    public PDFReportService(ILogger<PDFReportService> logger, ReportFormatter formatter, AuditInfoHelper auditHelper)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        _auditHelper = auditHelper ?? throw new ArgumentNullException(nameof(auditHelper));
    }

    /// <summary>
    /// Genera reporte PDF de Clientes y Vehículos
    /// Incluye tabla con datos filtrados y pie de página con auditoría
    /// </summary>
    public async Task<byte[]> GenerarReporteClientesVehiculosAsync(
        dynamic reporteData,
        string nombreCliente,
        string marcaVehiculo,
        string infoAuditoria)
    {
        try
        {
            _logger.LogInformation($"🔴 Generando PDF: Reporte Clientes y Vehículos");

            using (var memoryStream = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 30, 30, 40, 50);
                var writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                // Encabezado principal
                var titulo = new Paragraph("REPORTE DE CLIENTES Y VEHÍCULOS", 
                    new Font(Font.FontFamily.HELVETICA, 18, Font.BOLD, BaseColor.WHITE))
                {
                    Alignment = Element.ALIGN_CENTER,
                    BackgroundColor = new BaseColor(41, 128, 185),
                    SpacingBefore = 10,
                    SpacingAfter = 10,
                    IndentationLeft = 10,
                    IndentationRight = 10
                };
                document.Add(titulo);

                // Filtros aplicados
                var filtros = new Paragraph($"Filtros aplicados: Nombre/CI: {nombreCliente ?? "N/A"} | Marca: {marcaVehiculo ?? "Todas"}", 
                    new Font(Font.FontFamily.HELVETICA, 10))
                {
                    Alignment = Element.ALIGN_LEFT,
                    SpacingAfter = 15
                };
                document.Add(filtros);

                // Tabla principal
                var table = new PdfPTable(8)
                {
                    WidthPercentage = 100
                };
                table.SetWidths(new float[] { 6, 12, 18, 12, 12, 12, 10, 12 });

                // Encabezado tabla
                string[] headerTexts = { "Nro.", "CI/NIT", "Nombre Cliente", "Placa", "Marca", "Modelo", "Año", "Estado" };
                foreach (var headerText in headerTexts)
                {
                    var cell = new PdfPCell(new Phrase(headerText, new Font(Font.FontFamily.HELVETICA, 11, Font.BOLD, BaseColor.WHITE)))
                    {
                        BackgroundColor = new BaseColor(41, 128, 185),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 8
                    };
                    table.AddCell(cell);
                }

                // Datos
                int nro = 1;
                if (reporteData?.clientes != null)
                {
                    foreach (var cliente in reporteData.clientes)
                    {
                        table.AddCell(new PdfPCell(new Phrase(nro.ToString())) { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(cliente.cedula ?? "N/A")));
                        table.AddCell(new PdfPCell(new Phrase(_formatter.TruncateText(cliente.nombreCompleto ?? "N/A", 25))));

                        if (cliente.vehiculos != null && cliente.vehiculos.Count > 0)
                        {
                            var primeraFila = true;
                            foreach (var vehiculo in cliente.vehiculos)
                            {
                                if (!primeraFila)
                                {
                                    table.AddCell(""); // Nro
                                    table.AddCell(""); // CI
                                    table.AddCell(""); // Nombre
                                }
                                table.AddCell(new PdfPCell(new Phrase(vehiculo.placa ?? "N/A")));
                                table.AddCell(new PdfPCell(new Phrase(vehiculo.marca ?? "N/A")));
                                table.AddCell(new PdfPCell(new Phrase(vehiculo.modelo ?? "N/A")));
                                table.AddCell(new PdfPCell(new Phrase(vehiculo.anio?.ToString() ?? "N/A")));
                                table.AddCell(new PdfPCell(new Phrase(vehiculo.estado ?? "Activo")));
                                primeraFila = false;
                                nro++;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < 5; i++)
                                table.AddCell("");
                        }
                    }
                }

                document.Add(table);

                // Totales
                document.Add(new Paragraph("\n", new Font(Font.FontFamily.HELVETICA, 8)));

                var totalClientes = reporteData?.clientes?.Count ?? 0;
                var totalVehiculos = reporteData?.clientes?.Sum(c => c.vehiculos?.Count ?? 0) ?? 0;

                var totales = new Paragraph(
                    $"TOTAL CLIENTES FILTRADOS: {totalClientes}                TOTAL VEHÍCULOS: {totalVehiculos}",
                    new Font(Font.FontFamily.HELVETICA, 11, Font.BOLD))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingBefore = 10,
                    SpacingAfter = 10
                };
                document.Add(totales);

                // Pie de página
                var piePagina = new Paragraph(infoAuditoria, new Font(Font.FontFamily.HELVETICA, 9))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(piePagina);

                document.Close();
                writer.Close();

                _logger.LogInformation($"✅ PDF generado exitosamente");
                return memoryStream.ToArray();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando PDF de Clientes y Vehículos");
            throw;
        }
    }

    /// <summary>
    /// Genera reporte PDF de Analítica de Servicios con gráfico
    /// </summary>
    public async Task<byte[]> GenerarReporteAnalyticaServiciosAsync(
        dynamic reporteData,
        DateTime fechaDesde,
        DateTime fechaHasta,
        byte[] graficoImg,
        string infoAuditoria)
    {
        try
        {
            _logger.LogInformation($"🔴 Generando PDF: Reporte Analítica de Servicios");

            using (var memoryStream = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 30, 30, 40, 50);
                var writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                // Encabezado
                var titulo = new Paragraph("ANALÍTICA DE INGRESOS POR SERVICIOS",
                    new Font(Font.FontFamily.HELVETICA, 18, Font.BOLD, BaseColor.WHITE))
                {
                    Alignment = Element.ALIGN_CENTER,
                    BackgroundColor = new BaseColor(41, 128, 185),
                    SpacingBefore = 10,
                    SpacingAfter = 10,
                    IndentationLeft = 10,
                    IndentationRight = 10
                };
                document.Add(titulo);

                // Rango de fechas
                var rangoFechas = new Paragraph(
                    $"Rango de Fechas: Desde {_formatter.FormatFecha(fechaDesde)} Al {_formatter.FormatFecha(fechaHasta)}",
                    new Font(Font.FontFamily.HELVETICA, 11))
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 20
                };
                document.Add(rangoFechas);

                // Gráfico
                if (graficoImg != null && graficoImg.Length > 0)
                {
                    var image = Image.GetInstance(graficoImg);
                    image.ScaleToFit(400f, 300f);
                    image.Alignment = Element.ALIGN_CENTER;
                    document.Add(image);
                    document.Add(new Paragraph("\n"));
                }

                // Tabla de datos
                var table = new PdfPTable(3)
                {
                    WidthPercentage = 100,
                    SpacingBefore = 20
                };
                table.SetWidths(new float[] { 40, 20, 40 });

                // Encabezado tabla
                string[] headers = { "Nombre del Servicio", "Cant. Atendida", "Total Recaudado" };
                foreach (var header in headers)
                {
                    var cell = new PdfPCell(new Phrase(header, new Font(Font.FontFamily.HELVETICA, 11, Font.BOLD, BaseColor.WHITE)))
                    {
                        BackgroundColor = new BaseColor(41, 128, 185),
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 8
                    };
                    table.AddCell(cell);
                }

                // Datos servicios
                decimal totalRecaudado = 0;
                if (reporteData?.servicios != null)
                {
                    foreach (var servicio in reporteData.servicios)
                    {
                        table.AddCell(new PdfPCell(new Phrase(servicio.nombreServicio ?? "N/A")));
                        table.AddCell(new PdfPCell(new Phrase(servicio.cantidadAtendida?.ToString() ?? "0")) 
                        { 
                            HorizontalAlignment = Element.ALIGN_CENTER 
                        });
                        var monto = servicio.totalRecaudado ?? 0;
                        table.AddCell(new PdfPCell(new Phrase(_formatter.FormatMoneda(monto)))
                        {
                            HorizontalAlignment = Element.ALIGN_RIGHT
                        });
                        totalRecaudado += monto;
                    }
                }

                document.Add(table);

                // Total recaudado
                document.Add(new Paragraph("\n"));
                var totalParagraph = new Paragraph(
                    $"TOTAL RECAUDADO EN EL PERÍODO:                {_formatter.FormatMoneda(totalRecaudado)}",
                    new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD))
                {
                    Alignment = Element.ALIGN_RIGHT,
                    SpacingBefore = 10,
                    SpacingAfter = 20
                };
                document.Add(totalParagraph);

                // Pie de página
                var piePagina = new Paragraph(infoAuditoria, new Font(Font.FontFamily.HELVETICA, 9))
                {
                    Alignment = Element.ALIGN_CENTER
                };
                document.Add(piePagina);

                document.Close();
                writer.Close();

                _logger.LogInformation($"✅ PDF de Analítica generado exitosamente");
                return memoryStream.ToArray();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando PDF de Analítica de Servicios");
            throw;
        }
    }
}
