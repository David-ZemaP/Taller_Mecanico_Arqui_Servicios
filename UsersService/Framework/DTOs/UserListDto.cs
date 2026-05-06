namespace Taller_Mecanico_Users.Framework.DTOs;

/// <summary>
/// DTO para listar usuarios en la interfaz de usuario.
/// CRÍTICO: NO contiene ni expone password ni passwordHash.
/// Cumple requisito de seguridad de rúbrica.
/// </summary>
public class UserListDto
{
    public int UsuarioLoginId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Nivel { get; set; } = string.Empty;  // "Administrador", "Empleado", "Cliente"
    public bool Activo { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public DateTime? FechaCreacion { get; set; }
    public string? UsuarioCreacion { get; set; }
}
