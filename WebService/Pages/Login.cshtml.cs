using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Taller_Mecanico_WebService.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LoginModel> _logger;
        private readonly IConfiguration _config;

        public LoginModel(IHttpClientFactory httpClientFactory, ILogger<LoginModel> logger, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _config = config;
        }

        [BindProperty]
        public LoginInput Input { get; set; } = new();

        public string ReturnUrl { get; set; } = "/";

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/";
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/";

            if (!ModelState.IsValid)
                return Page();

            try
            {
                var baseUrl = _config["UsersServiceBaseUrl"] ?? "http://localhost:5297";
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(baseUrl);

                var response = await client.PostAsJsonAsync("/api/auth/login", new
                {
                    Email = Input.Email,
                    Password = Input.Password
                });

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError(string.Empty, "Correo electrónico o contraseña incorrectos.");
                    return Page();
                }

                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (loginResponse == null)
                {
                    ModelState.AddModelError(string.Empty, "Error al procesar la respuesta del servidor.");
                    return Page();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, Input.Email),
                    new Claim("Token", loginResponse.Token),
                    new Claim("RequiereCambio", loginResponse.RequiereCambioPassword.ToString()),
                    new Claim(ClaimTypes.Role, loginResponse.EsCliente ? "Cliente" : "Empleado"),
                    new Claim("NivelAcceso", loginResponse.NivelAcceso ?? "Parcial")
                };

                if (!string.IsNullOrEmpty(loginResponse.Nombre))
                    claims.Add(new Claim(ClaimTypes.Name, loginResponse.Nombre));

                if (loginResponse.UsuarioLoginId > 0)
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, loginResponse.UsuarioLoginId.ToString()));

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var props = new AuthenticationProperties { IsPersistent = Input.RememberMe };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), props);

                if (loginResponse.RequiereCambioPassword)
                    return RedirectToPage("/ChangePassword");

                return LocalRedirect(ReturnUrl);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "No se pudo conectar al servicio de autenticación");
                ModelState.AddModelError(string.Empty, "El servicio de autenticación no está disponible. Intenta nuevamente.");
                return Page();
            }
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

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public bool RequiereCambioPassword { get; set; }
        public bool EsCliente { get; set; }
        public string? NivelAcceso { get; set; }
        public string? Nombre { get; set; }
        public int UsuarioLoginId { get; set; }
    }
}
