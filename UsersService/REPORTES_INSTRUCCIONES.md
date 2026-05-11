# Instrucciones para implementar los Reportes — Taller Mecánico PITSTOP

Este documento contiene **todo el código exacto** para implementar los 2 reportes del 3er entregable en la rama destino. No requiere ningún endpoint nuevo en OrdenTrabajoService — los reportes consumen `IOrdenTrabajoAdapter.GetAllAsync()` que ya existe y filtran en el Frontend.

---

## Requisitos previos

### 1. Instalar NuGet en `Frontend/Frontend.csproj`

Agregar dentro del `<ItemGroup>` de PackageReferences:

```xml
<PackageReference Include="ClosedXML" Version="0.105.0" />
```

Ejecutar:
```powershell
dotnet restore Frontend
```

### 2. Verificar que estos elementos YA existen (no crear de nuevo)

- `Frontend/Adapters/IOrdenTrabajoAdapter.cs` tiene el método:
  ```csharp
  Task<List<OrdenTrabajoListDto>> GetAllAsync();
  ```
- `Frontend/DTOs/ordenTrabajo.cs` tiene la clase `OrdenTrabajoListDto` con propiedades:
  - `int OrdenTrabajoId`
  - `string VehiculoPlaca`
  - `DateTime FechaIngreso`
  - `DateTime? FechaEntrega`
  - `string EstadoTrabajo`
  - `string EstadoPago`
  - `double Total`

---

## Archivos a modificar

### Archivo 1 — `Frontend/Pages/Reportes/Index.cshtml.cs`

Reemplazar el contenido completo con:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Taller_Mecanico_Arqui.Frontend.Authorization;

namespace Taller_Mecanico_Arqui.Pages.Reportes;

[RequireAccessLevel(NivelAcceso.Completo)]
public class IndexModel : PageModel
{
    public void OnGet() { }
}
```

---

### Archivo 2 — `Frontend/Pages/Reportes/Index.cshtml`

Reemplazar el contenido completo con:

```razor
@page
@model Taller_Mecanico_Arqui.Pages.Reportes.IndexModel
@using Taller_Mecanico_Arqui.Frontend.Authorization
@attribute [RequireAccessLevel(NivelAcceso.Completo)]
@{
    ViewData["Title"] = "Reportes";
}

<div class="container mt-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h1 class="text-white">
            <i class="bi bi-graph-up me-2"></i>Reportes
        </h1>
    </div>

    <div class="row">
        <div class="col-md-6 mb-4">
            <div class="card shadow-lg h-100" style="background-color: var(--bg-card); border: 1px solid #2a2a3e; border-top: 4px solid #20c997;">
                <div class="card-body d-flex flex-column">
                    <div class="mb-3">
                        <i class="bi bi-clipboard-data" style="font-size: 2.5rem; color: var(--color-teal);"></i>
                    </div>
                    <h4 class="text-white mb-2">Reporte de Órdenes de Trabajo</h4>
                    <p class="text-muted flex-grow-1">
                        Lista detallada de órdenes filtrada por rango de fechas y estado de pago.
                        Exportación a PDF (impresión) y Excel.
                    </p>
                    <div class="d-flex gap-2 mt-3">
                        <a asp-page="/Reportes/ReporteOrdenes" class="btn btn-teal">
                            <i class="bi bi-eye me-1"></i>Ver Reporte
                        </a>
                        <a asp-page="/Reportes/ReporteOrdenes" asp-page-handler="Excel" class="btn btn-outline-success">
                            <i class="bi bi-file-earmark-excel me-1"></i>Excel
                        </a>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-md-6 mb-4">
            <div class="card shadow-lg h-100" style="background-color: var(--bg-card); border: 1px solid #2a2a3e; border-top: 4px solid #ffc107;">
                <div class="card-body d-flex flex-column">
                    <div class="mb-3">
                        <i class="bi bi-bar-chart-line" style="font-size: 2.5rem; color: #ffc107;"></i>
                    </div>
                    <h4 class="text-white mb-2">Resumen de Ingresos</h4>
                    <p class="text-muted flex-grow-1">
                        Análisis estadístico de ingresos mensuales con gráfico de barras.
                        Filtrable por año y estado del trabajo. Exportación a PDF y Excel.
                    </p>
                    <div class="d-flex gap-2 mt-3">
                        <a asp-page="/Reportes/ReporteResumen" class="btn btn-warning text-dark">
                            <i class="bi bi-eye me-1"></i>Ver Reporte
                        </a>
                        <a asp-page="/Reportes/ReporteResumen" asp-page-handler="Excel" class="btn btn-outline-success">
                            <i class="bi bi-file-earmark-excel me-1"></i>Excel
                        </a>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="alert mt-2" style="background-color: rgba(32, 201, 151, 0.1); border-color: rgba(32, 201, 151, 0.3); color: var(--color-teal);">
        <i class="bi bi-info-circle me-2"></i>
        <strong>Nivel de acceso:</strong> Solo disponible para Gerentes y Administradores
    </div>
</div>
```

---

## Archivos a CREAR

### Archivo 3 — `Frontend/Pages/Reportes/ReporteOrdenes.cshtml.cs` (NUEVO)

```csharp
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Taller_Mecanico_Arqui.Frontend.Adapters;
using Taller_Mecanico_Arqui.Frontend.DTOs.OrdenTrabajo;

namespace Taller_Mecanico_Arqui.Pages.Reportes;

[Authorize]
public class ReporteOrdenesModel : PageModel
{
    private readonly IOrdenTrabajoAdapter _ordenTrabajoAdapter;

    public ReporteOrdenesModel(IOrdenTrabajoAdapter ordenTrabajoAdapter)
    {
        _ordenTrabajoAdapter = ordenTrabajoAdapter;
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
        var todas = await _ordenTrabajoAdapter.GetAllAsync();
        Ordenes = AplicarFiltros(todas);
        TotalGeneral = Ordenes.Sum(o => o.Total);
    }

    public async Task<IActionResult> OnGetExcelAsync()
    {
        var todas = await _ordenTrabajoAdapter.GetAllAsync();
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
```

**NOTA sobre el namespace del DTO:** Si en la otra rama el DTO no está en `Taller_Mecanico_Arqui.Frontend.DTOs.OrdenTrabajo`, ajusta el `using` al namespace correcto donde esté `OrdenTrabajoListDto`.

---

### Archivo 4 — `Frontend/Pages/Reportes/ReporteOrdenes.cshtml` (NUEVO)

```razor
@page
@model Taller_Mecanico_Arqui.Pages.Reportes.ReporteOrdenesModel
@{
    ViewData["Title"] = "Reporte de Órdenes de Trabajo";
}

<style>
    @@media print {
        .no-print { display: none !important; }
        .card { background: white !important; color: black !important; border: 1px solid #ccc !important; }
        .text-white, .text-teal, .text-light { color: black !important; }
        .table-dark { background: white !important; color: black !important; }
        .table-dark td, .table-dark th { color: black !important; background: white !important; border-color: #ccc !important; }
        body { background: white !important; }
        header, footer { display: none !important; }
        h2 { color: #1a7a5e !important; }
        .badge { border: 1px solid #999 !important; color: black !important; background: #eee !important; }
        .fw-bold.text-teal { color: #1a7a5e !important; }
    }
</style>

<div class="no-print mb-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h2 class="text-teal fw-bold mb-0"><i class="bi bi-clipboard-data me-2"></i>Reporte de Órdenes de Trabajo</h2>
            <p class="text-muted">Listado detallado con filtros por fecha y estado de pago</p>
        </div>
        <a asp-page="/Reportes/Index" class="btn btn-outline-secondary text-white">
            <i class="bi bi-arrow-left me-1"></i>Volver a Reportes
        </a>
    </div>

    <div class="card shadow" style="background-color: var(--bg-card); border: 1px solid #2a2a3e;">
        <div class="card-body">
            <form method="get" class="row g-3 align-items-end">
                <div class="col-md-3">
                    <label class="form-label text-teal">Fecha Desde</label>
                    <input type="date" name="FechaDesde" value="@Model.FechaDesde?.ToString("yyyy-MM-dd")" class="form-control form-control-dark" />
                </div>
                <div class="col-md-3">
                    <label class="form-label text-teal">Fecha Hasta</label>
                    <input type="date" name="FechaHasta" value="@Model.FechaHasta?.ToString("yyyy-MM-dd")" class="form-control form-control-dark" />
                </div>
                <div class="col-md-3">
                    <label class="form-label text-teal">Estado de Pago</label>
                    <select name="EstadoPago" class="form-select form-control-dark">
                        <option value="">— Todos —</option>
                        <option value="Pendiente" selected="@(Model.EstadoPago == "Pendiente")">Pendiente</option>
                        <option value="Pagado" selected="@(Model.EstadoPago == "Pagado")">Pagado</option>
                        <option value="Cancelado" selected="@(Model.EstadoPago == "Cancelado")">Cancelado</option>
                    </select>
                </div>
                <div class="col-md-3 d-flex gap-2">
                    <button type="submit" class="btn btn-teal flex-grow-1">
                        <i class="bi bi-funnel me-1"></i>Filtrar
                    </button>
                    <a asp-page="ReporteOrdenes" class="btn btn-outline-secondary text-white">
                        <i class="bi bi-x-lg"></i>
                    </a>
                </div>
            </form>
        </div>
    </div>
</div>

<div class="card shadow mt-4" style="background-color: var(--bg-card); border: 1px solid #2a2a3e;">
    <div class="card-header d-flex justify-content-between align-items-center no-print" style="background-color: rgba(32,201,151,0.1); border-bottom: 1px solid #2a2a3e;">
        <span class="text-teal fw-bold">
            <i class="bi bi-list-ul me-2"></i>Resultados: @Model.Ordenes.Count orden(es)
        </span>
        <div class="d-flex gap-2">
            <button onclick="window.print()" class="btn btn-sm btn-outline-light">
                <i class="bi bi-printer me-1"></i>Imprimir / PDF
            </button>
            <a asp-page="ReporteOrdenes" asp-page-handler="Excel"
               asp-route-FechaDesde="@Model.FechaDesde?.ToString("yyyy-MM-dd")"
               asp-route-FechaHasta="@Model.FechaHasta?.ToString("yyyy-MM-dd")"
               asp-route-EstadoPago="@Model.EstadoPago"
               class="btn btn-sm btn-success">
                <i class="bi bi-file-earmark-excel me-1"></i>Exportar Excel
            </a>
        </div>
    </div>

    <div style="text-align:center; padding: 10px 0; display:none;" class="print-only">
        <h3 style="color:#1a7a5e; font-weight:bold;">TALLER MECÁNICO — PITSTOP</h3>
        <p style="color:#555;">Reporte de Órdenes de Trabajo — Generado: @DateTime.Now.ToString("dd/MM/yyyy HH:mm")</p>
    </div>

    <div class="table-responsive">
        <table class="table table-dark table-hover align-middle mb-0" style="background-color: transparent;">
            <thead class="border-bottom border-secondary">
                <tr>
                    <th class="text-teal pb-3">#</th>
                    <th class="text-teal pb-3">N° Orden</th>
                    <th class="text-teal pb-3">Fecha Ingreso</th>
                    <th class="text-teal pb-3">Fecha Entrega</th>
                    <th class="text-teal pb-3">Placa</th>
                    <th class="text-teal pb-3">Estado Trabajo</th>
                    <th class="text-teal pb-3">Estado Pago</th>
                    <th class="text-teal pb-3 text-end">Total (Bs.)</th>
                </tr>
            </thead>
            <tbody>
                @if (Model.Ordenes.Any())
                {
                    int i = 1;
                    foreach (var o in Model.Ordenes)
                    {
                        <tr class="border-bottom border-secondary">
                            <td><span class="badge bg-secondary">@(i++)</span></td>
                            <td class="text-white fw-bold">#@o.OrdenTrabajoId</td>
                            <td class="text-light">@o.FechaIngreso.ToString("dd/MM/yyyy")</td>
                            <td class="text-light">@(o.FechaEntrega?.ToString("dd/MM/yyyy") ?? "—")</td>
                            <td>
                                <span class="badge bg-light text-dark" style="font-family:monospace;">@o.VehiculoPlaca</span>
                            </td>
                            <td>
                                <span class="badge @(o.EstadoTrabajo == "Completado" ? "bg-success" : o.EstadoTrabajo == "Anulado" ? "bg-danger" : "bg-info text-dark")">
                                    @o.EstadoTrabajo
                                </span>
                            </td>
                            <td>
                                <span class="badge @(o.EstadoPago == "Pagado" ? "bg-success" : o.EstadoPago == "Cancelado" ? "bg-danger" : "bg-warning text-dark")">
                                    @o.EstadoPago
                                </span>
                            </td>
                            <td class="text-end text-white fw-bold">@o.Total.ToString("F2")</td>
                        </tr>
                    }
                }
                else
                {
                    <tr>
                        <td colspan="8" class="text-center text-muted py-5">
                            <i class="bi bi-clipboard2-x fs-2 d-block mb-2"></i>No hay órdenes con los filtros aplicados.
                        </td>
                    </tr>
                }
            </tbody>
            @if (Model.Ordenes.Any())
            {
                <tfoot>
                    <tr style="border-top: 2px solid #20c997;">
                        <td colspan="7" class="text-end fw-bold text-teal py-3">TOTAL GENERAL:</td>
                        <td class="text-end fw-bold text-white py-3 fs-5">Bs. @Model.TotalGeneral.ToString("F2")</td>
                    </tr>
                </tfoot>
            }
        </table>
    </div>
</div>
```

---

### Archivo 5 — `Frontend/Pages/Reportes/ReporteResumen.cshtml.cs` (NUEVO)

```csharp
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
```

---

### Archivo 6 — `Frontend/Pages/Reportes/ReporteResumen.cshtml` (NUEVO)

```razor
@page
@model Taller_Mecanico_Arqui.Pages.Reportes.ReporteResumenModel
@{
    ViewData["Title"] = "Resumen de Ingresos";
    var labelsJson = System.Text.Json.JsonSerializer.Serialize(Model.ResumenMensual.Select(m => m.NombreMes).ToList());
    var totalesJson = System.Text.Json.JsonSerializer.Serialize(Model.ResumenMensual.Select(m => m.Total).ToList());
    var cantidadesJson = System.Text.Json.JsonSerializer.Serialize(Model.ResumenMensual.Select(m => m.Cantidad).ToList());
}

<style>
    @@media print {
        .no-print { display: none !important; }
        .card { background: white !important; color: black !important; border: 1px solid #ccc !important; }
        .text-white, .text-teal, .text-light { color: black !important; }
        .table-dark { background: white !important; color: black !important; }
        .table-dark td, .table-dark th { color: black !important; background: white !important; border-color: #ccc !important; }
        body { background: white !important; }
        header, footer { display: none !important; }
        h2 { color: #1a7a5e !important; }
        .chart-container { page-break-inside: avoid; }
    }
</style>

<div class="no-print mb-4">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h2 class="text-teal fw-bold mb-0"><i class="bi bi-bar-chart-line me-2"></i>Resumen de Ingresos</h2>
            <p class="text-muted">Análisis estadístico mensual de ingresos del taller</p>
        </div>
        <a asp-page="/Reportes/Index" class="btn btn-outline-secondary text-white">
            <i class="bi bi-arrow-left me-1"></i>Volver a Reportes
        </a>
    </div>

    <div class="card shadow" style="background-color: var(--bg-card); border: 1px solid #2a2a3e;">
        <div class="card-body">
            <form method="get" class="row g-3 align-items-end">
                <div class="col-md-3">
                    <label class="form-label text-teal">Año</label>
                    <select name="Anio" class="form-select form-control-dark">
                        @for (int y = DateTime.Now.Year; y >= DateTime.Now.Year - 4; y--)
                        {
                            <option value="@y" selected="@(Model.Anio == y)">@y</option>
                        }
                    </select>
                </div>
                <div class="col-md-4">
                    <label class="form-label text-teal">Estado de Trabajo</label>
                    <select name="EstadoTrabajo" class="form-select form-control-dark">
                        <option value="">— Todos los estados —</option>
                        <option value="Recibido" selected="@(Model.EstadoTrabajo == "Recibido")">Recibido</option>
                        <option value="EnProceso" selected="@(Model.EstadoTrabajo == "EnProceso")">En Proceso</option>
                        <option value="Completado" selected="@(Model.EstadoTrabajo == "Completado")">Completado</option>
                        <option value="Anulado" selected="@(Model.EstadoTrabajo == "Anulado")">Anulado</option>
                    </select>
                </div>
                <div class="col-md-3 d-flex gap-2">
                    <button type="submit" class="btn btn-teal flex-grow-1">
                        <i class="bi bi-funnel me-1"></i>Aplicar
                    </button>
                    <a asp-page="ReporteResumen" class="btn btn-outline-secondary text-white">
                        <i class="bi bi-x-lg"></i>
                    </a>
                </div>
            </form>
        </div>
    </div>
</div>

<div class="row mt-4">
    <div class="col-md-3 mb-3">
        <div class="card shadow text-center py-3" style="background-color: rgba(32,201,151,0.15); border: 1px solid #20c997;">
            <div class="card-body">
                <h6 class="text-teal mb-1">Total Órdenes</h6>
                <h3 class="text-white fw-bold mb-0">@Model.TotalOrdenes</h3>
            </div>
        </div>
    </div>
    <div class="col-md-5 mb-3">
        <div class="card shadow text-center py-3" style="background-color: rgba(32,201,151,0.15); border: 1px solid #20c997;">
            <div class="card-body">
                <h6 class="text-teal mb-1">Total Ingresos @Model.Anio</h6>
                <h3 class="text-white fw-bold mb-0">Bs. @Model.TotalGeneral.ToString("F2")</h3>
            </div>
        </div>
    </div>
    <div class="col-md-4 mb-3 no-print">
        <div class="card shadow h-100 d-flex flex-column justify-content-center align-items-center py-3" style="background-color: var(--bg-card); border: 1px solid #2a2a3e;">
            <div class="d-flex gap-2">
                <button onclick="window.print()" class="btn btn-outline-light btn-sm">
                    <i class="bi bi-printer me-1"></i>Imprimir / PDF
                </button>
                <a asp-page="ReporteResumen" asp-page-handler="Excel"
                   asp-route-Anio="@Model.Anio"
                   asp-route-EstadoTrabajo="@Model.EstadoTrabajo"
                   class="btn btn-success btn-sm">
                    <i class="bi bi-file-earmark-excel me-1"></i>Excel
                </a>
            </div>
        </div>
    </div>
</div>

<div class="chart-container card shadow mt-2 mb-4" style="background-color: var(--bg-card); border: 1px solid #2a2a3e; padding: 20px;">
    <h5 class="text-teal mb-3"><i class="bi bi-bar-chart-fill me-2"></i>Ingresos Mensuales — @Model.Anio</h5>
    <canvas id="ingresosChart" style="max-height: 320px;"></canvas>
</div>

<div class="card shadow mt-2" style="background-color: var(--bg-card); border: 1px solid #2a2a3e;">
    <div class="card-header d-flex justify-content-between align-items-center" style="background-color: rgba(32,201,151,0.1); border-bottom: 1px solid #2a2a3e;">
        <span class="text-teal fw-bold"><i class="bi bi-table me-2"></i>Detalle Mensual</span>
    </div>
    <div class="table-responsive">
        <table class="table table-dark table-hover align-middle mb-0" style="background-color: transparent;">
            <thead class="border-bottom border-secondary">
                <tr>
                    <th class="text-teal pb-3">Mes</th>
                    <th class="text-teal pb-3 text-center">Cantidad de Órdenes</th>
                    <th class="text-teal pb-3 text-end">Total Ingresos (Bs.)</th>
                    <th class="text-teal pb-3 text-end">Promedio por Orden (Bs.)</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var m in Model.ResumenMensual)
                {
                    <tr class="border-bottom border-secondary @(m.Cantidad == 0 ? "opacity-50" : "")">
                        <td class="text-white">@m.NombreMes</td>
                        <td class="text-center">
                            @if (m.Cantidad > 0)
                            {
                                <span class="badge bg-info text-dark">@m.Cantidad</span>
                            }
                            else
                            {
                                <span class="text-muted">—</span>
                            }
                        </td>
                        <td class="text-end @(m.Total > 0 ? "text-white fw-bold" : "text-muted")">
                            @(m.Total > 0 ? $"Bs. {m.Total:F2}" : "—")
                        </td>
                        <td class="text-end text-light">
                            @(m.Cantidad > 0 ? $"Bs. {(m.Total / m.Cantidad):F2}" : "—")
                        </td>
                    </tr>
                }
            </tbody>
            <tfoot>
                <tr style="border-top: 2px solid #20c997;">
                    <td class="fw-bold text-teal py-3">TOTAL</td>
                    <td class="text-center fw-bold text-white py-3">@Model.TotalOrdenes</td>
                    <td class="text-end fw-bold text-white py-3 fs-5">Bs. @Model.TotalGeneral.ToString("F2")</td>
                    <td class="text-end text-light py-3">
                        @(Model.TotalOrdenes > 0 ? $"Bs. {(Model.TotalGeneral / Model.TotalOrdenes):F2}" : "—")
                    </td>
                </tr>
            </tfoot>
        </table>
    </div>
</div>

@section Scripts {
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
    <script>
        const labels = @Html.Raw(labelsJson);
        const totales = @Html.Raw(totalesJson);
        const cantidades = @Html.Raw(cantidadesJson);

        const ctx = document.getElementById('ingresosChart').getContext('2d');
        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Ingresos (Bs.)',
                        data: totales,
                        backgroundColor: 'rgba(32, 201, 151, 0.75)',
                        borderColor: '#20c997',
                        borderWidth: 1,
                        yAxisID: 'y'
                    },
                    {
                        label: 'Cantidad de Órdenes',
                        data: cantidades,
                        type: 'line',
                        backgroundColor: 'rgba(255, 193, 7, 0.2)',
                        borderColor: '#ffc107',
                        borderWidth: 2,
                        pointBackgroundColor: '#ffc107',
                        pointRadius: 5,
                        yAxisID: 'y1'
                    }
                ]
            },
            options: {
                responsive: true,
                interaction: { mode: 'index', intersect: false },
                plugins: {
                    legend: { labels: { color: '#e0e0e0' } },
                    tooltip: {
                        callbacks: {
                            label: function(ctx) {
                                if (ctx.datasetIndex === 0)
                                    return ` Bs. ${ctx.parsed.y.toFixed(2)}`;
                                return ` ${ctx.parsed.y} órdenes`;
                            }
                        }
                    }
                },
                scales: {
                    x: { ticks: { color: '#aaa' }, grid: { color: 'rgba(255,255,255,0.05)' } },
                    y: {
                        type: 'linear',
                        position: 'left',
                        ticks: { color: '#20c997', callback: v => 'Bs. ' + v.toFixed(0) },
                        grid: { color: 'rgba(32,201,151,0.1)' }
                    },
                    y1: {
                        type: 'linear',
                        position: 'right',
                        ticks: { color: '#ffc107' },
                        grid: { drawOnChartArea: false }
                    }
                }
            }
        });
    </script>
}
```

---

## Posibles ajustes según la otra rama

### Si el namespace del DTO es diferente

Busca dónde está definida `OrdenTrabajoListDto` en la otra rama:
```powershell
grep -rn "class OrdenTrabajoListDto" Frontend/
```
Y reemplaza el `using` en los dos `.cshtml.cs`:
```csharp
// Cambiar esto:
using Taller_Mecanico_Arqui.Frontend.DTOs.OrdenTrabajo;
// Por el namespace correcto que encuentres
```

### Si `OrdenTrabajoListDto` no tiene `FechaEntrega`

En `ReporteOrdenes.cshtml.cs`, cambiar:
```csharp
ws.Cell(row, 4).Value = o.FechaEntrega?.ToString("dd/MM/yyyy") ?? "-";
```
Por:
```csharp
ws.Cell(row, 4).Value = "-";
```
Y en `ReporteOrdenes.cshtml`, cambiar:
```razor
<td class="text-light">@(o.FechaEntrega?.ToString("dd/MM/yyyy") ?? "—")</td>
```
Por:
```razor
<td class="text-muted">—</td>
```

### Si `NivelAcceso.Completo` no existe

En `Index.cshtml.cs`, cambiar `[RequireAccessLevel(NivelAcceso.Completo)]` por `[Authorize]`.
En `Index.cshtml`, eliminar las líneas `@using` y `@attribute`.

### Si `var(--bg-card)` y `btn-teal` no existen en el CSS

Son variables del tema oscuro del proyecto. Si no existen, reemplazar:
- `var(--bg-card)` → `#1e1e2e`
- `btn-teal` → `btn-success`
- `form-control-dark` → `form-control`

---

## Verificación rápida

Después de implementar, compilar:
```powershell
dotnet build Frontend
```

Luego navegar a:
- `http://localhost:5146/Reportes` — ver las 2 tarjetas
- `http://localhost:5146/Reportes/ReporteOrdenes` — filtrar y exportar Excel
- `http://localhost:5146/Reportes/ReporteResumen` — ver gráfico y exportar Excel
