namespace OrdenTrabajoService.Application.DTOs.OrdenTrabajo
{
    public class OrdenTrabajoDetalleDto
    {
        public int OrdenTrabajoId { get; set; }
        public int ClienteId { get; set; }
        public string ClienteCi { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public int VehiculoId { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string FechaIngreso { get; set; } = string.Empty;
        public string? FechaEntrega { get; set; }
        public string EstadoTrabajo { get; set; } = string.Empty;
        public string EstadoPago { get; set; } = string.Empty;
        public string EstadoVehiculo { get; set; } = string.Empty;
        public double Total { get; set; }
        public bool IsDeleted { get; set; }
        public List<OrdenTrabajoDetalleProductoDto> Productos { get; set; } = new();
        public List<OrdenTrabajoDetalleServicioDto> Servicios { get; set; } = new();
    }

    public class OrdenTrabajoDetalleProductoDto
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public double PrecioUnitario { get; set; }
        public double Subtotal { get; set; }
    }

    public class OrdenTrabajoDetalleServicioDto
    {
        public int ServicioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public double PrecioUnitario { get; set; }
        public double Subtotal { get; set; }
    }
}
