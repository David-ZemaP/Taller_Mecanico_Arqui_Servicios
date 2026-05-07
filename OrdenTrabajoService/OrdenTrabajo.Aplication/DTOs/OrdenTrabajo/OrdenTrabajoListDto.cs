namespace Taller_Mecanico_Arqui.Application.DTOs.OrdenTrabajo
{
    public class OrdenTrabajoListDto
    {
        public int OrdenTrabajoId { get; set; }
        public int VehiculoId { get; set; }
        public string VehiculoPlaca { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public string EstadoTrabajo { get; set; } = string.Empty;
        public string EstadoPago { get; set; } = string.Empty;
        public string EstadoVehiculo { get; set; } = string.Empty;
        public double Total { get; set; }
    }
}
