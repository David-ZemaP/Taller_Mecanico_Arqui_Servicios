namespace Taller_Mecanico_Arqui.Domain.Entities;

public class Cliente
{
    public int ClienteId { get; set; }
    public string? Ci { get; set; }
    public string? Nombres { get; set; }
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }

    public string? NombreCompleto =>
        string.Join(" ", new[] { Nombres, PrimerApellido, SegundoApellido }
            .Where(s => !string.IsNullOrWhiteSpace(s)));
}
