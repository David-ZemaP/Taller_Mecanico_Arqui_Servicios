using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebService.Pages
{
    // TODO: conectar con UsersService (S2) — obtener JWT y guardarlo en Session["JwtToken"]
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        [BindProperty]
        public LoginInput Input { get; set; } = new();

        public string ReturnUrl { get; set; } = "/";

        public void OnGet(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                ReturnUrl = returnUrl;
        }

        public Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ModelState.AddModelError(string.Empty, "El servicio de autenticación aún no está conectado.");
            return Task.FromResult<IActionResult>(Page());
        }
    }

    public class LoginInput
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
