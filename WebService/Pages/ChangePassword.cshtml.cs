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
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<ChangePasswordModel> _logger;

        public ChangePasswordModel(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<ChangePasswordModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        [BindProperty]
        public ChangePasswordInput Input { get; set; } = new();

        public IActionResult OnGet()
        {
            var requiereCambio = User.FindFirst("RequiereCambio")?.Value;
            if (requiereCambio != "True" && requiereCambio != "true")
                return RedirectToPage("/Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (Input.NewPassword != Input.ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Las contraseñas nuevas no coinciden.");
                return Page();
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                ModelState.AddModelError(string.Empty, "No se pudo identificar al usuario.");
                return Page();
            }

            try
            {
                var baseUrl = _config["UsersServiceBaseUrl"] ?? "http://localhost:5297";
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(baseUrl);

                var token = User.FindFirst("Token")?.Value ?? "";
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.PostAsJsonAsync($"/api/users/{userId}/change-password", new
                {
                    CurrentPassword = Input.CurrentPassword,
                    NewPassword = Input.NewPassword,
                    ConfirmPassword = Input.ConfirmPassword
                });

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    ModelState.AddModelError(string.Empty, error?.Message ?? "No se pudo actualizar la contraseña.");
                    return Page();
                }

                // Re-issue cookie with RequiereCambio=False so middleware doesn't loop
                var newClaims = User.Claims
                    .Where(c => c.Type != "RequiereCambio")
                    .Append(new Claim("RequiereCambio", "False"))
                    .ToList();
                var identity = new ClaimsIdentity(newClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                var props = new AuthenticationProperties { IsPersistent = User.Identity is { IsAuthenticated: true } };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), props);

                TempData["SuccessMessage"] = "Contraseña actualizada exitosamente.";
                return RedirectToPage("/Index");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Error conectando al servicio de usuarios");
                ModelState.AddModelError(string.Empty, "El servicio no está disponible.");
                return Page();
            }
        }
    }

    public class ChangePasswordInput
    {
        [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_\-+=\[\]{};':""\\|,.<>\/?]).{8,}$",
            ErrorMessage = "La contraseña debe tener al menos 8 caracteres, una mayúscula, una minúscula, un número y un carácter especial.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la nueva contraseña.")]
        [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ErrorResponse
    {
        public string? Message { get; set; }
    }
}
