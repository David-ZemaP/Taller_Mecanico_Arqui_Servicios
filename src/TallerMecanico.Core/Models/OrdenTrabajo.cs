namespace TallerMecanico.Core.Models;

public sealed class OrdenTrabajo
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public int VehiculoId { get; set; }

    public string? Descripcion { get; set; }

    public EstadoOrden Estado { get; set; } = EstadoOrden.Pendiente;

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaCompletado { get; set; }

    public DateTime? FechaAnulacion { get; set; }

    public int? UsuarioCreacionId { get; set; }

    public int? UsuarioAnulacionId { get; set; }

    public bool IsDeleted { get; set; }

    public List<DetalleProducto> Productos { get; set; } = new();

    public List<Servicio> Servicios { get; set; } = new();

    public decimal Total => Productos.Sum(producto => producto.Subtotal) + Servicios.Sum(servicio => servicio.Precio);

    public void Recalcular()
    {
        Productos = Productos ?? new List<DetalleProducto>();
        Servicios = Servicios ?? new List<Servicio>();
    }

    public void MarcarAnulada(int usuarioId)
    {
        Estado = EstadoOrden.Anulada;
        IsDeleted = true;
        UsuarioAnulacionId = usuarioId;
        FechaAnulacion = DateTime.UtcNow;
    }

    public void MarcarCompletada()
    {
        Estado = EstadoOrden.Completada;
        FechaCompletado = DateTime.UtcNow;
    }
}