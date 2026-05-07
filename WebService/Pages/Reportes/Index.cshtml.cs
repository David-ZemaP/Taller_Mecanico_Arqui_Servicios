using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebService.Pages.Reportes;

[Authorize]
public class IndexModel : PageModel
{
    public void OnGet() { }
}
