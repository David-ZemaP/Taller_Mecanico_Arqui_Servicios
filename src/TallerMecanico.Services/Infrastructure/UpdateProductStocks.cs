using TallerMecanico.Core.Contracts;
using TallerMecanico.Core.Models;

namespace TallerMecanico.Services.Infrastructure;

public sealed class UpdateProductStocks
{
    private readonly IProductoRepository _productoRepository;

    public UpdateProductStocks(IProductoRepository productoRepository)
    {
        _productoRepository = productoRepository;
    }

    public void ConsumedBy(OrdenTrabajo ordenTrabajo)
    {
        foreach (var detalle in ordenTrabajo.Productos)
        {
            if (!_productoRepository.UpdateStock(detalle.ProductoId, -detalle.Cantidad))
            {
                throw new InvalidOperationException($"No se pudo descontar stock para el producto {detalle.ProductoId}.");
            }
        }
    }

    public void Revert(OrdenTrabajo ordenTrabajo)
    {
        foreach (var detalle in ordenTrabajo.Productos)
        {
            if (!_productoRepository.UpdateStock(detalle.ProductoId, detalle.Cantidad))
            {
                throw new InvalidOperationException($"No se pudo revertir stock para el producto {detalle.ProductoId}.");
            }
        }
    }

    public bool RevertDelta(int productoId, int delta)
    {
        if (delta == 0)
        {
            return true;
        }

        return delta > 0
            ? _productoRepository.UpdateStock(productoId, -delta)
            : _productoRepository.UpdateStock(productoId, Math.Abs(delta));
    }
}