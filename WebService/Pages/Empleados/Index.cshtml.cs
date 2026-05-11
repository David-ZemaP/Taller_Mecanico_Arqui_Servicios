using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebService.Adapters;
using WebService.DTOs;
using WebService.Models;

namespace WebService.Pages.Empleados
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly EmpleadosAdapter _adapter;

        public IList<EmpleadoDto> Empleados { get; set; } = new List<EmpleadoDto>();
        public NivelAcceso CurrentUserLevel { get; set; }
        public string FiltroActual { get; private set; } = "todos";

        public string MensajeSinResultados => FiltroActual switch
        {
            "mecanicos" => "No hay mecánicos registrados.",
            "administradores" => "No hay administradores registrados.",
            _ => "No hay empleados registrados aún."
        };

        public string? NuevoEmail { get; set; }
        public string? NuevaPassword { get; set; }
        public bool UsuarioExistente { get; set; }
        public string? EmailExistente { get; set; }
        public string? NotificationMessage { get; set; }
        public string? NotificationIcon { get; set; }
        public string? NotificationTitle { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Filtro { get; set; }

        [BindProperty]
        public EmpleadoFormDto FormDto { get; set; } = new();

        public IndexModel(EmpleadosAdapter adapter)
        {
            _adapter = adapter;
        }

        public async Task OnGetAsync()
        {
            CurrentUserLevel = GetCurrentLevel();

            NuevoEmail = TempData["NuevoEmail"] as string;
            NuevaPassword = TempData["NuevaPassword"] as string;
            // Consumir TempData para que no reaparezca al recargar
            TempData.Remove("NuevoEmail");
            TempData.Remove("NuevaPassword");
            UsuarioExistente = TempData["UsuarioExistente"] as bool? ?? false;
            EmailExistente = TempData["EmailExistente"] as string;
            CargarNotificacion();

            FiltroActual = (Filtro ?? "todos").Trim().ToLowerInvariant() switch
            {
                "mecanicos" => "mecanicos",
                "administradores" => "administradores",
                _ => "todos"
            };

            await CargarEmpleadosAsync();
        }

        private void CargarNotificacion()
        {
            if (TempData["SuccessMessage"] != null)
            {
                NotificationMessage = TempData["SuccessMessage"]?.ToString();
                NotificationIcon = "bi-check-circle-fill";
                NotificationTitle = "Éxito";
            }
            else if (TempData["EmailWarning"] != null)
            {
                NotificationMessage = TempData["EmailWarning"]?.ToString();
                NotificationIcon = "bi-exclamation-triangle-fill";
                NotificationTitle = "Advertencia";
            }
            else if (TempData["ErrorMessage"] != null)
            {
                NotificationMessage = TempData["ErrorMessage"]?.ToString();
                NotificationIcon = "bi-x-circle-fill";
                NotificationTitle = "Error";
            }
        }

        public async Task<JsonResult> OnGetEmpleadoAsync(int id)
        {
            var (ok, emp, error) = await _adapter.GetEmpleadoByIdAsync(id);
            if (!ok || emp is null)
                return new JsonResult(new { error = error ?? "Empleado no encontrado." }) { StatusCode = 404 };

            return new JsonResult(new
            {
                empleadoId = emp.EmpleadoId,
                nombres = emp.Nombre,
                primerApellido = emp.PrimerApellido,
                segundoApellido = emp.SegundoApellido,
                ciNumero = emp.Ci,
                ciComplemento = emp.CiComplemento,
                telefono = emp.Telefono,
                email = emp.Email,
                fechaContratacion = emp.FechaContratacion.ToString("yyyy-MM-dd"),
                tipoEmpleado = emp.TipoEmpleado,
                estadoLaboral = emp.EstadoLaboral,
                especialidad = emp.Especialidad,
                salarioPorHora = emp.SalarioPorHora,
                salarioMensual = emp.SalarioMensual,
                nivelAcceso = emp.NivelAcceso
            });
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return RedirectToPage();
            }

            bool isNew = FormDto.EmpleadoId == 0;

            if (isNew)
            {
                var (ok, empleadoId, error) = await _adapter.CrearEmpleadoAsync(FormDto);
                if (!ok)
                {
                    TempData["ErrorMessage"] = error ?? "No se pudo crear el empleado.";
                    return RedirectToPage();
                }

                // Crear automáticamente el acceso al sistema si se proporcionó email
                if (!string.IsNullOrWhiteSpace(FormDto.Email) && empleadoId.HasValue)
                {
                    var (userOk, _, _, _) = await _adapter.CreateUsuarioAsync(
                        empleadoId.Value, FormDto.Email, null);

                    if (!userOk)
                    {
                        TempData["UsuarioExistente"] = true;
                        TempData["EmailExistente"] = FormDto.Email;
                    }
                }
            }
            else
            {
                var (ok, error) = await _adapter.ActualizarEmpleadoAsync(FormDto.EmpleadoId, FormDto);
                if (!ok)
                {
                    TempData["ErrorMessage"] = error ?? "No se pudo actualizar el empleado.";
                    return RedirectToPage();
                }
            }

            TempData["SuccessMessage"] = isNew ? "Empleado creado correctamente." : "Empleado actualizado correctamente.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _adapter.EliminarEmpleadoAsync(id);
            return RedirectToPage();
        }

        private async Task CargarEmpleadosAsync()
        {
            var (ok, empleados, error) = await _adapter.GetAllEmpleadosAsync();
            if (!ok || empleados is null)
            {
                ModelState.AddModelError(string.Empty, error ?? "No se pudieron cargar los empleados.");
                Empleados = new List<EmpleadoDto>();
                return;
            }

            var query = empleados.AsQueryable();
            query = FiltroActual switch
            {
                "mecanicos" => query.Where(e => e.TipoEmpleado == "Mecanico"),
                "administradores" => query.Where(e => e.TipoEmpleado == "Administrador"),
                _ => query
            };

            Empleados = query.OrderByDescending(e => e.FechaContratacion).ToList();
        }

        private NivelAcceso GetCurrentLevel()
        {
            var claim = User.FindFirst("NivelAcceso");
            return claim != null && Enum.TryParse<NivelAcceso>(claim.Value, out var lvl) ? lvl : NivelAcceso.Parcial;
        }
    }
}
