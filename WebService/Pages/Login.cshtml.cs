using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Npgsql;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Domain.Entities;

namespace Taller_Mecanico_Arqui.Pages
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IUsuarioLoginRepository _loginRepository;
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            IUsuarioLoginRepository loginRepository,
            IEmpleadoRepository empleadoRepository,
            IClienteRepository clienteRepository,
            ILogger<LoginModel> logger)
        {
            _loginRepository = loginRepository;
            _empleadoRepository = empleadoRepository;
            _clienteRepository = clienteRepository;
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

            UsuarioLogin? usuario;
            try
            {
                usuario = await _loginRepository.GetByEmailAsync(Input.Email);
            }
            catch (NpgsqlException ex)
            {
                _logger.LogWarning(ex, "No se pudo conectar a PostgreSQL durante el inicio de sesion para {Email}.", Input.Email);
                ModelState.AddModelError(string.Empty, "La base de datos no esta disponible en este momento. Intenta nuevamente en unos minutos.");
                return Page();
            }

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(Input.Password, usuario.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Correo electronico o contrasena incorrectos.");
                return Page();
            }

            if (usuario.EsCliente)
            {
                return await HandleClienteLoginAsync(usuario);
            }
            else
            {
                return await HandleEmpleadoLoginAsync(usuario);
            }
        }

        private async Task<IActionResult> HandleClienteLoginAsync(UsuarioLogin usuario)
        {
            var clienteResult = await _clienteRepository.GetByIdAsync(usuario.ClienteId!.Value);
            if (clienteResult.IsFailure)
            {
                ModelState.AddModelError(string.Empty, clienteResult.ErrorMessage ?? "No se pudo validar el cliente.");
                return Page();
            }

            var cliente = clienteResult.Value;
            if (cliente == null)
            {
                ModelState.AddModelError(string.Empty, "Cliente no encontrado.");
                return Page();
            }

            usuario.RegistrarAcceso();
            await _loginRepository.UpdateAsync(usuario);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.ClienteId.ToString()!),
                new Claim(ClaimTypes.Name, cliente.NombreCompleto.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, "Cliente"),
                new Claim("NivelAcceso", "Cliente"),
                new Claim("ClienteId", usuario.ClienteId.ToString()!)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = Input.RememberMe });
            return RedirectToPage("/Clientes/Perfil");
        }

        private async Task<IActionResult> HandleEmpleadoLoginAsync(UsuarioLogin usuario)
        {
            var empleadoResult = await _empleadoRepository.GetByIdAsync(usuario.EmpleadoId!.Value);
            if (empleadoResult.IsFailure)
            {
                ModelState.AddModelError(string.Empty, empleadoResult.ErrorMessage ?? "No se pudo validar el empleado.");
                return Page();
            }

            var empleado = empleadoResult.Value;
            if (empleado == null || empleado is not Administrador admin)
            {
                ModelState.AddModelError(string.Empty, "Solo los administradores pueden acceder al sistema.");
                return Page();
            }

            usuario.RegistrarAcceso();
            await _loginRepository.UpdateAsync(usuario);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.EmpleadoId.ToString()!),
                new Claim(ClaimTypes.Name, empleado.NombreCompleto.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, "Administrador"),
                new Claim("NivelAcceso", admin.NivelAcceso.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), new AuthenticationProperties { IsPersistent = Input.RememberMe });

            if (usuario.RequiereCambioPassword && usuario.Email != "administrador.principal@taller.com")
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
