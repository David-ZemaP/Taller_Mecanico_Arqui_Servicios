namespace Taller_Mecanico_Arqui.Domain.Entities;

public class Servicio
{
    public int ServicioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public double Precio { get; set; }
    public bool Activo { get; set; } = true;
}
