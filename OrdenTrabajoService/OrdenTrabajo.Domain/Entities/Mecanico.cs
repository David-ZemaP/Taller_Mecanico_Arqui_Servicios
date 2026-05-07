namespace Taller_Mecanico_Arqui.Domain.Entities;

public class Mecanico
{
    public int MecanicoId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public bool Activo { get; set; } = true;

    public string NombreCompleto =>
        string.Join(" ", new[] { Nombres, PrimerApellido, SegundoApellido }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
}
