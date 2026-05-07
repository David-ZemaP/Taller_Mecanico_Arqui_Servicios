namespace Taller_Mecanico_Arqui.Domain.Entities;

public class Cliente
{
    public int ClienteId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string SegundoApellido { get; set; } = string.Empty;
    public int CiNumero { get; set; }
    public string CiComplemento { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
