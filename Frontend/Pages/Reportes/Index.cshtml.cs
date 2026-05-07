using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Taller_Mecanico_Arqui.Infrastructure.Authorization;
using Taller_Mecanico_Arqui.Domain.Enums;

namespace Taller_Mecanico_Arqui.Pages.Reportes;

[RequireAccessLevel(NivelAcceso.Gerente)]
public class IndexModel : PageModel
{
    public void OnGet()
    {
        // Placeholder - no logic needed yet for the construction page
    }
}
