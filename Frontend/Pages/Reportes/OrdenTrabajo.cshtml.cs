using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Taller_Mecanico_Arqui.Application.DTOs.OrdenTrabajo;
using Taller_Mecanico_Arqui.Application.Facades;

namespace Taller_Mecanico_Arqui.Pages.Reportes;

[Authorize]
public class OrdenTrabajoModel : PageModel
{
    private readonly OrdenTrabajoCreate _ordenFacade;

    public OrdenTrabajoModel(OrdenTrabajoCreate ordenFacade)
    {
        _ordenFacade = ordenFacade;
    }

    public OrdenTrabajoDetalleDto? Orden { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var result = await _ordenFacade.GetDetalleAsync(id);
        if (result.IsFailure)
        {
            return NotFound();
        }

        Orden = result.Value;
        return Page();
    }
}