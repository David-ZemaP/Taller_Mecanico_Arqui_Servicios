using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Wpf;
using System.IO;
using System.Text;

namespace Taller_Mecanico_WebService.Services.Reports;

/// <summary>
/// Servicio para generar gráficos estadísticos usando OxyPlot
/// Soporta pie charts, bar charts y line charts para reportes
/// </summary>
public class ChartService : IChartService
{
    private readonly ILogger<ChartService> _logger;
    private readonly ReportFormatter _formatter;

    public ChartService(ILogger<ChartService> logger, ReportFormatter formatter)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
    }

    /// <summary>
    /// Genera gráfico de pastel (pie chart) con distribución de servicios
    /// </summary>
    public async Task<byte[]> GenerarGraficoPastelAsync(
        Dictionary<string, decimal> datos,
        string titulo,
        int ancho = 600,
        int alto = 400)
    {
        try
        {
            _logger.LogInformation($"📊 Generando gráfico de pastel: {titulo}");

            var model = new PlotModel { Title = titulo };
            var pieSeries = new PieSeries();

            foreach (var item in datos.OrderByDescending(x => x.Value))
            {
                var porcentaje = (double)(item.Value / datos.Values.Sum() * 100);
                pieSeries.Slices.Add(new PieSlice 
                { 
                    Label = $"{item.Key} ({porcentaje:F1}%)", 
                    Value = (double)item.Value 
                });
            }

            model.Series.Add(pieSeries);

            var pngExporter = new PngExporter { Width = ancho, Height = alto };
            using (var stream = new MemoryStream())
            {
                pngExporter.Export(model, stream);
                _logger.LogInformation($"✅ Gráfico de pastel generado exitosamente");
                return stream.ToArray();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando gráfico de pastel");
            throw;
        }
    }

    /// <summary>
    /// Genera gráfico de barras con datos de servicios
    /// </summary>
    public async Task<byte[]> GenerarGraficoBarrasAsync(
        Dictionary<string, decimal> datos,
        string titulo,
        string ejeX,
        string ejeY,
        int ancho = 600,
        int alto = 400)
    {
        try
        {
            _logger.LogInformation($"📊 Generando gráfico de barras: {titulo}");

            var model = new PlotModel 
            { 
                Title = titulo,
                Background = OxyColor.FromRgb(255, 255, 255)
            };

            // Ejes
            model.Axes.Add(new OxyPlot.Axes.CategoryAxis 
            { 
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                Title = ejeX
            });

            model.Axes.Add(new OxyPlot.Axes.LinearAxis 
            { 
                Position = OxyPlot.Axes.AxisPosition.Left,
                Title = ejeY
            });

            // Series de barras
            var barSeries = new OxyPlot.Series.BarSeries();
            int index = 0;
            foreach (var item in datos)
            {
                var categoryAxis = model.Axes.OfType<OxyPlot.Axes.CategoryAxis>().First();
                categoryAxis.Labels.Add(item.Key);
                barSeries.Items.Add(new OxyPlot.Series.BarItem { Value = (double)item.Value });
                index++;
            }

            model.Series.Add(barSeries);

            var pngExporter = new PngExporter { Width = ancho, Height = alto };
            using (var stream = new MemoryStream())
            {
                pngExporter.Export(model, stream);
                _logger.LogInformation($"✅ Gráfico de barras generado exitosamente");
                return stream.ToArray();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando gráfico de barras");
            throw;
        }
    }

    /// <summary>
    /// Genera gráfico de líneas para tendencias temporales
    /// </summary>
    public async Task<byte[]> GenerarGraficoLineasAsync(
        Dictionary<string, decimal> datos,
        string titulo,
        string ejeX,
        string ejeY,
        int ancho = 600,
        int alto = 400)
    {
        try
        {
            _logger.LogInformation($"📊 Generando gráfico de líneas: {titulo}");

            var model = new PlotModel 
            { 
                Title = titulo,
                Background = OxyColor.FromRgb(255, 255, 255)
            };

            model.Axes.Add(new OxyPlot.Axes.LinearAxis 
            { 
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                Title = ejeX
            });

            model.Axes.Add(new OxyPlot.Axes.LinearAxis 
            { 
                Position = OxyPlot.Axes.AxisPosition.Left,
                Title = ejeY
            });

            var lineSeries = new OxyPlot.Series.LineSeries 
            { 
                Color = OxyColor.FromRgb(41, 128, 185)
            };

            double xValue = 1;
            foreach (var item in datos)
            {
                lineSeries.Points.Add(new DataPoint(xValue, (double)item.Value));
                xValue++;
            }

            model.Series.Add(lineSeries);

            var pngExporter = new PngExporter { Width = ancho, Height = alto };
            using (var stream = new MemoryStream())
            {
                pngExporter.Export(model, stream);
                _logger.LogInformation($"✅ Gráfico de líneas generado exitosamente");
                return stream.ToArray();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generando gráfico de líneas");
            throw;
        }
    }
}
