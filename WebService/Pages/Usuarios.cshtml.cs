using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Infrastructure.Authorization;
using Taller_Mecanico_Arqui.Domain.Enums;
using Taller_Mecanico_Arqui.Application.Common;

namespace Taller_Mecanico_Arqui.Pages
{
    [RequireAccessLevel(NivelAcceso.Completo, AllowedLevels = new[] { NivelAcceso.Completo, NivelAcceso.Gerente })]
    public class UsuariosModel : PageModel
    {
        private readonly IUsuarioLoginRepository _loginRepository;
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly Taller_Mecanico_Arqui.Infrastructure.Services.AuthenticationHelper _authHelper;
        private readonly ICredentialEmailSender _emailSender;

        public UsuariosModel(
            IUsuarioLoginRepository loginRepository, 
            IEmpleadoRepository empleadoRepository,
            Taller_Mecanico_Arqui.Infrastructure.Services.AuthenticationHelper authHelper,
            ICredentialEmailSender emailSender)
        {
            _loginRepository = loginRepository;
            _empleadoRepository = empleadoRepository;
            _authHelper = authHelper;
            _emailSender = emailSender;
        }

        public List<UsuarioViewModel> Usuarios { get; set; } = new();
        public List<SelectListItem> AdministradoresSelect { get; set; } = new();
        public NivelAcceso CurrentUserLevel { get; set; }

        public string? NuevoEmail { get; set; }
        public string? NuevaPassword { get; set; }

        [BindProperty]
        public UsuarioFormDto FormDto { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Recuperar credenciales del TempData si existen
            NuevoEmail = TempData["NuevoEmail"] as string;
            NuevaPassword = TempData["NuevaPassword"] as string;
            
            await CargarDatosAsync();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                await CargarDatosAsync();
                return Page();
            }

            // Check if user can create/modify this admin
            var empleadoResult = await _empleadoRepository.GetByIdAsync(FormDto.EmpleadoId);
            if (empleadoResult.IsFailure)
            {
                ModelState.AddModelError(string.Empty, empleadoResult.ErrorMessage ?? "No se pudo consultar el empleado seleccionado.");
                await CargarDatosAsync();
                return Page();
            }

            var empleado = empleadoResult.Value;
            if (empleado is Administrador admin)
            {
                if (FormDto.UsuarioLoginId == 0)
                {
                    var canCreateValidation = ValidationHelper.RequireCanCreateAdmin(_authHelper.CanCreateAdmin(admin.NivelAcceso), admin.NivelAcceso);
                    if (canCreateValidation.IsFailure)
                    {
                        ModelState.AddModelError(string.Empty, canCreateValidation.ErrorMessage ?? $"No tienes permisos para crear usuarios para administradores con nivel {admin.NivelAcceso}.");
                        await CargarDatosAsync();
                        return Page();
                    }
                }
                else
                {
                    var canModifyValidation = ValidationHelper.RequireCanModifyAdmin(_authHelper.CanModifyAdmin(admin.NivelAcceso), admin.NivelAcceso);
                    if (canModifyValidation.IsFailure)
                    {
                        ModelState.AddModelError(string.Empty, canModifyValidation.ErrorMessage ?? $"No tienes permisos para modificar usuarios de administradores con nivel {admin.NivelAcceso}.");
                        await CargarDatosAsync();
                        return Page();
                    }
                }
            }

            if (FormDto.UsuarioLoginId == 0)
            {
                // Guardar credenciales en TempData para mostrar en el modal
                TempData["NuevoEmail"] = FormDto.Email;
                TempData["NuevaPassword"] = FormDto.Password;
                
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(FormDto.Password);
                var nuevoLogin = UsuarioLogin.Crear(FormDto.EmpleadoId, FormDto.Email, passwordHash);
                var addResult = await _loginRepository.AddAsync(nuevoLogin);
                if (addResult.IsFailure)
                {
                    ModelState.AddModelError(string.Empty, addResult.ErrorMessage ?? "No se pudo crear el usuario.");
                    await CargarDatosAsync();
                    return Page();
                }

                // Enviar credenciales por correo
                var empResult = await _empleadoRepository.GetByIdAsync(FormDto.EmpleadoId);
                if (empResult.IsSuccess)
                {
                    var emailResult = await _emailSender.SendCredentialsAsync(FormDto.Email, empResult.Value!.NombreCompleto!.ToString(), FormDto.Password);
                    if (emailResult.IsFailure)
                    {
                        TempData["EmailWarning"] = "Usuario creado, pero no se pudo enviar el correo: " + emailResult.ErrorMessage;
                    }
                }

                TempData["SuccessMessage"] = "Usuario creado correctamente.";
            }
            else
            {
                var existente = await _loginRepository.GetByEmailAsync(FormDto.Email);
                if (existente != null && existente.UsuarioLoginId != FormDto.UsuarioLoginId)
                {
                    ModelState.AddModelError("FormDto.Email", "Este correo electrónico ya está en uso.");
                    await CargarDatosAsync();
                    return Page();
                }

                var login = await GetByIdAsync(FormDto.UsuarioLoginId);
                if (login != null)
                {
                    login.CambiarEmail(FormDto.Email);
                    var updateResult = await _loginRepository.UpdateAsync(login);
                    if (updateResult.IsFailure)
                    {
                        ModelState.AddModelError(string.Empty, updateResult.ErrorMessage ?? "No se pudo actualizar el usuario.");
                        await CargarDatosAsync();
                        return Page();
                    }
                    TempData["SuccessMessage"] = "Usuario actualizado correctamente.";
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleActivoAsync(int id)
        {
            var login = await GetByIdAsync(id);
            if (login != null)
            {
                if (login.EsCliente)
                {
                    TempData["ErrorMessage"] = "No se puede modificar el estado de usuarios de clientes desde esta página.";
                    return RedirectToPage();
                }

                var empleadoResult = await _empleadoRepository.GetByIdAsync(login.EmpleadoId!.Value);
                if (empleadoResult.IsFailure)
                {
                    TempData["ErrorMessage"] = empleadoResult.ErrorMessage;
                    return RedirectToPage();
                }

                var empleado = empleadoResult.Value;
                if (empleado is Administrador admin)
                {
                    if (!_authHelper.CanModifyAdmin(admin.NivelAcceso))
                    {
                        TempData["ErrorMessage"] =
                            $"No tienes permisos para modificar usuarios de administradores con nivel {admin.NivelAcceso}.";
                        return RedirectToPage();
                    }
                }

                if (login.Activo) login.Desactivar();
                else login.Activar();
                var updateResult = await _loginRepository.UpdateAsync(login);
                if (updateResult.IsFailure)
                {
                    TempData["ErrorMessage"] = updateResult.ErrorMessage;
                    return RedirectToPage();
                }
                TempData["SuccessMessage"] = login.Activo ? "Usuario activado." : "Usuario desactivado.";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostResetPasswordAsync(int id)
        {
            var login = await GetByIdAsync(id);
            if (login != null)
            {
                if (login.EsCliente)
                {
                    TempData["ErrorMessage"] = "No se puede restablecer la contraseña de clientes desde esta página.";
                    return RedirectToPage();
                }

                var empleadoResult = await _empleadoRepository.GetByIdAsync(login.EmpleadoId!.Value);
                if (empleadoResult.IsFailure)
                {
                    TempData["ErrorMessage"] = empleadoResult.ErrorMessage;
                    return RedirectToPage();
                }

                var empleado = empleadoResult.Value;
                if (empleado is Administrador admin)
                {
                    if (!_authHelper.CanModifyAdmin(admin.NivelAcceso))
                    {
                        TempData["ErrorMessage"] =
                            $"No tienes permisos para restablecer contraseñas de administradores con nivel {admin.NivelAcceso}.";
                        return RedirectToPage();
                    }
                }

                var tempPassword = GenerateRandomPassword(10);
                login.ResetearPassword(BCrypt.Net.BCrypt.HashPassword(tempPassword));
                var updateResult = await _loginRepository.UpdateAsync(login);
                if (updateResult.IsFailure)
                {
                    TempData["ErrorMessage"] = updateResult.ErrorMessage;
                    return RedirectToPage();
                }
                TempData["NewPassword"] = tempPassword;
                TempData["SuccessMessage"] = "Contraseña restablecida. La contraseña temporal se muestra abajo. El usuario deberá cambiarla en su primer inicio de sesión.";
            }
            return RedirectToPage();
        }

        private async Task CargarDatosAsync()
        {
            CurrentUserLevel = _authHelper.GetCurrentUserAccessLevel() ?? NivelAcceso.Parcial;

            var logins = await _loginRepository.GetAllAsync();
            var empleados = await _empleadoRepository.GetAllAsync();
            var empleadosDict = empleados.ToDictionary(e => e.EmpleadoId);

            Usuarios = logins.Select(l => new UsuarioViewModel
            {
                UsuarioLoginId = l.UsuarioLoginId,
                Email = l.Email,
                EmpleadoId = l.EmpleadoId ?? 0,
                EmpleadoNombre = l.EsCliente ? "CLIENTE" : (empleadosDict.TryGetValue(l.EmpleadoId!.Value, out var emp) ? emp!.NombreCompleto!.ToString() : "No disponible"),
                UltimoAcceso = l.UltimoAcceso,
                Activo = l.Activo,
                AdminNivelAcceso = l.EsCliente ? NivelAcceso.Cliente : (empleadosDict.TryGetValue(l.EmpleadoId!.Value, out var e) && e is Administrador admin ? admin.NivelAcceso : NivelAcceso.Parcial)
            }).ToList();

            var currentUserLevel = _authHelper.GetCurrentUserAccessLevel();
            var admins = empleados.OfType<Administrador>().Where(a => !a.IsDeleted);
            
            // Filter admins based on current user's permissions
            if (currentUserLevel == NivelAcceso.Completo)
            {
                // Completo can only see and create Parcial admins
                admins = admins.Where(a => a.NivelAcceso == NivelAcceso.Parcial);
            }
            // Gerente sees all admins (no filter)
            
            AdministradoresSelect = admins.OrderBy(a => a.NombreCompleto!.Nombres).Select(a => new SelectListItem(
                $"{a.NombreCompleto!.Nombres} {a.NombreCompleto.PrimerApellido} ({a.NivelAcceso})",
                a.EmpleadoId.ToString())).ToList();
        }

        private async Task<UsuarioLogin?> GetByIdAsync(int id)
        {
            var loginResult = await _loginRepository.GetByIdAsync(id);
            return loginResult.IsSuccess ? loginResult.Value : null;
        }

        private static string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    public class UsuarioViewModel
    {
        public int UsuarioLoginId { get; set; }
        public string Email { get; set; } = string.Empty;
        public int EmpleadoId { get; set; }
        public string EmpleadoNombre { get; set; } = string.Empty;
        public DateTime? UltimoAcceso { get; set; }
        public bool Activo { get; set; }
        public NivelAcceso AdminNivelAcceso { get; set; }
    }

    public class UsuarioFormDto
    {
        public int UsuarioLoginId { get; set; }

        [Required(ErrorMessage = "El empleado es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un empleado válido.")]
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 20 caracteres.")]
        public string Password { get; set; } = string.Empty;
    }
}
