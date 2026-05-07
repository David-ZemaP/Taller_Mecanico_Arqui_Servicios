namespace Taller_Mecanico_Arqui.Frontend.DTOs.OrdenTrabajo
{
    /// <summary>
    /// DTO para listar órdenes de trabajo en grids/tablas
    /// </summary>
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

    /// <summary>
    /// DTO para el detalle completo de una orden de trabajo
    /// </summary>
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

    /// <summary>
    /// DTO para mostrar el detalle completo de una orden de trabajo
    /// </summary>
    public class OrdenTrabajoDetalleDto
    {
        public int OrdenTrabajoId { get; set; }
        public int ClienteId { get; set; }
        public string ClienteCi { get; set; } = string.Empty;
        public int VehiculoId { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string FechaIngreso { get; set; } = string.Empty;
        public string? FechaEntrega { get; set; }
        public string EstadoTrabajo { get; set; } = string.Empty;
        public string EstadoPago { get; set; } = string.Empty;
        public string EstadoVehiculo { get; set; } = string.Empty;
        public double Total { get; set; }
        public List<OrdenTrabajoDetalleProductoDto> Productos { get; set; } = new();
        public List<OrdenTrabajoDetalleServicioDto> Servicios { get; set; } = new();
    }

    /// <summary>
    /// DTO para el formulario de crear/editar orden de trabajo
    /// </summary>
    public class OrdenTrabajoFormDto
    {
        public int OrdenTrabajoId { get; set; }
        public int ClienteId { get; set; }
        public int VehiculoId { get; set; }
        public DateTime FechaIngreso { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public string EstadoTrabajo { get; set; } = "Recibido";
        public string EstadoPago { get; set; } = "Pendiente";
        public string EstadoVehiculo { get; set; } = string.Empty;
        public double Total { get; set; }
        public string ProductosJson { get; set; } = "[]";
        public string ServiciosJson { get; set; } = "[]";
    }

    /// <summary>
    /// DTO para buscar vehículos (utilizado en autocomplete/select)
    /// </summary>
    public class VehiculoLookupDto
    {
        public int VehiculoId { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int Anio { get; set; }
    }

    /// <summary>
    /// DTO para actualizar stock de productos en una orden de trabajo
    /// </summary>
    public class CreateOrdenTrabajoProductoDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public double? PrecioUnitario { get; set; }
    }
}