using TallerMecanico.Core.Models;

namespace TallerMecanico.Services.Infrastructure;

public sealed class InMemoryTallerContext
{
    public List<Cliente> Clientes { get; } = new();

    public List<Vehiculo> Vehiculos { get; } = new();

    public List<Producto> Productos { get; } = new();

    public List<OrdenTrabajo> OrdenesTrabajo { get; } = new();

    public int NextClienteId { get; set; } = 1;

    public int NextVehiculoId { get; set; } = 1;

    public int NextProductoId { get; set; } = 1;

    public int NextOrdenTrabajoId { get; set; } = 1;

    public InMemoryTallerContext Clone()
    {
        var clone = new InMemoryTallerContext
        {
            NextClienteId = NextClienteId,
            NextVehiculoId = NextVehiculoId,
            NextProductoId = NextProductoId,
            NextOrdenTrabajoId = NextOrdenTrabajoId
        };

        clone.Clientes.AddRange(Clientes.Select(cliente => Clone(cliente)));
        clone.Vehiculos.AddRange(Vehiculos.Select(vehiculo => Clone(vehiculo)));
        clone.Productos.AddRange(Productos.Select(producto => Clone(producto)));
        clone.OrdenesTrabajo.AddRange(OrdenesTrabajo.Select(orden => Clone(orden)));

        return clone;
    }

    public void RestoreFrom(InMemoryTallerContext snapshot)
    {
        Clientes.Clear();
        Clientes.AddRange(snapshot.Clientes.Select(cliente => Clone(cliente)));

        Vehiculos.Clear();
        Vehiculos.AddRange(snapshot.Vehiculos.Select(vehiculo => Clone(vehiculo)));

        Productos.Clear();
        Productos.AddRange(snapshot.Productos.Select(producto => Clone(producto)));

        OrdenesTrabajo.Clear();
        OrdenesTrabajo.AddRange(snapshot.OrdenesTrabajo.Select(orden => Clone(orden)));

        NextClienteId = snapshot.NextClienteId;
        NextVehiculoId = snapshot.NextVehiculoId;
        NextProductoId = snapshot.NextProductoId;
        NextOrdenTrabajoId = snapshot.NextOrdenTrabajoId;
    }

    private static Cliente Clone(Cliente source) => new()
    {
        Id = source.Id,
        Nombre = source.Nombre,
        Apellido = source.Apellido,
        Telefono = source.Telefono,
        Email = source.Email,
        IsDeleted = source.IsDeleted,
        CreatedAt = source.CreatedAt,
        CreatedByUserId = source.CreatedByUserId,
        DeletedAt = source.DeletedAt,
        DeletedByUserId = source.DeletedByUserId
    };

    private static Vehiculo Clone(Vehiculo source) => new()
    {
        Id = source.Id,
        ClienteId = source.ClienteId,
        Marca = source.Marca,
        Modelo = source.Modelo,
        Anio = source.Anio,
        Placa = source.Placa,
        Color = source.Color,
        IsDeleted = source.IsDeleted,
        CreatedAt = source.CreatedAt,
        CreatedByUserId = source.CreatedByUserId,
        DeletedAt = source.DeletedAt,
        DeletedByUserId = source.DeletedByUserId
    };

    private static Producto Clone(Producto source) => new()
    {
        Id = source.Id,
        Nombre = source.Nombre,
        Descripcion = source.Descripcion,
        Precio = source.Precio,
        Stock = source.Stock,
        IsDeleted = source.IsDeleted
    };

    private static OrdenTrabajo Clone(OrdenTrabajo source) => new()
    {
        Id = source.Id,
        ClienteId = source.ClienteId,
        VehiculoId = source.VehiculoId,
        Descripcion = source.Descripcion,
        Estado = source.Estado,
        FechaCreacion = source.FechaCreacion,
        FechaCompletado = source.FechaCompletado,
        FechaAnulacion = source.FechaAnulacion,
        UsuarioCreacionId = source.UsuarioCreacionId,
        UsuarioAnulacionId = source.UsuarioAnulacionId,
        IsDeleted = source.IsDeleted,
        Productos = source.Productos.Select(producto => new DetalleProducto
        {
            ProductoId = producto.ProductoId,
            NombreProducto = producto.NombreProducto,
            Cantidad = producto.Cantidad,
            PrecioUnitario = producto.PrecioUnitario
        }).ToList(),
        Servicios = source.Servicios.Select(servicio => new Servicio
        {
            Id = servicio.Id,
            Nombre = servicio.Nombre,
            Descripcion = servicio.Descripcion,
            Precio = servicio.Precio,
            IsDeleted = servicio.IsDeleted
        }).ToList()
    };
}