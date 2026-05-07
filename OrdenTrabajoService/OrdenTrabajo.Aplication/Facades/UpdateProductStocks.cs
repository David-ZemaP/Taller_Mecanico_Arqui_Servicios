using OrdenTrabajoService.Application.DTOs.OrdenTrabajo;
using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Interfaces;
using Taller_Mecanico_Users.Domain.Common;

namespace OrdenTrabajoService.Application.Facades
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
            foreach (var dto in productos.Where(p => p.ProductoId > 0 && p.Cantidad > 0))
            {
                var result = await _productoRepository.GetByIdAsync(dto.ProductoId);
                if (result.IsFailure)
                    return Result.Failure(result.ErrorCode!, result.ErrorMessage!);

                var producto = result.Value;
                if (producto == null)
                    return Result.Failure(ErrorCodes.ValidationInvalidValue,
                        $"Producto con ID {dto.ProductoId} no encontrado.");

                if (producto.Stock < dto.Cantidad)
                    return Result.Failure(ErrorCodes.ValidationInvalidValue,
                        $"Stock insuficiente para '{producto.Nombre}'. Stock actual: {producto.Stock}.");

                producto.ReducirStock(dto.Cantidad);
                var updateResult = await _productoRepository.UpdateAsync(producto);
                if (updateResult.IsFailure)
                    return Result.Failure(updateResult.ErrorCode!, updateResult.ErrorMessage!);
            }
            return Result.Success();
        }

        public async Task<Result> RestoreAsync(IEnumerable<OrdenTrabajoProducto> productos)
        {
            foreach (var item in productos.Where(p => p.ProductoId > 0 && p.Cantidad > 0))
            {
                var result = await _productoRepository.GetByIdAsync(item.ProductoId);
                if (result.IsFailure)
                    return Result.Failure(result.ErrorCode!, result.ErrorMessage!);

                var producto = result.Value;
                if (producto == null)
                    return Result.Failure(ErrorCodes.ValidationInvalidValue,
                        $"Producto con ID {item.ProductoId} no encontrado.");

                producto.AumentarStock(item.Cantidad);
                var updateResult = await _productoRepository.UpdateAsync(producto);
                if (updateResult.IsFailure)
                    return Result.Failure(updateResult.ErrorCode!, updateResult.ErrorMessage!);
            }
            return Result.Success();
        }
    }
}
