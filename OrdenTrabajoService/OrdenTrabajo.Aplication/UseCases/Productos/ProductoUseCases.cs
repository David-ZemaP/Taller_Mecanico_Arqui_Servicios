using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Application.DTOs.Productos;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Services;

namespace Taller_Mecanico_Arqui.Application.UseCases.Productos
{
    public class CreateProductoUseCase
    {
        private readonly IProductoRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public CreateProductoUseCase(IProductoRepository repository, ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<Result<int>> ExecuteAsync(CreateProductoDto dto)
        {
            if (await _repository.GetByNombreAsync(dto.Nombre) != null)
            {
                return Result<int>.Failure(ErrorCodes.ValidationDuplicateValue, "Ya existe un producto con ese nombre.");
            }

            var producto = Producto.Crear(dto.Nombre, dto.Precio, dto.Stock);
            
            // Set auditoria
            var currentUser = _currentUser.GetCurrentUserId() ?? _currentUser.GetCurrentUserEmail() ?? "system";
            producto.SetAuditoriaCreacion(currentUser);
            
            var result = await _repository.AddAsync(producto);

            if (result.IsFailure)
                return Result<int>.Failure(result.ErrorCode ?? ErrorCodes.DbError, result.ErrorMessage ?? "No se pudo registrar el producto.");

            return result;
        }
    }

    public class UpdateProductoUseCase
    {
        private readonly IProductoRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public UpdateProductoUseCase(IProductoRepository repository, ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<Result> ExecuteAsync(UpdateProductoDto dto)
        {
            var existing = await _repository.GetByIdAsync(dto.ProductoId);
            if (existing.IsFailure || existing.Value == null)
            {
                return Result.Failure(ErrorCodes.ProductoNotFound, "Producto no encontrado.");
            }

            var producto = existing.Value;
            producto.ActualizarDatos(dto.Nombre, dto.Precio, dto.Stock);

            // Set auditoria
            var currentUser = _currentUser.GetCurrentUserId() ?? _currentUser.GetCurrentUserEmail() ?? "system";
            producto.SetAuditoriaActualizacion(currentUser);

            return await _repository.UpdateAsync(producto);
        }
    }

    public class GetProductoByIdUseCase
    {
        private readonly IProductoRepository _repository;

        public GetProductoByIdUseCase(IProductoRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<Producto?>> ExecuteAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }

    public class GetAllProductosUseCase
    {
        private readonly IProductoRepository _repository;

        public GetAllProductosUseCase(IProductoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Producto>> ExecuteAsync()
        {
            return await _repository.GetAllAsync();
        }
    }

    public class DeleteProductoUseCase
    {
        private readonly IProductoRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public DeleteProductoUseCase(IProductoRepository repository, ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<Result> ExecuteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing.IsFailure || existing.Value == null)
            {
                return Result.Failure(ErrorCodes.ProductoNotFound, "Producto no encontrado.");
            }

            // Set auditoria
            var currentUser = _currentUser.GetCurrentUserId() ?? _currentUser.GetCurrentUserEmail() ?? "system";
            existing.Value.MarcarEliminado(currentUser);

            await _repository.DeleteAsync(id, currentUser);
            return Result.Success();
        }
    }
}
