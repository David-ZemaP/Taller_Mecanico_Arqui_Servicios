namespace Taller_Mecanico_Arqui.Domain.Entities;

public class UsuarioLogin
{
    public int UsuarioLoginId { get; set; }
    public int? EmpleadoId { get; set; }
    public int? ClienteId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime? UltimoAcceso { get; set; }
    public bool Activo { get; set; }
    public bool RequiereCambioPassword { get; set; }
    public bool EsCliente { get; set; }

    public static UsuarioLogin Crear(int empleadoId, string email, string passwordHash)
    {
        return new UsuarioLogin
        {
            EmpleadoId = empleadoId,
            Email = email,
            PasswordHash = passwordHash
        };
    }
}
