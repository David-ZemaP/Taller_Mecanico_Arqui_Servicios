namespace Taller_Mecanico_Arqui.Domain.Entities;

public class Vehiculo
{
    public int VehiculoId { get; set; }
    public int ClienteId { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Ano { get; set; }
    public string Color { get; set; } = string.Empty;
}
