using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Taller_Mecanico_WebService.Pages
{
    [Authorize(Roles = "Empleado")]
    public class UsuariosModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<UsuariosModel> _logger;

        public UsuariosModel(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<UsuariosModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        public List<UsuarioVm> Usuarios { get; set; } = new();
        public string? MensajeExito { get; set; }
        public string? MensajeError { get; set; }

        [BindProperty]
        public CreateUserForm FormCrear { get; set; } = new();

        public async Task OnGetAsync()
        {
            MensajeExito = TempData["Exito"] as string;
            await CargarUsuariosAsync();
        }

        public async Task<IActionResult> OnPostCrearAsync()
        {
            if (!ModelState.IsValid)
            {
                await CargarUsuariosAsync();
                return Page();
            }

            try
            {
                var client = GetClient();
                var response = await client.PostAsJsonAsync("/api/users", new
                {
                    Nombres = FormCrear.Nombres,
                    PrimerApellido = FormCrear.PrimerApellido,
                    SegundoApellido = FormCrear.SegundoApellido,
                    Email = FormCrear.Email,
                    Nivel = FormCrear.Nivel,
                    EmpleadoId = FormCrear.EmpleadoId,
                    EsCliente = false
                });

                if (response.IsSuccessStatusCode)
                {
                    TempData["Exito"] = "Usuario creado exitosamente. Las credenciales fueron enviadas por correo.";
                    return RedirectToPage();
                }

                var error = await response.Content.ReadFromJsonAsync<ErrorVm>();
                MensajeError = error?.Message ?? "Error al crear usuario.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando usuario");
                MensajeError = "No se pudo conectar al servicio.";
            }

            await CargarUsuariosAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostEliminarAsync(int id)
        {
            try
            {
                var client = GetClient();
                var response = await client.DeleteAsync($"/api/users/{id}");

                if (response.IsSuccessStatusCode)
                    TempData["Exito"] = "Usuario desactivado correctamente.";
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorVm>();
                    TempData["Error"] = error?.Message ?? "Error al eliminar.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando usuario {Id}", id);
                TempData["Error"] = "No se pudo conectar al servicio.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostResetPasswordAsync(int id)
        {
            try
            {
                var client = GetClient();
                var response = await client.PostAsync($"/api/users/{id}/reset-password", null);

                if (response.IsSuccessStatusCode)
                    TempData["Exito"] = "Contraseña reiniciada. Credenciales enviadas al usuario.";
                else
                {
                    var error = await response.Content.ReadFromJsonAsync<ErrorVm>();
                    TempData["Error"] = error?.Message ?? "Error al reiniciar contraseña.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reseteando contraseña del usuario {Id}", id);
                TempData["Error"] = "No se pudo conectar al servicio.";
            }

            return RedirectToPage();
        }

        private async Task CargarUsuariosAsync()
        {
            try
            {
                var client = GetClient();
                var response = await client.GetAsync("/api/users");
                if (response.IsSuccessStatusCode)
                {
                    var lista = await response.Content.ReadFromJsonAsync<List<UsuarioVm>>();
                    Usuarios = lista ?? new();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error cargando usuarios");
                MensajeError = "No se pudieron cargar los usuarios del servicio.";
            }
        }

        private HttpClient GetClient()
        {
            var baseUrl = _config["UsersServiceBaseUrl"] ?? "http://localhost:5297";
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUrl);
            var token = User.FindFirst("Token")?.Value ?? "";
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }

    public class UsuarioVm
    {
        public int UsuarioLoginId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public bool RequiereCambioPassword { get; set; }
        public bool EsCliente { get; set; }
        public DateTime? UltimoAcceso { get; set; }
    }

    public class CreateUserForm
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "El primer apellido es obligatorio.")]
        public string PrimerApellido { get; set; } = string.Empty;

        public string? SegundoApellido { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Correo inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio.")]
        public string Nivel { get; set; } = string.Empty;

        public int? EmpleadoId { get; set; }
    }

    public class ErrorVm
    {
        public string? Message { get; set; }
    }
}
