using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebService.Adapters;
using WebService.DTOs;
using WebService.Models;

namespace WebService.Pages
{
    [Authorize]
    public class UsuariosModel : PageModel
    {
        private readonly EmpleadosAdapter _adapter;

        public List<UsuarioViewModel> Usuarios { get; set; } = new();
        public List<SelectListItem> AdministradoresSelect { get; set; } = new();
        public NivelAcceso CurrentUserLevel { get; set; }
        public string? NuevoEmail { get; set; }
        public string? NuevaPassword { get; set; }

        [BindProperty]
        public UsuarioFormDto FormDto { get; set; } = new();

        public UsuariosModel(EmpleadosAdapter adapter)
        {
            _adapter = adapter;
        }

        public async Task OnGetAsync()
        {
            NuevoEmail = TempData["NuevoEmail"] as string;
            NuevaPassword = TempData["NuevaPassword"] as string;
            await CargarDatosAsync();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (FormDto.UsuarioLoginId == 0 && string.IsNullOrWhiteSpace(FormDto.Password))
                ModelState.AddModelError("FormDto.Password", "La contraseña es obligatoria al crear un usuario.");

            if (!ModelState.IsValid)
            {
                await CargarDatosAsync();
                return Page();
            }

            if (FormDto.UsuarioLoginId == 0)
            {
                var (ok, password, error) = await _adapter.CreateUsuarioAsync(FormDto.EmpleadoId, FormDto.Email, FormDto.Password);
                if (!ok)
                {
                    ModelState.AddModelError(string.Empty, error ?? "No se pudo crear el usuario.");
                    await CargarDatosAsync();
                    return Page();
                }
                if (!string.IsNullOrEmpty(password))
                {
                    TempData["NuevoEmail"] = FormDto.Email;
                    TempData["NuevaPassword"] = password;
                }
                TempData["SuccessMessage"] = "Usuario creado correctamente.";
            }
            else
            {
                var (ok, error) = await _adapter.UpdateUsuarioAsync(FormDto.UsuarioLoginId, FormDto.Email, true);
                if (!ok)
                {
                    ModelState.AddModelError(string.Empty, error ?? "No se pudo actualizar el usuario.");
                    await CargarDatosAsync();
                    return Page();
                }
                TempData["SuccessMessage"] = "Usuario actualizado correctamente.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleActivoAsync(int id)
        {
            var (ok, user, error) = await _adapter.GetUsuarioByIdAsync(id);
            if (!ok || user is null)
            {
                TempData["ErrorMessage"] = error ?? "Usuario no encontrado.";
                return RedirectToPage();
            }

            var newActivo = !user.Activo;
            var (updateOk, updateError) = await _adapter.UpdateUsuarioAsync(id, user.Email, newActivo);
            if (!updateOk)
                TempData["ErrorMessage"] = updateError ?? "No se pudo actualizar el estado.";
            else
                TempData["SuccessMessage"] = newActivo ? "Usuario activado." : "Usuario desactivado.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostResetPasswordAsync(int id)
        {
            var (ok, password, error) = await _adapter.ResetPasswordAsync(id);
            if (!ok)
                TempData["ErrorMessage"] = error ?? "No se pudo restablecer la contraseña.";
            else
            {
                TempData["NewPassword"] = password;
                TempData["SuccessMessage"] = "Contraseña restablecida. La contraseña temporal se muestra abajo. El usuario deberá cambiarla en su primer inicio de sesión.";
            }
            return RedirectToPage();
        }

        private async Task CargarDatosAsync()
        {
            var claim = User.FindFirst("NivelAcceso");
            CurrentUserLevel = claim != null && Enum.TryParse<NivelAcceso>(claim.Value, out var lvl)
                ? lvl : NivelAcceso.Parcial;

            var (empOk, empleados, _) = await _adapter.GetAllEmpleadosAsync();
            var (usrOk, usuarios, _) = await _adapter.GetAllUsuariosAsync();

            var empList = (empleados ?? Enumerable.Empty<EmpleadoDto>()).ToList();
            var empDict = empList.ToDictionary(e => e.EmpleadoId);

            Usuarios = (usuarios ?? Enumerable.Empty<UsuarioDto>()).Select(u => new UsuarioViewModel
            {
                UsuarioLoginId = u.UsuarioLoginId,
                Email = u.Email,
                EmpleadoId = u.EmpleadoId ?? 0,
                EmpleadoNombre = u.EsCliente
                    ? "CLIENTE"
                    : (u.EmpleadoId.HasValue && empDict.TryGetValue(u.EmpleadoId.Value, out var emp)
                        ? emp.NombreCompleto : "No disponible"),
                UltimoAcceso = u.UltimoAcceso,
                Activo = u.Activo,
                AdminNivelAcceso = u.EsCliente
                    ? NivelAcceso.Cliente
                    : ResolveAdminLevel(u.EmpleadoId, empDict)
            }).ToList();

            var admins = empList.Where(e => e.TipoEmpleado == "Administrador");
            if (CurrentUserLevel == NivelAcceso.Completo)
                admins = admins.Where(e => e.NivelAcceso == "Parcial");

            AdministradoresSelect = admins
                .Select(e => new SelectListItem(
                    $"{e.NombreCompleto} ({e.NivelAcceso ?? "—"})",
                    e.EmpleadoId.ToString()))
                .ToList();
        }

        private static NivelAcceso ResolveAdminLevel(int? empleadoId, Dictionary<int, EmpleadoDto> empDict)
        {
            if (!empleadoId.HasValue) return NivelAcceso.Parcial;
            if (!empDict.TryGetValue(empleadoId.Value, out var emp)) return NivelAcceso.Parcial;
            if (emp.TipoEmpleado != "Administrador") return NivelAcceso.Parcial;
            return emp.NivelAcceso != null && Enum.TryParse<NivelAcceso>(emp.NivelAcceso, out var lvl)
                ? lvl : NivelAcceso.Parcial;
        }
    }
}
