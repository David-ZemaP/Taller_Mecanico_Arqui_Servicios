using System.ComponentModel.DataAnnotations;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Taller_Mecanico_Arqui.Domain.Ports;

namespace Taller_Mecanico_Arqui.Pages
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly IUsuarioLoginRepository _loginRepository;

        public ChangePasswordModel(IUsuarioLoginRepository loginRepository)
        {
            _loginRepository = loginRepository;
        }

        [BindProperty]
        public ChangePasswordInput Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var login = await FindLoginAsync();
            if (login == null)
                return RedirectToPage("/Login");

            if (!login.RequiereCambioPassword)
                return RedirectToPage("/Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var login = await FindLoginAsync();
            if (login == null)
                return RedirectToPage("/Login");

            if (!login.RequiereCambioPassword)
                return RedirectToPage("/Index");

            login.CambiarPassword(BCrypt.Net.BCrypt.HashPassword(Input.NewPassword));
            var updateResult = await _loginRepository.UpdateAsync(login);
            if (updateResult.IsFailure)
            {
                ModelState.AddModelError(string.Empty, updateResult.ErrorMessage ?? "No se pudo actualizar la contraseña.");
                return Page();
            }

            TempData["SuccessMessage"] = "Contraseña actualizada exitosamente.";
            return RedirectToPage("/Clientes/Perfil");
        }

        private async Task<Domain.Entities.UsuarioLogin?> FindLoginAsync()
        {
            var allLogins = await _loginRepository.GetAllAsync();
            
            var clienteId = User.FindFirst("ClienteId")?.Value;
            if (!string.IsNullOrEmpty(clienteId) && int.TryParse(clienteId, out int cId))
                return allLogins.FirstOrDefault(l => l.ClienteId == cId);
            
            var empleadoId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(empleadoId) && int.TryParse(empleadoId, out int eId))
                return allLogins.FirstOrDefault(l => l.EmpleadoId == eId);
            
            return null;
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
