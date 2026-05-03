namespace TallerMecanico.Core.Models;

public class Vehiculo
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string NumeroVin { get; set; } = string.Empty;
}
