using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebService.Adapters;
using WebService.DTOs;

namespace WebService.Pages.Reportes;

[Authorize]
public class ReporteOrdenesModel : PageModel
{
    private readonly OrdenTrabajoAdapter _adapter;

    public ReporteOrdenesModel(OrdenTrabajoAdapter adapter)
    {
        _adapter = adapter;
    }

    [BindProperty(SupportsGet = true)]
    public DateTime? FechaDesde { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FechaHasta { get; set; }

    [BindProperty(SupportsGet = true)]
    public string EstadoPago { get; set; } = "";

    public List<OrdenTrabajoListDto> Ordenes { get; set; } = new();
    public double TotalGeneral { get; set; }

    public async Task OnGetAsync()
    {
        var todas = await _adapter.GetAllOrdenesAsync();
        Ordenes = AplicarFiltros(todas);
        TotalGeneral = Ordenes.Sum(o => o.Total);
    }

    public async Task<IActionResult> OnGetExcelAsync()
    {
        var todas = await _adapter.GetAllOrdenesAsync();
        var ordenes = AplicarFiltros(todas);

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Órdenes de Trabajo");

        ws.Cell(1, 1).Value = "TALLER MECÁNICO — PITSTOP";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;
        ws.Range(1, 1, 1, 8).Merge();

        ws.Cell(2, 1).Value = $"Reporte de Órdenes de Trabajo — Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
        ws.Range(2, 1, 2, 8).Merge();

        ws.Cell(4, 1).Value = "#";
        ws.Cell(4, 2).Value = "N° Orden";
        ws.Cell(4, 3).Value = "Fecha Ingreso";
        ws.Cell(4, 4).Value = "Fecha Entrega";
        ws.Cell(4, 5).Value = "Placa";
        ws.Cell(4, 6).Value = "Estado Trabajo";
        ws.Cell(4, 7).Value = "Estado Pago";
        ws.Cell(4, 8).Value = "Total (Bs.)";

        var headerRow = ws.Row(4);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromHtml("#20c997");
        headerRow.Style.Font.FontColor = XLColor.Black;

        int row = 5;
        int contador = 1;
        foreach (var o in ordenes)
        {
            ws.Cell(row, 1).Value = contador++;
            ws.Cell(row, 2).Value = o.OrdenTrabajoId;
            ws.Cell(row, 3).Value = o.FechaIngreso.ToString("dd/MM/yyyy");
            ws.Cell(row, 4).Value = o.FechaEntrega?.ToString("dd/MM/yyyy") ?? "-";
            ws.Cell(row, 5).Value = o.VehiculoPlaca;
            ws.Cell(row, 6).Value = o.EstadoTrabajo;
            ws.Cell(row, 7).Value = o.EstadoPago;
            ws.Cell(row, 8).Value = o.Total;
            ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";
            row++;
        }

        ws.Cell(row, 6).Value = "TOTAL GENERAL:";
        ws.Cell(row, 6).Style.Font.Bold = true;
        ws.Cell(row, 8).Value = ordenes.Sum(o => o.Total);
        ws.Cell(row, 8).Style.Font.Bold = true;
        ws.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00";
        ws.Cell(row, 8).Style.Fill.BackgroundColor = XLColor.FromHtml("#20c997");

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"ordenes_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    private List<OrdenTrabajoListDto> AplicarFiltros(List<OrdenTrabajoListDto> todas)
    {
        var query = todas.AsEnumerable();

        if (FechaDesde.HasValue)
            query = query.Where(o => o.FechaIngreso.Date >= FechaDesde.Value.Date);
        if (FechaHasta.HasValue)
            query = query.Where(o => o.FechaIngreso.Date <= FechaHasta.Value.Date);
        if (!string.IsNullOrEmpty(EstadoPago))
            query = query.Where(o => o.EstadoPago.Equals(EstadoPago, StringComparison.OrdinalIgnoreCase));

        return query.OrderByDescending(o => o.FechaIngreso).ToList();
    }
}
