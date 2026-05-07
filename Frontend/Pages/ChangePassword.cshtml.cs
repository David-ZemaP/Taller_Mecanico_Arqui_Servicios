using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Taller_Mecanico_Arqui.Frontend.Adapters;

namespace Taller_Mecanico_Arqui.Pages
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly IUsersServiceAdapter _usersAdapter;

        public ChangePasswordModel(IUsersServiceAdapter usersAdapter)
        {
            _usersAdapter = usersAdapter;
        }

        [BindProperty]
        public ChangePasswordInput Input { get; set; } = new();

        public IActionResult OnGet() => Page();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                ModelState.AddModelError(string.Empty, "No se pudo identificar el usuario. Inicia sesión nuevamente.");
                return Page();
            }

            var (success, error) = await _usersAdapter.ChangePasswordAsync(
                userId, Input.CurrentPassword, Input.NewPassword, Input.ConfirmPassword);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "No se pudo cambiar la contraseña.");
                return Page();
            }

            TempData["SuccessMessage"] = "Contraseña cambiada exitosamente.";
            return RedirectToPage("/Index");
        }
    }

    public class ChangePasswordInput
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme la nueva contraseña.")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
