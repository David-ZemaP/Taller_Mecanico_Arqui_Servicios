using Taller_Mecanico_Arqui.Application.DTOs.OrdenTrabajo;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Ports;

namespace Taller_Mecanico_Arqui.Application.Facades
{
    public class UpdateProductStocks
    {
        private readonly IRepository<Producto> _productoRepository;

        public UpdateProductStocks(IRepository<Producto> productoRepository)
        {
            _productoRepository = productoRepository;
        }

        public async Task<Result> ExecuteAsync(IEnumerable<CreateOrdenTrabajoProductoDto> productos)
        {
            foreach (var productoDto in productos.Where(p => p.ProductoId > 0 && p.Cantidad > 0))
            {
                var productoResult = await _productoRepository.GetByIdAsync(productoDto.ProductoId);
                if (productoResult.IsFailure)
                {
                    return Result.Failure(
                        productoResult.ErrorCode ?? ErrorCodes.DbError,
                        productoResult.ErrorMessage ?? "Error al consultar producto.");
                }

                var producto = productoResult.Value;
                if (producto == null)
                {
                    return Result.Failure(
                        ErrorCodes.ValidationInvalidValue,
                        $"Producto con ID {productoDto.ProductoId} no encontrado.");
                }

                if (producto.Stock < productoDto.Cantidad)
                {
                    return Result.Failure(
                        ErrorCodes.ValidationInvalidValue,
                        $"Stock insuficiente para el producto '{producto.Nombre}'. Stock actual: {producto.Stock}.");
                }

                producto.ReducirStock(productoDto.Cantidad);
                var updateResult = await _productoRepository.UpdateAsync(producto);
                if (updateResult.IsFailure)
                {
                    return Result.Failure(
                        updateResult.ErrorCode ?? ErrorCodes.DbError,
                        updateResult.ErrorMessage ?? "No se pudo actualizar el stock del producto.");
                }
            }

            return Result.Success();
        }

        public async Task<Result> RestoreAsync(IEnumerable<OrdenTrabajoProducto> productos)
        {
            foreach (var productoOrden in productos.Where(p => p.ProductoId > 0 && p.Cantidad > 0))
            {
                var productoResult = await _productoRepository.GetByIdAsync(productoOrden.ProductoId);
                if (productoResult.IsFailure)
                {
                    return Result.Failure(
                        productoResult.ErrorCode ?? ErrorCodes.DbError,
                        productoResult.ErrorMessage ?? "Error al consultar producto.");
                }

                var producto = productoResult.Value;
                if (producto == null)
                {
                    return Result.Failure(
                        ErrorCodes.ValidationInvalidValue,
                        $"Producto con ID {productoOrden.ProductoId} no encontrado.");
                }

                producto.AumentarStock(productoOrden.Cantidad);
                var updateResult = await _productoRepository.UpdateAsync(producto);
                if (updateResult.IsFailure)
                {
                    return Result.Failure(
                        updateResult.ErrorCode ?? ErrorCodes.DbError,
                        updateResult.ErrorMessage ?? "No se pudo reponer el stock del producto.");
                }
            }

            return Result.Success();
        }
    }
}