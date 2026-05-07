using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Taller_Mecanico_WebService.Pages.Reportes;

[Authorize(Roles = "Empleado")]
public class IndexModel : PageModel
{
    public void OnGet() { }
}
