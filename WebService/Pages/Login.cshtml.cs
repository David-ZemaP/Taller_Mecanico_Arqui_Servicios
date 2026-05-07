using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebService.Adapters;

namespace WebService.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UsersServiceAdapter _usersService;

        public LoginModel(UsersServiceAdapter usersService)
        {
            _usersService = usersService;
        }

        [BindProperty]
        public LoginInput Input { get; set; } = new();

        public string ReturnUrl { get; set; } = "/";

        public void OnGet(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _usersService.LoginAsync(Input.Email, Input.Password);
            if (!result.ok || result.response is null || string.IsNullOrWhiteSpace(result.response.Token))
            {
                ModelState.AddModelError(string.Empty, result.error ?? "No fue posible iniciar sesión.");
                return Page();
            }

            HttpContext.Session.SetString("JwtToken", result.response.Token);

            var claims = new List<Claim>();
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.response.Token);
            claims.AddRange(jwt.Claims);

            var emailClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(emailClaim))
            {
                emailClaim = Input.Email;
            }

            if (!claims.Any(c => c.Type == ClaimTypes.Name))
            {
                claims.Add(new Claim(ClaimTypes.Name, emailClaim));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = Input.RememberMe
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            if (result.response.RequiereCambioPassword)
            {
                return RedirectToPage("/ChangePassword");
            }

            var targetUrl = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : ReturnUrl;

            return LocalRedirect(targetUrl);
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
