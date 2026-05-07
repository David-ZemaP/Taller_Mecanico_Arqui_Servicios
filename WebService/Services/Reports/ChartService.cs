using iTextSharp.text;
using iTextSharp.text.pdf;
using Taller_Mecanico_WebService.Helpers;

namespace Taller_Mecanico_WebService.Services.Reports;

public class ChartService : IChartService
{
    private readonly ILogger<ChartService> _logger;
    private readonly ReportFormatter _formatter;

    public ChartService(ILogger<ChartService> logger, ReportFormatter formatter)
    {
        _logger = logger;
        _formatter = formatter;
    }

    public async Task<byte[]> GenerarGraficoPastelAsync(Dictionary<string, decimal> datos, string titulo, int ancho = 600, int alto = 400)
    {
        return await DibujarGraficoBarrasITextAsync(datos, titulo, ancho, alto);
    }

    public async Task<byte[]> GenerarGraficoBarrasAsync(Dictionary<string, decimal> datos, string titulo, string ejeX, string ejeY, int ancho = 600, int alto = 400)
    {
        return await DibujarGraficoBarrasITextAsync(datos, titulo, ancho, alto);
    }

    public async Task<byte[]> GenerarGraficoLineasAsync(Dictionary<string, decimal> datos, string titulo, string ejeX, string ejeY, int ancho = 600, int alto = 400)
    {
        return await DibujarGraficoBarrasITextAsync(datos, titulo, ancho, alto);
    }

    private async Task<byte[]> DibujarGraficoBarrasITextAsync(Dictionary<string, decimal> datos, string titulo, int ancho, int alto)
    {
        await Task.Yield();
        try
        {
            using var ms = new MemoryStream();
            var doc = new Document(new Rectangle(ancho, alto), 20, 20, 30, 20);
            var writer = PdfWriter.GetInstance(doc, ms);
            doc.Open();

            var cb = writer.DirectContent;
            var teal = new BaseColor(32, 201, 151);
            var dark = new BaseColor(26, 26, 46);
            var white = new BaseColor(255, 255, 255);
            var gray = new BaseColor(100, 100, 120);

            // Fondo
            cb.SetColorFill(dark);
            cb.Rectangle(0, 0, ancho, alto);
            cb.Fill();

            // Título
            var font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 13, white);
            var titlePhrase = new Phrase(titulo, font);
            ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER, titlePhrase, ancho / 2f, alto - 25, 0);

            if (datos == null || datos.Count == 0)
            {
                doc.Close();
                return ms.ToArray();
            }

            decimal maxVal = datos.Values.Max();
            if (maxVal == 0) maxVal = 1;

            float marginLeft = 60f;
            float marginRight = 20f;
            float marginBottom = 50f;
            float marginTop = 50f;
            float chartWidth = ancho - marginLeft - marginRight;
            float chartHeight = alto - marginBottom - marginTop;

            float barWidth = (chartWidth / datos.Count) * 0.6f;
            float barSpacing = chartWidth / datos.Count;
            float barStartX = marginLeft + barSpacing * 0.2f;

            int i = 0;
            foreach (var kv in datos)
            {
                float barHeight = (float)(kv.Value / maxVal) * chartHeight;
                float x = barStartX + i * barSpacing;
                float y = marginBottom;

                // Barra
                cb.SetColorFill(teal);
                cb.Rectangle(x, y, barWidth, barHeight);
                cb.Fill();

                // Valor encima de la barra
                var valFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8, white);
                ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER,
                    new Phrase(_formatter.FormatMoneda(kv.Value), valFont),
                    x + barWidth / 2, y + barHeight + 5, 0);

                // Etiqueta debajo
                var labelFont = FontFactory.GetFont(FontFactory.HELVETICA, 7, white);
                var label = kv.Key.Length > 12 ? kv.Key.Substring(0, 12) + "..." : kv.Key;
                ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER,
                    new Phrase(label, labelFont),
                    x + barWidth / 2, marginBottom - 15, 45);

                i++;
            }

            // Eje Y
            cb.SetColorStroke(gray);
            cb.MoveTo(marginLeft, marginBottom);
            cb.LineTo(marginLeft, marginBottom + chartHeight);
            cb.Stroke();

            // Eje X
            cb.MoveTo(marginLeft, marginBottom);
            cb.LineTo(marginLeft + chartWidth, marginBottom);
            cb.Stroke();

            doc.Close();
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando gráfico");
            return Array.Empty<byte>();
        }
    }
}
