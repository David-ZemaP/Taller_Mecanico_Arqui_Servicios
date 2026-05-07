using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Taller_Mecanico_Arqui.Frontend.Adapters;

namespace Taller_Mecanico_Arqui.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IUsersServiceAdapter _usersAdapter;
        private readonly IClienteAdapter _clienteAdapter;
        private readonly IEmpleadoAdapter _empleadoAdapter;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            IUsersServiceAdapter usersAdapter,
            IClienteAdapter clienteAdapter,
            IEmpleadoAdapter empleadoAdapter,
            ILogger<LoginModel> logger)
        {
            _usersAdapter = usersAdapter;
            _clienteAdapter = clienteAdapter;
            _empleadoAdapter = empleadoAdapter;
            _logger = logger;
        }

        [BindProperty]
        public LoginInput Input { get; set; } = new();

        public string ReturnUrl { get; set; } = "/";

        public void OnGet(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                ReturnUrl = returnUrl;
            }
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? "/";

            if (!ModelState.IsValid)
                return Page();

            var authResult = await _usersAdapter.LoginAsync(Input.Email, Input.Password);

            if (authResult == null)
            {
                ModelState.AddModelError(string.Empty, "Correo electrónico o contraseña incorrectos.");
                return Page();
            }

            if (authResult.EsCliente)
            {
                return await HandleClienteLoginAsync(authResult);
            }
            else
            {
                return await HandleEmpleadoLoginAsync(authResult);
            }
        }

        private async Task<IActionResult> HandleClienteLoginAsync(AuthResponse authResult)
        {
            if (!authResult.ClienteId.HasValue)
            {
                _logger.LogWarning("Login de cliente sin ClienteId en el token para {Email}", Input.Email);
            }

            var clienteId = authResult.ClienteId?.ToString() ?? Input.Email;
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, clienteId),
                new Claim(ClaimTypes.Name, Input.Email),
                new Claim(ClaimTypes.Email, Input.Email),
                new Claim(ClaimTypes.Role, "Cliente"),
                new Claim("ClienteId", clienteId),
                new Claim("NivelAcceso", "Cliente")
            };

            var identity = new ClaimsIdentity(claims, "FrontendScheme");
            await HttpContext.SignInAsync("FrontendScheme", new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = Input.RememberMe });
            return RedirectToPage("/Clientes/Perfil");
        }

        private async Task<IActionResult> HandleEmpleadoLoginAsync(AuthResponse authResult)
        {
            if (!authResult.UserId.HasValue)
            {
                _logger.LogWarning("Login de empleado sin UserId en el token para {Email}", Input.Email);
            }

            var userId = authResult.UserId?.ToString() ?? Input.Email;
            var nivelAcceso = authResult.NivelAcceso ?? "Empleado"; // Default si no viene en el token
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, Input.Email),
                new Claim(ClaimTypes.Email, Input.Email),
                new Claim(ClaimTypes.Role, "Administrador"),
                new Claim("UserId", userId),
                new Claim("NivelAcceso", nivelAcceso)
            };

            var identity = new ClaimsIdentity(claims, "FrontendScheme");
            await HttpContext.SignInAsync("FrontendScheme", new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = Input.RememberMe });

            if (authResult.RequiereCambioPassword && Input.Email != "administrador.principal@taller.com")
            {
                return RedirectToPage("/ChangePassword");
            }

            return LocalRedirect(ReturnUrl);
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
