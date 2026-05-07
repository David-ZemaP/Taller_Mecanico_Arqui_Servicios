namespace Taller_Mecanico_Arqui.Domain.Entities;

public class OrdenTrabajo
{
    public int OrdenTrabajoId { get; set; }
    public int ClienteId { get; set; }
    public int VehiculoId { get; set; }
    public int EmpleadoId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}
