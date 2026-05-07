using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Taller_Mecanico_Arqui.Frontend.Adapters;
using Taller_Mecanico_Arqui.Frontend.Authorization;

namespace Taller_Mecanico_Arqui.Pages
{
    [RequireAccessLevel(NivelAcceso.Completo)]
    public class UsuariosModel : PageModel
    {
        private readonly IUsersServiceAdapter _usersAdapter;
        private readonly IEmpleadoAdapter _empleadoAdapter;

        public UsuariosModel(IUsersServiceAdapter usersAdapter, IEmpleadoAdapter empleadoAdapter)
        {
            _usersAdapter = usersAdapter;
            _empleadoAdapter = empleadoAdapter;
        }

        public List<UserListDto> Usuarios { get; set; } = new();
        public List<EmpleadoListDto> Empleados { get; set; } = new();

        [BindProperty]
        public UsuarioFormDto FormDto { get; set; } = new();

        public async Task OnGetAsync()
        {
            Usuarios = await _usersAdapter.GetAllUsersAsync();
            Empleados = await _empleadoAdapter.GetAllAsync();
        }

        public async Task<JsonResult> OnGetUsuarioAsync(int id)
        {
            var usuario = await _usersAdapter.GetUserByIdAsync(id);
            if (usuario == null)
                return new JsonResult(new { error = "Usuario no encontrado." }) { StatusCode = 404 };

            return new JsonResult(new
            {
                usuarioLoginId = usuario.UsuarioLoginId,
                empleadoId = usuario.EmpleadoId,
                clienteId = usuario.ClienteId,
                email = usuario.Email,
                activo = usuario.Activo,
                requiereCambioPassword = usuario.RequiereCambioPassword,
                esCliente = usuario.EsCliente,
                ultimoAcceso = usuario.UltimoAcceso?.ToString("dd/MM/yyyy HH:mm") ?? "Nunca"
            });
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var (success, _, error) = await _usersAdapter.CreateUserAsync(new CreateUserDto
            {
                EmpleadoId = FormDto.EmpleadoId,
                Email = FormDto.Email
            });

            if (!success)
            {
                TempData["ErrorMessage"] = error ?? "No se pudo crear el usuario.";
                return RedirectToPage();
            }

            TempData["SuccessMessage"] = "Usuario creado exitosamente. Se envió la contraseña temporal al correo.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleActivoAsync(int id, bool activo)
        {
            var (success, error) = await _usersAdapter.ToggleActivoAsync(id, activo);
            if (!success)
                TempData["ErrorMessage"] = error ?? "No se pudo actualizar el estado del usuario.";
            else
                TempData["SuccessMessage"] = activo ? "Usuario activado." : "Usuario desactivado.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostResetPasswordAsync(int id)
        {
            var (success, error) = await _usersAdapter.ResetPasswordAsync(id);
            if (!success)
                TempData["ErrorMessage"] = error ?? "No se pudo restablecer la contraseña.";
            else
                TempData["SuccessMessage"] = "Contraseña restablecida. Se envió la nueva contraseña temporal al correo del usuario.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var (success, error) = await _usersAdapter.DeleteUserAsync(id);
            if (!success)
                TempData["ErrorMessage"] = error ?? "No se pudo eliminar el usuario.";
            else
                TempData["SuccessMessage"] = "Usuario eliminado correctamente.";

            return RedirectToPage();
        }
    }

    public class UsuarioFormDto
    {
        [Required(ErrorMessage = "Debe seleccionar un empleado.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un empleado válido.")]
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string Email { get; set; } = string.Empty;
    }
}
