using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebService.Pages
{
    // TODO: conectar con UsersService (S2) para cambio de contraseña
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        [BindProperty]
        public ChangePasswordInput Input { get; set; } = new();

        public IActionResult OnGet() => Page();

        public IActionResult OnPost()
        {
            ModelState.AddModelError(string.Empty, "El servicio de usuarios aún no está conectado.");
            return Page();
        }
    }

    public class ChangePasswordInput
    {
        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 20 caracteres.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
