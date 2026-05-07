namespace Taller_Mecanico_Arqui.Domain.Entities;

public class Vehiculo
{
    public int VehiculoId { get; set; }
    public string Placa { get; set; } = string.Empty;
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public int ClienteId { get; set; }
    public bool IsDeleted { get; set; }
    public Cliente? Cliente { get; set; }
}
