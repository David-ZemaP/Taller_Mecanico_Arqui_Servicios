namespace Taller_Mecanico_Users.Framework.DTOs;

/// <summary>
/// DTO para crear usuarios en el sistema.
/// Recibe datos crudos del formulario.
/// Username se autogenera en CreateUserUseCase (inicial_nombre + apellido).
/// Password temporal se genera y envía por email.
/// </summary>
public class CreateUserRequestDto
{
    public string Nombres { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;  // "Empleado", "Cliente", "Administrador"
    
    // Username y Password se generan en el servidor (NO se envían)
}
