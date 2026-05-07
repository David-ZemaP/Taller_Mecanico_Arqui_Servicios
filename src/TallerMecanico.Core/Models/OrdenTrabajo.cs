namespace TallerMecanico.Core.Models;

public enum EstadoOrden
{
    Pendiente,
    EnProceso,
    Completada,
    Cancelada
}

public class OrdenTrabajo
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int VehiculoId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaCompletado { get; set; }
    public EstadoOrden Estado { get; set; } = EstadoOrden.Pendiente;
    public string Descripcion { get; set; } = string.Empty;
    public List<Servicio> Servicios { get; set; } = new();
    public decimal Total => Servicios.Sum(s => s.Precio);
    public string Observaciones { get; set; } = string.Empty;
}
