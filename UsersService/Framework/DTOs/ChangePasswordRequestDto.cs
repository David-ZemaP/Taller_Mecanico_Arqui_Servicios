namespace Taller_Mecanico_Users.Framework.DTOs;

/// <summary>
/// DTO para cambiar contraseña.
/// REQUISITO: CurrentPassword es obligatorio para validar con BCrypt.Verify
/// antes de permitir el cambio.
/// </summary>
public class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
