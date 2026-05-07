using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Taller_Mecanico_Arqui.Frontend.Adapters;
using Taller_Mecanico_Arqui.Frontend.Authorization;
using System.Linq;

namespace Taller_Mecanico_Arqui.Pages.Empleados
{
    [RequireAccessLevel(NivelAcceso.Completo)]
    public class IndexModel : PageModel
    {
        private readonly IEmpleadoAdapter _empleadoAdapter;

        public IndexModel(IEmpleadoAdapter empleadoAdapter)
        {
            _empleadoAdapter = empleadoAdapter;
        }

        [BindProperty(SupportsGet = true)]
        public string? Filtro { get; set; }

        public string FiltroActual { get; private set; } = "todos";

        public string MensajeSinResultados => FiltroActual switch
        {
            "mecanicos" => "No hay mecánicos registrados.",
            "administradores" => "No hay administradores registrados.",
            _ => "No hay empleados registrados aún."
        };

        public List<EmpleadoListDto> Empleados { get; set; } = new();

        [BindProperty]
        public EmpleadoFormDto FormDto { get; set; } = new();

        public async Task OnGetAsync()
        {
            FiltroActual = (Filtro ?? "todos").Trim().ToLowerInvariant() switch
            {
                "mecanicos" => "mecanicos",
                "administradores" => "administradores",
                _ => "todos"
            };

            var allEmpleados = await _empleadoAdapter.GetAllAsync();

            Empleados = FiltroActual switch
            {
                "mecanicos" => allEmpleados.Where(e => e.Cargo == "Mecanico").ToList(),
                "administradores" => allEmpleados.Where(e => e.Cargo == "Administrador").ToList(),
                _ => allEmpleados.Where(e => e.Cargo == "Mecanico" || e.Cargo == "Administrador").ToList()
            };

            Empleados = Empleados.OrderByDescending(e => e.FechaContratacion).ToList();
        }

        public async Task<JsonResult> OnGetEmpleadoAsync(int id)
        {
            var empleado = await _empleadoAdapter.GetByIdAsync(id);
            if (empleado == null)
            {
                return new JsonResult(new { error = "Empleado no encontrado." }) { StatusCode = 404 };
            }

            var data = new
            {
                empleadoId = empleado.EmpleadoId,
                nombres = empleado.Nombres,
                primerApellido = empleado.PrimerApellido,
                segundoApellido = empleado.SegundoApellido,
                ciNumero = empleado.Ci.Split('-').FirstOrDefault() ?? "",
                ciComplemento = empleado.Ci.Split('-').Length > 1 ? empleado.Ci.Split('-')[1] : null,
                telefono = empleado.Telefono,
                email = empleado.Email,
                estadoLaboral = empleado.EstadoLaboral,
                cargo = empleado.Cargo
            };

            return new JsonResult(data);
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            if (FormDto.EmpleadoId == 0)
            {
                var createResult = await _empleadoAdapter.CreateAsync(new CreateEmpleadoDto
                {
                    Nombres = FormDto.Nombres,
                    PrimerApellido = FormDto.PrimerApellido,
                    SegundoApellido = FormDto.SegundoApellido,
                    CiNumero = FormDto.CiNumero,
                    CiComplemento = FormDto.CiComplemento,
                    Telefono = FormDto.Telefono,
                    Email = FormDto.Email ?? string.Empty,
                    Cargo = FormDto.Cargo
                });

                if (!createResult.Success)
                {
                    ModelState.AddModelError(string.Empty, createResult.Error ?? "No se pudo registrar el empleado.");
                    await OnGetAsync();
                    return Page();
                }
            }
            else
            {
                var updateResult = await _empleadoAdapter.UpdateAsync(new UpdateEmpleadoDto
                {
                    EmpleadoId = FormDto.EmpleadoId,
                    Nombres = FormDto.Nombres,
                    PrimerApellido = FormDto.PrimerApellido,
                    SegundoApellido = FormDto.SegundoApellido,
                    CiNumero = FormDto.CiNumero,
                    CiComplemento = FormDto.CiComplemento,
                    Telefono = FormDto.Telefono,
                    Email = FormDto.Email ?? string.Empty,
                    Cargo = FormDto.Cargo
                });

                if (!updateResult.Success)
                {
                    ModelState.AddModelError(string.Empty, updateResult.Error ?? "No se pudo actualizar el empleado.");
                    await OnGetAsync();
                    return Page();
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _empleadoAdapter.DeleteAsync(id);
            return RedirectToPage();
        }
    }

    public class EmpleadoFormDto
    {
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        [StringLength(20, ErrorMessage = "Los nombres no pueden tener más de 20 caracteres.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "El primer apellido es obligatorio.")]
        [StringLength(20, ErrorMessage = "El primer apellido no puede tener más de 20 caracteres.")]
        public string PrimerApellido { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "El segundo apellido no puede tener más de 20 caracteres.")]
        public string? SegundoApellido { get; set; }

        [Required(ErrorMessage = "El CI es obligatorio.")]
        [Range(100000, 99999999, ErrorMessage = "El CI debe tener entre 6 y 8 dígitos.")]
        public int CiNumero { get; set; }

        [RegularExpression(@"^\d[A-Z]$", ErrorMessage = "Formato de complemento inválido (Ej: 1G).")]
        public string? CiComplemento { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [RegularExpression(@"^[67]\d{7}$", ErrorMessage = "El teléfono debe tener 8 dígitos y empezar por 6 o 7.")]
        public int Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "La fecha de contratación es obligatoria.")]
        public DateTime FechaContratacion { get; set; }

        public string Cargo { get; set; } = "Mecanico";
    }
}
