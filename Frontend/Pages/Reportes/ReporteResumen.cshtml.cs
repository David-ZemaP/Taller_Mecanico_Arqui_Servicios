using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Taller_Mecanico_Arqui.Frontend.Adapters;
using Taller_Mecanico_Arqui.Frontend.DTOs.OrdenTrabajo;

namespace Taller_Mecanico_Arqui.Pages.Reportes;

[Authorize]
public class ReporteResumenModel : PageModel
{
    private readonly IOrdenTrabajoAdapter _ordenTrabajoAdapter;

    private static readonly string[] NombresMeses =
    { "", "Enero", "Febrero", "Marzo", "Abril", "Mayo", "Junio",
      "Julio", "Agosto", "Septiembre", "Octubre", "Noviembre", "Diciembre" };

    public ReporteResumenModel(IOrdenTrabajoAdapter ordenTrabajoAdapter)
    {
        _ordenTrabajoAdapter = ordenTrabajoAdapter;
    }

    [BindProperty(SupportsGet = true)]
    public int Anio { get; set; } = DateTime.Now.Year;

    [BindProperty(SupportsGet = true)]
    public string EstadoTrabajo { get; set; } = "";

    public List<ResumenMensualDto> ResumenMensual { get; set; } = new();
    public double TotalGeneral { get; set; }
    public int TotalOrdenes { get; set; }

    public async Task OnGetAsync()
    {
        var todas = await _ordenTrabajoAdapter.GetAllAsync();
        CalcularResumen(todas);
    }

    public async Task<IActionResult> OnGetExcelAsync()
    {
        var todas = await _ordenTrabajoAdapter.GetAllAsync();
        CalcularResumen(todas);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Resumen de Ingresos");

        ws.Cell(1, 1).Value = "TALLER MECÁNICO — PITSTOP";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Range(1, 1, 1, 4).Merge();

        ws.Cell(2, 1).Value = $"Resumen de Ingresos — Año {Anio}" + (string.IsNullOrEmpty(EstadoTrabajo) ? "" : $" — Estado: {EstadoTrabajo}");
        ws.Cell(2, 1).Style.Font.Italic = true;
        ws.Range(2, 1, 2, 4).Merge();

        ws.Cell(3, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws.Range(3, 1, 3, 4).Merge();

        ws.Cell(5, 1).Value = "Mes";
        ws.Cell(5, 2).Value = "Cantidad de Órdenes";
        ws.Cell(5, 3).Value = "Total Ingresos (Bs.)";
        ws.Cell(5, 4).Value = "Promedio por Orden (Bs.)";

        var headerRow = ws.Row(5);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#20c997");
        headerRow.Style.Font.FontColor = XLColor.Black;

        int row = 6;
        foreach (var m in ResumenMensual)
        {
            ws.Cell(row, 1).Value = m.NombreMes;
            ws.Cell(row, 2).Value = m.Cantidad;
            ws.Cell(row, 3).Value = m.Total;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
            ws.Cell(row, 4).Value = m.Cantidad > 0 ? Math.Round(m.Total / m.Cantidad, 2) : 0;
            ws.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }

        ws.Cell(row, 1).Value = "TOTAL";
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 2).Value = TotalOrdenes;
        ws.Cell(row, 2).Style.Font.Bold = true;
        ws.Cell(row, 3).Value = TotalGeneral;
        ws.Cell(row, 3).Style.Font.Bold = true;
        ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 3).Style.Fill.BackgroundColor = XLColor.FromHtml("#20c997");

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"resumen_ingresos_{Anio}.xlsx");
    }

    private void CalcularResumen(List<OrdenTrabajoListDto> todas)
    {
        var query = todas.AsEnumerable().Where(o => o.FechaIngreso.Year == Anio);

        if (!string.IsNullOrEmpty(EstadoTrabajo))
            query = query.Where(o => o.EstadoTrabajo.Equals(EstadoTrabajo, StringComparison.OrdinalIgnoreCase));

        var grouped = query.GroupBy(o => o.FechaIngreso.Month)
            .ToDictionary(g => g.Key, g => new { Cantidad = g.Count(), Total = g.Sum(x => x.Total) });

        ResumenMensual = Enumerable.Range(1, 12).Select(mes => new ResumenMensualDto
        {
            Mes = mes,
            NombreMes = NombresMeses[mes],
            Cantidad = grouped.ContainsKey(mes) ? grouped[mes].Cantidad : 0,
            Total = grouped.ContainsKey(mes) ? grouped[mes].Total : 0
        }).ToList();

        TotalGeneral = ResumenMensual.Sum(m => m.Total);
        TotalOrdenes = ResumenMensual.Sum(m => m.Cantidad);
    }
}

public class ResumenMensualDto
{
    public int Mes { get; set; }
    public string NombreMes { get; set; } = "";
    public int Cantidad { get; set; }
    public double Total { get; set; }
}
