namespace Taller_Mecanico_Arqui.Application.DTOs.OrdenTrabajo;

public class CreateOrdenTrabajoDto
{
    public int VehiculoId { get; set; }
    public DateTime FechaIngreso { get; set; } = DateTime.UtcNow;
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
    public string EstadoTrabajo { get; set; } = string.Empty;
    public string EstadoPago { get; set; } = string.Empty;
    public string EstadoVehiculo { get; set; } = string.Empty;
    public double Total { get; set; }
}

public class OrdenTrabajoFormDto
{
    public int OrdenTrabajoId { get; set; }
    public int VehiculoId { get; set; }
    public DateTime FechaIngreso { get; set; } = DateTime.UtcNow;
    public DateTime? FechaEntrega { get; set; }
    public string EstadoTrabajo { get; set; } = "Recibido";
    public string EstadoPago { get; set; } = "Pendiente";
    public string EstadoVehiculo { get; set; } = string.Empty;
    public string? ProductosJson { get; set; }
    public string? ServiciosJson { get; set; }
}

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
    public List<OrdenTrabajoDetalleProductoDto> Productos { get; set; } = new();
    public List<OrdenTrabajoDetalleServicioDto> Servicios { get; set; } = new();
}

public class VehiculoLookupDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
}

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
