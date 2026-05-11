using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Taller_Mecanico_Users.UseCases.Users;
using Taller_Mecanico_Users.Domain.Ports;

namespace Taller_Mecanico_Users.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly CreateUserUseCase _createUserUseCase;
        private readonly GetUserByIdUseCase _getUserByIdUseCase;
        private readonly GetUsersUseCase _getUsersUseCase;
        private readonly GetClientesUseCase _getClientesUseCase;
        private readonly UpdateUserUseCase _updateUserUseCase;
        private readonly ChangePasswordUseCase _changePasswordUseCase;
        private readonly ResetPasswordUseCase _resetPasswordUseCase;
        private readonly DeleteUserUseCase _deleteUserUseCase;
        private readonly IRolRepository _rolRepository;
        private readonly IUsuarioLoginRepository _usuarioLoginRepository;

        public UsersController(
            CreateUserUseCase createUserUseCase,
            GetUserByIdUseCase getUserByIdUseCase,
            GetUsersUseCase getUsersUseCase,
            GetClientesUseCase getClientesUseCase,
            UpdateUserUseCase updateUserUseCase,
            ChangePasswordUseCase changePasswordUseCase,
            ResetPasswordUseCase resetPasswordUseCase,
            DeleteUserUseCase deleteUserUseCase,
            IRolRepository rolRepository,
            IUsuarioLoginRepository usuarioLoginRepository)
        {
            _createUserUseCase = createUserUseCase;
            _getUserByIdUseCase = getUserByIdUseCase;
            _getUsersUseCase = getUsersUseCase;
            _getClientesUseCase = getClientesUseCase;
            _updateUserUseCase = updateUserUseCase;
            _changePasswordUseCase = changePasswordUseCase;
            _resetPasswordUseCase = resetPasswordUseCase;
            _deleteUserUseCase = deleteUserUseCase;
            _rolRepository = rolRepository;
            _usuarioLoginRepository = usuarioLoginRepository;
        }

        [HttpPost]
        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var usuarioResult = await _createUserUseCase.ExecuteAsync(request.EmpleadoId, request.Email, request.Password);
            if (usuarioResult.IsFailure || usuarioResult.Value == null)
            {
                return ApiResultMapper.MapError(this, usuarioResult);
            }

            var creation = usuarioResult.Value;
            return CreatedAtAction(nameof(GetUserById), new { id = creation.User.UsuarioLoginId },
                new
                {
                    creation.User.UsuarioLoginId,
                    creation.User.Email,
                    plainPassword = creation.PlainPassword,
                    notificationRecipients = creation.NotificationRecipients
                });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var result = await _getUserByIdUseCase.ExecuteAsync(id);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }

            var usuario = result.Value;
            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            return Ok(ToDto(usuario));
        }

        [HttpGet]
        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> GetUsers()
        {
            var usuarios = await _getUsersUseCase.ExecuteAsync();
            return Ok(usuarios.Select(ToDto));
        }

        [HttpGet("empleado/{empleadoId}")]
        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> GetUserByEmpleadoId(int empleadoId)
        {
            var usuario = await _usuarioLoginRepository.GetByEmpleadoIdAsync(empleadoId);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado para este empleado." });
            }

            return Ok(ToDto(usuario));
        }

        [HttpGet("clientes")]
        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> GetClientes()
        {
            var clientes = await _getClientesUseCase.ExecuteAsync();
            return Ok(clientes.Select(ToDto));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Empleado")] 
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var result = await _updateUserUseCase.ExecuteAsync(id, request.Email, request.Activo);

            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }

            return NoContent(); 
        }

        [HttpPost("{id}/change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            if (!CanChangeOwnPassword(id))
            {
                return Forbid();
            }

            var result = await _changePasswordUseCase.ExecuteAsync(id, request.CurrentPassword, request.NewPassword, request.ConfirmPassword);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }

            return NoContent();
        }

        [HttpPost("{id}/reset-password")]
        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var result = await _resetPasswordUseCase.ExecuteAsync(id);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }

            return Ok(new { plainPassword = result.Value });
        }

        [HttpPut("{id}/rol")]
        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateRoleRequest request)
        {
            // Verificar que el usuario actual tenga permisos (Gerente o Completo)
            // Temporal: verificar también Parcial hasta debuguear el claim
            var currentNivelAcceso = User.FindFirst("NivelAcceso")?.Value;
            if (currentNivelAcceso != "Gerente" && currentNivelAcceso != "Completo" && currentNivelAcceso != "Parcial")
            {
                return Forbid();
            }

            // Obtener el rol por nombre
            var rol = await _rolRepository.GetByNombreAsync(request.RolNombre);
            if (rol == null)
            {
                return BadRequest(new { message = "Rol no válido. Roles válidos: Gerente, Administrador, Mecanico, Cliente" });
            }

            // Obtener el usuario
            var usuarioResult = await _usuarioLoginRepository.GetByIdAsync(id);
            if (usuarioResult.IsFailure || usuarioResult.Value == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            var usuario = usuarioResult.Value;
            
            // No permitir que un cliente se asigne rol de empleado
            if (usuario.EsCliente && request.RolNombre != "Cliente")
            {
                return BadRequest(new { message = "Un cliente no puede tener rol de empleado" });
            }

            // Asignar el rol y actualizar
            usuario.AsignarRol(rol);
            usuario.RegistrarActualizacion(User.FindFirst(ClaimTypes.Email)?.Value);
            var result = await _usuarioLoginRepository.UpdateAsync(usuario);
            
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }

            return Ok(new { message = "Rol actualizado correctamente", rolId = rol.RolId, rolNombre = rol.Nombre });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _deleteUserUseCase.ExecuteAsync(id);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }

            return NoContent();
        }

        private bool CanChangeOwnPassword(int usuarioLoginId)
        {
            if (User.IsInRole("Empleado"))
            {
                return true;
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(currentUserId, out var currentId) && currentId == usuarioLoginId;
        }

        private static UserDto ToDto(Taller_Mecanico_Users.Domain.Entities.UsuarioLogin usuario)
        {
            // Mapear Rol a NivelAcceso para el frontend
            string nivelAcceso;
            if (usuario.EsCliente)
            {
                nivelAcceso = "Cliente";
            }
            else if (usuario.Rol != null)
            {
                nivelAcceso = usuario.Rol.Nombre switch
                {
                    "Gerente" => "Gerente",
                    "Administrador" => "Completo",
                    "Mecanico" => "Parcial",
                    "Cliente" => "Cliente",
                    _ => "Parcial"
                };
            }
            else
            {
                nivelAcceso = usuario.NivelAcceso ?? "Parcial";
            }

            return new UserDto
            {
                UsuarioLoginId = usuario.UsuarioLoginId,
                EmpleadoId = usuario.EmpleadoId,
                ClienteId = usuario.ClienteId,
                Email = usuario.Email,
                UltimoAcceso = usuario.UltimoAcceso,
                Activo = usuario.Activo,
                RequiereCambioPassword = usuario.RequiereCambioPassword,
                EsCliente = usuario.EsCliente,
                NivelAcceso = nivelAcceso
            };
        }
    }

    public class CreateUserRequest
    {
        public int EmpleadoId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
    }

    public class UpdateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

        public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class UpdateRoleRequest
    {
        public string RolNombre { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public int UsuarioLoginId { get; set; }
        public int? EmpleadoId { get; set; }
        public int? ClienteId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime? UltimoAcceso { get; set; }
        public bool Activo { get; set; }
        public bool RequiereCambioPassword { get; set; }
        public bool EsCliente { get; set; }
        public string? NivelAcceso { get; set; }
    }
}
