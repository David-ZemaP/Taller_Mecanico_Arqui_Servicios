namespace Taller_Mecanico_Arqui.Application.DTOs.OrdenTrabajo
{
    public class CreateOrdenTrabajoProductoDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public double? PrecioUnitario { get; set; }
    }

    public class CreateOrdenTrabajoServicioDto
    {
        public int ServicioId { get; set; }
        public int Cantidad { get; set; }
        public double? PrecioUnitario { get; set; }
    }

    public class CreateOrdenTrabajoDto
    {
        public int VehiculoId { get; set; }
        public DateTime FechaIngreso { get; set; }
        public string EstadoVehiculo { get; set; } = string.Empty;
        public string EstadoTrabajo { get; set; } = "Recibido";
        public string EstadoPago { get; set; } = "Pendiente";
        public double Total { get; set; }
        public List<int> MecanicosSeleccionados { get; set; } = new();
        public List<CreateOrdenTrabajoProductoDto> Productos { get; set; } = new();
        public List<CreateOrdenTrabajoServicioDto> Servicios { get; set; } = new();
    }

    public class UpdateOrdenTrabajoDto
    {
        public int OrdenTrabajoId { get; set; }
        public int VehiculoId { get; set; }
        public DateTime FechaIngreso { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public string EstadoTrabajo { get; set; } = "Recibido";
        public string EstadoPago { get; set; } = "Pendiente";
        public string EstadoVehiculo { get; set; } = string.Empty;
        public double Total { get; set; }
        public List<int> MecanicosSeleccionados { get; set; } = new();
    }
}
