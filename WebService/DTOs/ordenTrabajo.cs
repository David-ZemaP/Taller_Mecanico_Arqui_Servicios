using System.ComponentModel.DataAnnotations;

namespace WebService.DTOs
{
    // ─── Orden de Trabajo ────────────────────────────────────────────────────

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
        public bool IsDeleted { get; set; }
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

    // ─── Form DTO (se usa en el PageModel para crear y editar) ───────────────

    public class OrdenTrabajoFormDto
    {
        public int OrdenTrabajoId { get; set; }

        [Required]
        public int VehiculoId { get; set; }

        // ClienteId used by the UI to filter vehicles by client (S2 lookup)
        public int ClienteId { get; set; }

        public DateTime FechaIngreso { get; set; } = DateTime.Today;
        public DateTime? FechaEntrega { get; set; }

        [Required]
        public string EstadoVehiculo { get; set; } = string.Empty;

        public string EstadoTrabajo { get; set; } = "Recibido";
        public string EstadoPago { get; set; } = "Pendiente";
        public double Total { get; set; }

        // JSON strings populated by JavaScript before form submission
        public string ProductosJson { get; set; } = "[]";
        public string ServiciosJson { get; set; } = "[]";

        public List<int> MecanicosSeleccionados { get; set; } = new();

        // Parsed at post time from JSON fields
        [System.Text.Json.Serialization.JsonIgnore]
        public List<OrdenTrabajoProductoItemDto> Productos { get; set; } = new();
        [System.Text.Json.Serialization.JsonIgnore]
        public List<OrdenTrabajoServicioItemDto> Servicios { get; set; } = new();
    }

    public class OrdenTrabajoProductoItemDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public double? PrecioUnitario { get; set; }
    }

    public class OrdenTrabajoServicioItemDto
    {
        public int ServicioId { get; set; }
        public int Cantidad { get; set; }
        public double? PrecioUnitario { get; set; }
    }

    // ─── Producto ─────────────────────────────────────────────────────────────

    public class ProductoDto
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public double Precio { get; set; }
        public int Stock { get; set; }
    }

    public class ProductoFormDto
    {
        public int ProductoId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo.")]
        public double Precio { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int Stock { get; set; }
    }

    // ─── Servicio ─────────────────────────────────────────────────────────────

    public class ServicioDto
    {
        public int ServicioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public double Precio { get; set; }
    }

    public class ServicioFormDto
    {
        public int ServicioId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "El precio no puede ser negativo.")]
        public double Precio { get; set; }
    }

    // ─── Vehículo ─────────────────────────────────────────────────────────────

    public class VehiculoListDto
    {
        public int VehiculoId { get; set; }
        public string Placa { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public int Anio { get; set; }
        public string MarcaNombre { get; set; } = string.Empty;
        public string ModeloNombre { get; set; } = string.Empty;
        public string ColorNombre { get; set; } = string.Empty;
    }

    public class VehiculoLookupDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    public class VehiculoFormDto
    {
        public int VehiculoId { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "La placa es obligatoria.")]
        public string Placa { get; set; } = string.Empty;

        [Required]
        public int MarcaId { get; set; }

        [Required]
        public int ModeloId { get; set; }

        [Required]
        public int ColorVehiculoId { get; set; }

        [Range(1900, 2100, ErrorMessage = "Año inválido.")]
        public int Anio { get; set; }
    }

    // ─── Catálogo (Marca / Modelo / Color) ───────────────────────────────────

    public class MarcaDto
    {
        public int MarcaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class ModeloDto
    {
        public int ModeloId { get; set; }
        public int MarcaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class ColorVehiculoDto
    {
        public int ColorVehiculoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    // ─── Lookup genérico (para búsquedas typeahead) ───────────────────────────

    public class LookupDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public double? Precio { get; set; }
        public int? Stock { get; set; }
    }
}
