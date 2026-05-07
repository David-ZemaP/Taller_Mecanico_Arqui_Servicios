using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebService.Adapters;

namespace WebService.Pages
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly UsersServiceAdapter _usersService;

        public ChangePasswordModel(UsersServiceAdapter usersService)
        {
            _usersService = usersService;
        }

        [BindProperty]
        public ChangePasswordInput Input { get; set; } = new();

        public IActionResult OnGet() => Page();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var usuarioLoginId))
            {
                ModelState.AddModelError(string.Empty, "No se pudo identificar el usuario autenticado.");
                return Page();
            }

            var result = await _usersService.ChangePasswordAsync(
                usuarioLoginId,
                Input.CurrentPassword,
                Input.NewPassword,
                Input.ConfirmPassword);

            if (!result.ok)
            {
                ModelState.AddModelError(string.Empty, result.error ?? "No fue posible actualizar la contraseña.");
                return Page();
            }

            return RedirectToPage("/Index");
        }
    }

    public class ChangePasswordInput
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 20 caracteres.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la contraseña.")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
