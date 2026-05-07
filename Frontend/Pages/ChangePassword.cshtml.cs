using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Taller_Mecanico_Arqui.Pages
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        public ChangePasswordModel()
        {
        }

        public IActionResult OnGet()
        {
            TempData["ErrorMessage"] = "El cambio de contraseña via web no está disponible actualmente. Esta funcionalidad requiere un endpoint API en UsersService.";
            return RedirectToPage("/Index");
        }

        public IActionResult OnPost()
        {
            TempData["ErrorMessage"] = "El cambio de contraseña via web no está disponible actualmente. Esta funcionalidad requiere un endpoint API en UsersService.";
            return RedirectToPage("/Index");
        }
    }
}
