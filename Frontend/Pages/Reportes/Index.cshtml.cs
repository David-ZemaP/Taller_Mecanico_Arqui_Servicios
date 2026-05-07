using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Taller_Mecanico_Arqui.Frontend.Authorization;

namespace Taller_Mecanico_Arqui.Pages.Reportes;

[RequireAccessLevel(NivelAcceso.Completo)]
public class IndexModel : PageModel
{
    public void OnGet() { }
}
