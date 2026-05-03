using TallerMecanico.Core.Contracts;
using TallerMecanico.Core.Models;

namespace TallerMecanico.Services.Infrastructure;

public sealed class OrdenTrabajoRepository : IOrdenTrabajoRepository
{
    private readonly InMemoryTallerContext _context;
    private readonly UpdateProductStocks _updateProductStocks;
    private readonly object _sync = new();

    public OrdenTrabajoRepository(InMemoryTallerContext context, UpdateProductStocks updateProductStocks)
    {
        _context = context;
        _updateProductStocks = updateProductStocks;
    }

    public OrdenTrabajo Add(OrdenTrabajo ordenTrabajo, int? usuarioId = null)
    {
        lock (_sync)
        {
            var snapshot = _context.Clone();

            try
            {
                var stored = CloneForInsert(ordenTrabajo);
                stored.Id = _context.NextOrdenTrabajoId++;
                stored.FechaCreacion = DateTime.UtcNow;
                stored.Estado = EstadoOrden.Pendiente;
                stored.UsuarioCreacionId = usuarioId;

                _updateProductStocks.ConsumedBy(stored);

                _context.OrdenesTrabajo.Add(stored);
                return stored;
            }
            catch
            {
                _context.RestoreFrom(snapshot);
                throw;
            }
        }
    }

    public OrdenTrabajo? GetById(int id) => _context.OrdenesTrabajo.FirstOrDefault(orden => orden.Id == id && !orden.IsDeleted);

    public IEnumerable<OrdenTrabajo> GetAll() => _context.OrdenesTrabajo.Where(orden => !orden.IsDeleted).ToList();

    public IEnumerable<OrdenTrabajo> GetByCliente(int clienteId) => _context.OrdenesTrabajo.Where(orden => orden.ClienteId == clienteId && !orden.IsDeleted).ToList();

    public OrdenTrabajo? Update(int id, OrdenTrabajo ordenTrabajo)
    {
        lock (_sync)
        {
            var stored = _context.OrdenesTrabajo.FirstOrDefault(item => item.Id == id && !item.IsDeleted);
            if (stored is null)
            {
                return null;
            }

            var snapshot = _context.Clone();
            try
            {
                AdjustProductStocksForUpdate(stored.Productos, ordenTrabajo.Productos);
                stored.ClienteId = ordenTrabajo.ClienteId;
                stored.VehiculoId = ordenTrabajo.VehiculoId;
                stored.Descripcion = ordenTrabajo.Descripcion;
                stored.Productos = CloneProductos(ordenTrabajo.Productos);
                stored.Servicios = CloneServicios(ordenTrabajo.Servicios);
                return stored;
            }
            catch
            {
                _context.RestoreFrom(snapshot);
                throw;
            }
        }
    }

    public OrdenTrabajo? CambiarEstado(int id, EstadoOrden estado, int? usuarioId = null)
    {
        var stored = _context.OrdenesTrabajo.FirstOrDefault(item => item.Id == id && !item.IsDeleted);
        if (stored is null)
        {
            return null;
        }

        stored.Estado = estado;
        if (estado == EstadoOrden.Completada)
        {
            stored.FechaCompletado = DateTime.UtcNow;
        }

        return stored;
    }

    public OrdenTrabajo? Anular(int id, int usuarioId)
    {
        lock (_sync)
        {
            var stored = _context.OrdenesTrabajo.FirstOrDefault(item => item.Id == id && !item.IsDeleted);
            if (stored is null)
            {
                return null;
            }

            _updateProductStocks.Revert(stored);
            stored.MarcarAnulada(usuarioId);
            return stored;
        }
    }

    private static OrdenTrabajo CloneForInsert(OrdenTrabajo source) => new()
    {
        ClienteId = source.ClienteId,
        VehiculoId = source.VehiculoId,
        Descripcion = source.Descripcion,
        Productos = CloneProductos(source.Productos),
        Servicios = CloneServicios(source.Servicios)
    };

    private static List<DetalleProducto> CloneProductos(IEnumerable<DetalleProducto> source) => source.Select(producto => new DetalleProducto
    {
        ProductoId = producto.ProductoId,
        NombreProducto = producto.NombreProducto,
        Cantidad = producto.Cantidad,
        PrecioUnitario = producto.PrecioUnitario
    }).ToList();

    private static List<Servicio> CloneServicios(IEnumerable<Servicio> source) => source.Select(servicio => new Servicio
    {
        Id = servicio.Id,
        Nombre = servicio.Nombre,
        Descripcion = servicio.Descripcion,
        Precio = servicio.Precio
    }).ToList();

    private void AdjustProductStocksForUpdate(IEnumerable<DetalleProducto> existingDetails, IEnumerable<DetalleProducto> newDetails)
    {
        var existingByProduct = existingDetails
            .GroupBy(detail => detail.ProductoId)
            .ToDictionary(group => group.Key, group => group.Sum(detail => detail.Cantidad));

        var newByProduct = newDetails
            .GroupBy(detail => detail.ProductoId)
            .ToDictionary(group => group.Key, group => group.Sum(detail => detail.Cantidad));

        foreach (var productId in existingByProduct.Keys.Union(newByProduct.Keys))
        {
            var existingQuantity = existingByProduct.TryGetValue(productId, out var oldQuantity) ? oldQuantity : 0;
            var newQuantity = newByProduct.TryGetValue(productId, out var currentQuantity) ? currentQuantity : 0;
            var delta = newQuantity - existingQuantity;

            if (delta == 0)
            {
                continue;
            }

            if (!_updateProductStocks.RevertDelta(productId, delta))
            {
                throw new InvalidOperationException($"No se pudo ajustar el stock del producto {productId}.");
            }
        }
    }
}