namespace Taller_Mecanico_Arqui.Domain.Entities;

public class Empleado
{
    public int EmpleadoId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string SegundoApellido { get; set; } = string.Empty;
    public int CiNumero { get; set; }
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
