using System.Security.Claims;

namespace Taller_Mecanico_Arqui.Frontend.Adapters;

/// <summary>
/// Interface para el adapter de UsersService.
/// Abstrae todas las llamadas HTTP al UsersService (puerto 5001).
/// </summary>
public interface IUsersServiceAdapter
{
    // === Authentication ===
    Task<AuthResponse?> LoginAsync(string email, string password);

    // === User Management ===
    Task<List<UserListDto>> GetAllUsersAsync();
    Task<UserDetailDto?> GetUserByIdAsync(int id);
    Task<(bool Success, int? UserId, string? Error)> CreateUserAsync(CreateUserDto dto);
    Task<(bool Success, string? Error)> UpdateUserAsync(UpdateUserDto dto);
    Task<(bool Success, string? Error)> DeleteUserAsync(int id);
    Task<(bool Success, string? Error)> ToggleActivoAsync(int id, bool activo);

    // === Password Management ===
    Task<(bool Success, string? Error)> ChangePasswordAsync(int userId, string currentPassword, string newPassword, string confirmPassword);
    Task<(bool Success, string? Error)> ResetPasswordAsync(int userId);
}

/// <summary>
/// Response del endpoint de login.
/// Contiene el JWT token y flags necesarios para reconstruir la sesión.
/// </summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public bool RequiereCambioPassword { get; set; }
    public bool EsCliente { get; set; }

    // Claims extraídos del JWT (poblados por el adapter al validar el token)
    public int? UserId { get; set; }
    public int? EmpleadoId { get; set; }
    public int? ClienteId { get; set; }
    public string? NivelAcceso { get; set; }
}

// === DTOs para User Management ===

public class UserListDto
{
    public int UsuarioLoginId { get; set; }
    public int? EmpleadoId { get; set; }
    public string? EmpleadoNombre { get; set; }
    public int? ClienteId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime? UltimoAcceso { get; set; }
    public bool Activo { get; set; }
    public bool RequiereCambioPassword { get; set; }
    public bool EsCliente { get; set; }
}

public class UserDetailDto
{
    public int UsuarioLoginId { get; set; }
    public int? EmpleadoId { get; set; }
    public string? EmpleadoNombre { get; set; }
    public int? ClienteId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime? UltimoAcceso { get; set; }
    public bool Activo { get; set; }
    public bool RequiereCambioPassword { get; set; }
    public bool EsCliente { get; set; }
}

public class CreateUserDto
{
    public int EmpleadoId { get; set; }
    public string Email { get; set; } = string.Empty;
}

public class UpdateUserDto
{
    public int UsuarioLoginId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool Activo { get; set; }
}