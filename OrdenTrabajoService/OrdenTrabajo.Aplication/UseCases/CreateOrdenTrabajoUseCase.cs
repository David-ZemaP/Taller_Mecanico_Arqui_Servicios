using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Domain.Enums;
using Taller_Mecanico_Arqui.Application.DTOs.OrdenTrabajo;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Application.Common;
using Taller_Mecanico_Arqui.Domain.Services;

namespace Taller_Mecanico_Arqui.Application.UseCases.OrdenTrabajo
{
    public class CreateOrdenTrabajoUseCase
    {
        private readonly IOrdenTrabajoRepository _repository;
        private readonly IRepository<Producto> _productoRepository;
        private readonly IRepository<Servicio> _servicioRepository;
        private readonly ICurrentUserService _currentUser;

        public CreateOrdenTrabajoUseCase(
            IOrdenTrabajoRepository repository,
            IRepository<Producto> productoRepository,
            IRepository<Servicio> servicioRepository,
            ICurrentUserService currentUser)
        {
            _repository = repository;
            _productoRepository = productoRepository;
            _servicioRepository = servicioRepository;
            _currentUser = currentUser;
        }

        public async Task<Result<int>> ExecuteAsync(CreateOrdenTrabajoDto dto)
        {
            var estadoTrabajoResult = ValidationHelper.ParseEnum<EstadoTrabajo>(
                dto.EstadoTrabajo,
                "Estado de trabajo no válido.",
                removeSpaces: true);

            if (estadoTrabajoResult.IsFailure)
                return Result<int>.Failure(estadoTrabajoResult.ErrorCode ?? ErrorCodes.ValidationInvalidValue, estadoTrabajoResult.ErrorMessage ?? "Estado de trabajo no válido.");

            var estadoPagoResult = ValidationHelper.ParseEnum<EstadoPago>(
                dto.EstadoPago,
                "Estado de pago no válido.");

            if (estadoPagoResult.IsFailure)
                return Result<int>.Failure(estadoPagoResult.ErrorCode ?? ErrorCodes.ValidationInvalidValue, estadoPagoResult.ErrorMessage ?? "Estado de pago no válido.");

            var estadoTrabajo = estadoTrabajoResult.Value;
            var estadoPago = estadoPagoResult.Value;

            var orden = Domain.Entities.OrdenTrabajo.Crear(
                dto.VehiculoId,
                dto.FechaIngreso,
                dto.EstadoVehiculo,
                estadoTrabajo,
                estadoPago);

            foreach (var productoDto in dto.Productos.Where(p => p.ProductoId > 0 && p.Cantidad > 0))
            {
                var productoResult = await _productoRepository.GetByIdAsync(productoDto.ProductoId);
                if (productoResult.IsFailure)
                {
                    return Result<int>.Failure(
                        productoResult.ErrorCode ?? ErrorCodes.DbError,
                        productoResult.ErrorMessage ?? "Error al consultar producto.");
                }

                var producto = productoResult.Value;
                if (producto == null)
                {
                    return Result<int>.Failure(
                        ErrorCodes.ValidationInvalidValue,
                        $"Producto con ID {productoDto.ProductoId} no encontrado.");
                }

                if (producto.Stock < productoDto.Cantidad)
                {
                    return Result<int>.Failure(
                        ErrorCodes.ValidationInvalidValue,
                        $"Stock insuficiente para el producto '{producto.Nombre}'. Stock actual: {producto.Stock}.");
                }

                var precioUnitario = productoDto.PrecioUnitario.GetValueOrDefault(producto.Precio);
                if (precioUnitario < 0)
                {
                    return Result<int>.Failure(
                        ErrorCodes.ValidationInvalidValue,
                        "El precio del producto (Bs.) no puede ser negativo.");
                }

                orden.AgregarProducto(producto.ProductoId, productoDto.Cantidad, precioUnitario);
            }

            foreach (var servicioDto in dto.Servicios.Where(s => s.ServicioId > 0 && s.Cantidad > 0))
            {
                var servicioResult = await _servicioRepository.GetByIdAsync(servicioDto.ServicioId);
                if (servicioResult.IsFailure)
                {
                    return Result<int>.Failure(
                        servicioResult.ErrorCode ?? ErrorCodes.DbError,
                        servicioResult.ErrorMessage ?? "Error al consultar servicio.");
                }

                var servicio = servicioResult.Value;
                if (servicio == null)
                {
                    return Result<int>.Failure(
                        ErrorCodes.ValidationInvalidValue,
                        $"Servicio con ID {servicioDto.ServicioId} no encontrado.");
                }

                var precioUnitario = servicioDto.PrecioUnitario.GetValueOrDefault(servicio.Precio);
                if (precioUnitario < 0)
                {
                    return Result<int>.Failure(
                        ErrorCodes.ValidationInvalidValue,
                        "El precio del servicio (Bs.) no puede ser negativo.");
                }

                orden.AgregarServicio(servicio.ServicioId, servicioDto.Cantidad, precioUnitario);
            }

            if (!dto.Productos.Any() && !dto.Servicios.Any())
            {
                orden.ActualizarTotal(dto.Total);
            }

            foreach (var mecanicoId in dto.MecanicosSeleccionados.Where(id => id > 0).Distinct())
            {
                var asignacion = Domain.Entities.OrdenTrabajoMecanico.Crear(0, mecanicoId);
                orden.AsignarMecanico(asignacion);
            }

            // Set auditoria
            var currentUser = _currentUser.GetCurrentUserId() ?? _currentUser.GetCurrentUserEmail() ?? "system";
            orden.SetAuditoriaCreacion(currentUser);

            var addResult = await _repository.AddAsync(orden);
            if (addResult.IsFailure)
                return Result<int>.Failure(addResult.ErrorCode ?? ErrorCodes.DbError, addResult.ErrorMessage ?? "No se pudo registrar la orden de trabajo.");

            return Result<int>.Success(addResult.Value);
        }
    }
}
