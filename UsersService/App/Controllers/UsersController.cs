using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Taller_Mecanico_Users.Framework.DTOs;
using Taller_Mecanico_Users.UseCases.Users;

namespace Taller_Mecanico_Users.App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly CreateUserUseCase _createUserUseCase;
        private readonly GetUserByIdUseCase _getUserByIdUseCase;
        private readonly GetUsersUseCase _getUsersUseCase;
        private readonly UpdateUserUseCase _updateUserUseCase;
        private readonly ChangePasswordUseCase _changePasswordUseCase;
        private readonly ResetPasswordUseCase _resetPasswordUseCase;
        private readonly DeleteUserUseCase _deleteUserUseCase;

        public UsersController(
            CreateUserUseCase createUserUseCase,
            GetUserByIdUseCase getUserByIdUseCase,
            GetUsersUseCase getUsersUseCase,
            UpdateUserUseCase updateUserUseCase,
            ChangePasswordUseCase changePasswordUseCase,
            ResetPasswordUseCase resetPasswordUseCase,
            DeleteUserUseCase deleteUserUseCase)
        {
            _createUserUseCase = createUserUseCase;
            _getUserByIdUseCase = getUserByIdUseCase;
            _getUsersUseCase = getUsersUseCase;
            _updateUserUseCase = updateUserUseCase;
            _changePasswordUseCase = changePasswordUseCase;
            _resetPasswordUseCase = resetPasswordUseCase;
            _deleteUserUseCase = deleteUserUseCase;
        }

        [HttpPost]
        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
        {
            var result = await _createUserUseCase.ExecuteAsync(request);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            var usuario = result.Value!;
            return CreatedAtAction(nameof(GetUserById), new { id = usuario.UsuarioLoginId },
                new { usuario.UsuarioLoginId, usuario.Email, usuario.Username });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var result = await _getUserByIdUseCase.ExecuteAsync(id);
            if (result.IsFailure || result.Value == null)
                return NotFound(new { message = "Usuario no encontrado." });

            return Ok(ToDto(result.Value));
        }

        [HttpGet]
        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> GetUsers()
        {
            var usuarios = await _getUsersUseCase.ExecuteAsync();
            return Ok(usuarios.Select(ToDto));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var result = await _updateUserUseCase.ExecuteAsync(id, request.Email, request.Activo);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            return NoContent();
        }

        [HttpPost("{id}/change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest(new { message = "Las contraseñas no coinciden." });

            if (!CanChangeOwnPassword(id))
                return Forbid();

            var result = await _changePasswordUseCase.ExecuteAsync(id, request.CurrentPassword, request.NewPassword);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            return NoContent();
        }

        [HttpPost("{id}/reset-password")]
        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var result = await _resetPasswordUseCase.ExecuteAsync(id);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Empleado")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _deleteUserUseCase.ExecuteAsync(id);
            if (result.IsFailure)
                return BadRequest(new { message = result.ErrorMessage });

            return NoContent();
        }

        private bool CanChangeOwnPassword(int usuarioLoginId)
        {
            if (User.IsInRole("Empleado")) return true;
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(currentUserId, out var currentId) && currentId == usuarioLoginId;
        }

        private static UserDto ToDto(Taller_Mecanico_Users.Domain.Entities.UsuarioLogin usuario)
        {
            return new UserDto
            {
                UsuarioLoginId = usuario.UsuarioLoginId,
                EmpleadoId = usuario.EmpleadoId,
                ClienteId = usuario.ClienteId,
                Email = usuario.Email,
                Username = usuario.Username,
                UltimoAcceso = usuario.UltimoAcceso,
                Activo = usuario.Activo,
                RequiereCambioPassword = usuario.RequiereCambioPassword,
                EsCliente = usuario.EsCliente
            };
        }
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

    public class UserDto
    {
        public int UsuarioLoginId { get; set; }
        public int? EmpleadoId { get; set; }
        public int? ClienteId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime? UltimoAcceso { get; set; }
        public bool Activo { get; set; }
        public bool RequiereCambioPassword { get; set; }
        public bool EsCliente { get; set; }
    }
}
