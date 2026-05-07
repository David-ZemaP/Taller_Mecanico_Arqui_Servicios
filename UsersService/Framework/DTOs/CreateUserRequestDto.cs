namespace Taller_Mecanico_Users.Framework.DTOs;

public class CreateUserRequestDto
{
    public string Nombres { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
    public int? EmpleadoId { get; set; }
    public int? ClienteId { get; set; }
    public bool EsCliente { get; set; }
}
