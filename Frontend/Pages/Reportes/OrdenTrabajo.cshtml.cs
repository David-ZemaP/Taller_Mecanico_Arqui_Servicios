using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Taller_Mecanico_Arqui.Frontend.Adapters;
using Taller_Mecanico_Arqui.Frontend.DTOs.OrdenTrabajo;

namespace Taller_Mecanico_Arqui.Pages.Reportes;

[Authorize]
public class OrdenTrabajoModel : PageModel
{
    private readonly IOrdenTrabajoAdapter _ordenTrabajoAdapter;

    public OrdenTrabajoModel(IOrdenTrabajoAdapter ordenTrabajoAdapter)
    {
        _ordenTrabajoAdapter = ordenTrabajoAdapter;
    }

    public OrdenTrabajoDetalleDto? Orden { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Orden = await _ordenTrabajoAdapter.GetByIdAsync(id);
        if (Orden == null)
        {
            return NotFound();
        }

        return Page();
    }
}