using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Taller_Mecanico_Arqui.Pages
{
    public class UsuariosModel : PageModel
    {
        public IActionResult OnGet()
        {
            TempData["ErrorMessage"] = "La gestión de usuarios via web no está disponible actualmente. Esta funcionalidad requiere endpoints API en UsersService.";
            return RedirectToPage("/Index");
        }

        public IActionResult OnPostSaveAsync()
        {
            TempData["ErrorMessage"] = "La gestión de usuarios via web no está disponible actualmente. Esta funcionalidad requiere endpoints API en UsersService.";
            return RedirectToPage("/Index");
        }

        public IActionResult OnPostToggleActivoAsync(int id)
        {
            TempData["ErrorMessage"] = "La gestión de usuarios via web no está disponible actualmente. Esta funcionalidad requiere endpoints API en UsersService.";
            return RedirectToPage("/Index");
        }

        public IActionResult OnPostResetPasswordAsync(int id)
        {
            TempData["ErrorMessage"] = "La gestión de usuarios via web no está disponible actualmente. Esta funcionalidad requiere endpoints API en UsersService.";
            return RedirectToPage("/Index");
        }
    }
}
