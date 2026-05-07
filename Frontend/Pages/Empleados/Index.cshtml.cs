using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Taller_Mecanico_Arqui.Application.UseCases.Empleados;
using Taller_Mecanico_Arqui.Application.DTOs.Empleados;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Enums;
using Taller_Mecanico_Arqui.Infrastructure.Authorization;
using System.Security.Cryptography;
using Taller_Mecanico_Arqui.Application.Common;

namespace Taller_Mecanico_Arqui.Pages.Empleados
{
    [RequireAccessLevel(NivelAcceso.Completo, AllowedLevels = new[] { NivelAcceso.Completo, NivelAcceso.Gerente })]
    public class IndexModel : PageModel
    {
        private readonly GetAllEmpleadosUseCase _getAllUseCase;
        private readonly GetEmpleadoByIdUseCase _getByIdUseCase;
        private readonly CreateEmpleadoUseCase _createUseCase;
        private readonly UpdateEmpleadoUseCase _updateUseCase;
        private readonly DeleteEmpleadoUseCase _deleteUseCase;
        private readonly Taller_Mecanico_Arqui.Infrastructure.Services.AuthenticationHelper _authHelper;
        private readonly IUsuarioLoginRepository _loginRepository;
        private readonly ICredentialEmailSender _emailSender;

        public IndexModel(
            GetAllEmpleadosUseCase getAllUseCase,
            GetEmpleadoByIdUseCase getByIdUseCase,
            CreateEmpleadoUseCase createUseCase,
            UpdateEmpleadoUseCase updateUseCase,
            DeleteEmpleadoUseCase deleteUseCase,
            Taller_Mecanico_Arqui.Infrastructure.Services.AuthenticationHelper authHelper,
            IUsuarioLoginRepository loginRepository,
            ICredentialEmailSender emailSender)
        {
            _getAllUseCase = getAllUseCase;
            _getByIdUseCase = getByIdUseCase;
            _createUseCase = createUseCase;
            _updateUseCase = updateUseCase;
            _deleteUseCase = deleteUseCase;
            _authHelper = authHelper;
            _loginRepository = loginRepository;
            _emailSender = emailSender;
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

        public IList<Empleado> Empleados { get; set; } = new List<Empleado>();
        public NivelAcceso CurrentUserLevel { get; set; }

        [BindProperty]
        public EmpleadoFormDto FormDto { get; set; } = new();

        // Propiedades para las credenciales de acceso
        public string? NuevoEmail { get; set; }
        public string? NuevaPassword { get; set; }
        
        // Propiedades para manejar usuario existente
        public bool UsuarioExistente { get; set; }
        public string? EmailExistente { get; set; }

        public async Task OnGetAsync()
        {
            CurrentUserLevel = _authHelper.GetCurrentUserAccessLevel() ?? NivelAcceso.Parcial;
            
            // Cargar credenciales desde TempData si existen
            NuevoEmail = TempData["NuevoEmail"] as string;
            NuevaPassword = TempData["NuevaPassword"] as string;
            
            // Cargar información de usuario existente
            UsuarioExistente = TempData["UsuarioExistente"] as bool? ?? false;
            EmailExistente = TempData["EmailExistente"] as string;
            
            FiltroActual = (Filtro ?? "todos").Trim().ToLowerInvariant() switch
            {
                "mecanicos" => "mecanicos",
                "administradores" => "administradores",
                _ => "todos"
            };

            var allEmpleados = await _getAllUseCase.ExecuteAsync();
            var query = allEmpleados.AsQueryable();

            query = FiltroActual switch
            {
                "mecanicos" => query.Where(e => e is Mecanico),
                "administradores" => query.Where(e => e is Administrador),
                _ => query.Where(e => e is Mecanico || e is Administrador)
            };

            Empleados = query.OrderByDescending(e => e.FechaContratacion).ToList();
        }

        public async Task<JsonResult> OnGetEmpleadoAsync(int id)
        {
            var empleadoResult = await _getByIdUseCase.ExecuteAsync(id);
            if (empleadoResult.IsFailure)
            {
                if (empleadoResult.ErrorCode == ErrorCodes.EmpleadoNotFound)
                    return new JsonResult(new { error = "Empleado no encontrado." }) { StatusCode = 404 };

                return new JsonResult(new { error = empleadoResult.ErrorMessage ?? "Error al consultar empleado." }) { StatusCode = 500 };
            }

            var empleado = empleadoResult.Value!;

            var data = new
            {
                empleadoId = empleado.EmpleadoId,
                nombres = empleado.NombreCompleto.Nombres,
                primerApellido = empleado.NombreCompleto.PrimerApellido,
                segundoApellido = empleado.NombreCompleto.SegundoApellido,
                ciNumero = empleado.Ci.Numero,
                ciComplemento = empleado.Ci.Complemento,
                telefono = empleado.Telefono,
                email = empleado.Email,
                fechaContratacion = empleado.FechaContratacion.ToString("yyyy-MM-dd"),
                estadoLaboral = empleado.EstadoLaboral.ToString(),
                tipoEmpleado = empleado switch
                {
                    Mecanico => "Mecanico",
                    Administrador => "Administrador",
                    _ => "Empleado"
                },
                especialidad = (empleado as Mecanico)?.Especialidad,
                salarioPorHora = (empleado as Mecanico)?.SalarioPorHora,
                salarioMensual = (empleado as Administrador)?.SalarioMensual,
                nivelAcceso = (empleado as Administrador)?.NivelAcceso
            };

            return new JsonResult(data);
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            var emailValidation = ValidationHelper.ValidateAdminEmail(FormDto.TipoEmpleado, FormDto.Email);
            if (emailValidation.IsFailure)
                ModelState.AddModelError("FormDto.Email", emailValidation.ErrorMessage ?? "El correo electrónico es obligatorio para administradores.");

            if (!ModelState.IsValid)
            {
                await RecargarLista();
                return Page();
            }

            var currentUserLevel = _authHelper.GetCurrentUserAccessLevel();
            var currentUserLevelValidation = ValidationHelper.ValidateAccessLevelConfigured(currentUserLevel);
            if (currentUserLevelValidation.IsFailure)
            {
                ModelState.AddModelError(string.Empty, currentUserLevelValidation.ErrorMessage ?? "No se pudo determinar el nivel de acceso del usuario actual.");
                await RecargarLista();
                return Page();
            }

            // Validate if user can create admin with specified access level
            if (FormDto.TipoEmpleado == "Administrador" && !string.IsNullOrEmpty(FormDto.NivelAcceso))
            {
                var nivelAccesoResult = ValidationHelper.ParseNivelAcceso(FormDto.NivelAcceso);
                if (nivelAccesoResult.IsFailure)
                {
                    ModelState.AddModelError(string.Empty, nivelAccesoResult.ErrorMessage ?? "Nivel de acceso no válido.");
                    await RecargarLista();
                    return Page();
                }

                var newLevel = nivelAccesoResult.Value;

                if (FormDto.EmpleadoId == 0)
                {
                    var canCreateValidation = ValidationHelper.RequireCanCreateAdmin(_authHelper.CanCreateAdmin(newLevel), newLevel);
                    if (canCreateValidation.IsFailure)
                    {
                        ModelState.AddModelError(string.Empty, canCreateValidation.ErrorMessage ?? $"No tienes permisos para crear administradores con nivel {newLevel}.");
                        await RecargarLista();
                        return Page();
                    }
                }
                else
                {
                    var existingEmpleadoResult = await _getByIdUseCase.ExecuteAsync(FormDto.EmpleadoId);
                    if (existingEmpleadoResult.IsFailure)
                    {
                        ModelState.AddModelError(string.Empty, existingEmpleadoResult.ErrorMessage ?? "No se pudo consultar el empleado.");
                        await RecargarLista();
                        return Page();
                    }

                    if (existingEmpleadoResult.Value is Administrador existingAdmin)
                    {
                        var canModifyValidation = ValidationHelper.RequireCanModifyAdmin(_authHelper.CanModifyAdmin(existingAdmin.NivelAcceso), existingAdmin.NivelAcceso);
                        if (canModifyValidation.IsFailure)
                        {
                            ModelState.AddModelError(string.Empty, canModifyValidation.ErrorMessage ?? $"No tienes permisos para modificar administradores con nivel {existingAdmin.NivelAcceso}.");
                            await RecargarLista();
                            return Page();
                        }

                        if (newLevel != existingAdmin.NivelAcceso)
                        {
                            var canUpgradeValidation = ValidationHelper.RequireCanCreateAdmin(_authHelper.CanCreateAdmin(newLevel), newLevel);
                            if (canUpgradeValidation.IsFailure)
                            {
                                ModelState.AddModelError(string.Empty, canUpgradeValidation.ErrorMessage ?? $"No tienes permisos para cambiar el nivel de acceso a {newLevel}.");
                                await RecargarLista();
                                return Page();
                            }
                        }
                    }
                }
            }

            if (FormDto.EmpleadoId == 0)
            {
                var createResult = await _createUseCase.ExecuteAsync(new CreateEmpleadoDto
                {
                    Nombres = FormDto.Nombres,
                    PrimerApellido = FormDto.PrimerApellido,
                    SegundoApellido = FormDto.SegundoApellido,
                    CiNumero = FormDto.CiNumero,
                    CiComplemento = FormDto.CiComplemento,
                    Telefono = FormDto.Telefono,
                    Email = FormDto.Email,
                    FechaContratacion = FormDto.FechaContratacion,
                    TipoEmpleado = FormDto.TipoEmpleado,
                    EstadoLaboral = FormDto.EstadoLaboral,
                    Especialidad = FormDto.Especialidad,
                    SalarioPorHora = FormDto.SalarioPorHora,
                    SalarioMensual = FormDto.SalarioMensual,
                    NivelAcceso = FormDto.NivelAcceso
                });

                if (createResult.IsFailure)
                {
                    ModelState.AddModelError(string.Empty, createResult.ErrorMessage ?? "No se pudo registrar el empleado.");
                    await RecargarLista();
                    return Page();
                }

                // Si es un administrador, crear automáticamente las credenciales de acceso
                if (FormDto.TipoEmpleado == "Administrador" && !string.IsNullOrEmpty(FormDto.Email))
                {
                    // Buscar el empleado recién creado por CI para obtener su ID
                    var empleados = await _getAllUseCase.ExecuteAsync();
                    var empleadoCreado = empleados
                        .Where(e => e.Ci.Numero == FormDto.CiNumero && 
                                   e.Ci.Complemento == FormDto.CiComplemento &&
                                   e is Administrador)
                        .OrderByDescending(e => e.FechaContratacion)
                        .FirstOrDefault();

                    if (empleadoCreado != null)
                    {
                        // Verificar si ya existe un usuario para este empleado
                        var usuarioExistente = await _loginRepository.GetByEmpleadoIdAsync(empleadoCreado.EmpleadoId);
                        
                        if (usuarioExistente == null)
                        {
                            var password = GenerateSecurePassword();
                            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
                            
                            var usuarioLogin = UsuarioLogin.Crear(empleadoCreado.EmpleadoId, FormDto.Email, passwordHash, true);
                            var addResult = await _loginRepository.AddAsync(usuarioLogin);
                            if (addResult.IsFailure)
                            {
                                ModelState.AddModelError(string.Empty, addResult.ErrorMessage ?? "No se pudo crear el usuario login del administrador.");
                                await RecargarLista();
                                return Page();
                            }

                            // Enviar credenciales por correo
                            var emailResult = await _emailSender.SendCredentialsAsync(FormDto.Email, empleadoCreado.NombreCompleto.ToString(), password);
                            if (emailResult.IsFailure)
                            {
                                TempData["EmailWarning"] = "Usuario creado, pero no se pudo enviar el correo: " + emailResult.ErrorMessage;
                            }
                            
                            // Guardar credenciales para mostrar en el modal
                            TempData["NuevoEmail"] = FormDto.Email;
                            TempData["NuevaPassword"] = password;
                        }
                        else
                        {
                            // Si ya existe un usuario, mostrar mensaje informativo
                            TempData["UsuarioExistente"] = true;
                            TempData["EmailExistente"] = usuarioExistente.Email;
                        }
                    }
                }
            }
            else
            {
                var updateResult = await _updateUseCase.ExecuteAsync(new UpdateEmpleadoDto
                {
                    EmpleadoId = FormDto.EmpleadoId,
                    Nombres = FormDto.Nombres,
                    PrimerApellido = FormDto.PrimerApellido,
                    SegundoApellido = FormDto.SegundoApellido,
                    CiNumero = FormDto.CiNumero,
                    CiComplemento = FormDto.CiComplemento,
                    Telefono = FormDto.Telefono,
                    Email = FormDto.Email,
                    FechaContratacion = FormDto.FechaContratacion,
                    TipoEmpleado = FormDto.TipoEmpleado,
                    EstadoLaboral = FormDto.EstadoLaboral,
                    Especialidad = FormDto.Especialidad,
                    SalarioPorHora = FormDto.SalarioPorHora,
                    SalarioMensual = FormDto.SalarioMensual,
                    NivelAcceso = FormDto.NivelAcceso
                });

                if (updateResult.IsFailure)
                {
                    if (updateResult.ErrorCode == ErrorCodes.EmpleadoNotFound)
                    {
                        TempData["ErrorMessage"] = updateResult.ErrorMessage;
                        return RedirectToPage();
                    }

                    ModelState.AddModelError(string.Empty, updateResult.ErrorMessage ?? "No se pudo actualizar el empleado.");
                    await RecargarLista();
                    return Page();
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _deleteUseCase.ExecuteAsync(id);
            return RedirectToPage();
        }

        private async Task RecargarLista()
        {
            var allEmpleados = await _getAllUseCase.ExecuteAsync();
            var query = allEmpleados.AsQueryable();
            query = FiltroActual switch
            {
                "mecanicos" => query.Where(e => e is Mecanico),
                "administradores" => query.Where(e => e is Administrador),
                _ => query.Where(e => e is Mecanico || e is Administrador)
            };
            Empleados = query.OrderByDescending(e => e.FechaContratacion).ToList();
        }

        private static string GenerateSecurePassword()
        {
            const string uppercase = "ABCDEFGHJKMNPQRSTUVWXYZ";     // 25 caracteres
            const string lowercase = "abcdefghijkmnpqrstuvwxyz";     // 25 caracteres  
            const string numbers = "23456789";                       // 8 caracteres
            const string specialChars = "!@#$%&*+=-";               // 10 caracteres
            const string allChars = uppercase + lowercase + numbers + specialChars;
            
            var password = new char[10];
            using var rng = RandomNumberGenerator.Create();
            
            // Asegurar al menos una mayúscula, una minúscula, un número y un carácter especial
            password[0] = uppercase[GetRandomIndex(rng, uppercase.Length)];
            password[1] = lowercase[GetRandomIndex(rng, lowercase.Length)];
            password[2] = numbers[GetRandomIndex(rng, numbers.Length)];
            password[3] = specialChars[GetRandomIndex(rng, specialChars.Length)];
            
            // Llenar el resto con caracteres aleatorios de todos los tipos
            for (int i = 4; i < 10; i++)
            {
                password[i] = allChars[GetRandomIndex(rng, allChars.Length)];
            }
            
            // Mezclar el array para que los tipos de caracteres no estén en posiciones fijas
            for (int i = password.Length - 1; i > 0; i--)
            {
                int j = GetRandomIndex(rng, i + 1);
                (password[i], password[j]) = (password[j], password[i]);
            }
            
            return new string(password);
        }

        private static int GetRandomIndex(RandomNumberGenerator rng, int maxValue)
        {
            if (maxValue <= 0)
                throw new ArgumentException("maxValue must be greater than 0", nameof(maxValue));
                
            byte[] randomBytes = new byte[4];
            rng.GetBytes(randomBytes);
            int randomValue = Math.Abs(BitConverter.ToInt32(randomBytes, 0));
            return randomValue % maxValue;
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

        public string TipoEmpleado { get; set; } = "Mecanico";
        public string EstadoLaboral { get; set; } = "Activo";
        public string? Especialidad { get; set; }
        public decimal? SalarioPorHora { get; set; }
        public decimal? SalarioMensual { get; set; }
        public string? NivelAcceso { get; set; }
    }
}
