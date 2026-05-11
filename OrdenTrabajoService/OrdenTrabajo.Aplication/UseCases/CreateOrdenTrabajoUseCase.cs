using OrdenTrabajoService.Application.Common;
using OrdenTrabajoService.Application.DTOs.OrdenTrabajo;
using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Enums;
using OrdenTrabajoService.Domain.Interfaces;
using Taller_Mecanico_Users.Domain.Common;

namespace OrdenTrabajoService.Application.UseCases
{
    public class CreateOrdenTrabajoUseCase
    {
        private readonly IOrdenTrabajoRepository _repository;
        private readonly IRepository<Producto> _productoRepository;
        private readonly IRepository<Servicio> _servicioRepository;

        public CreateOrdenTrabajoUseCase(
            IOrdenTrabajoRepository repository,
            IRepository<Producto> productoRepository,
            IRepository<Servicio> servicioRepository)
        {
            _repository = repository;
            _productoRepository = productoRepository;
            _servicioRepository = servicioRepository;
        }

        public async Task<Result<OrdenTrabajo>> ExecuteAsync(CreateOrdenTrabajoDto dto)
        {
            var estadoTrabajoResult = ValidationHelper.ParseEnum<EstadoTrabajo>(
                dto.EstadoTrabajo, "Estado de trabajo no válido.", removeSpaces: true);
            if (estadoTrabajoResult.IsFailure)
                return Result<OrdenTrabajo>.Failure(estadoTrabajoResult.ErrorCode!, estadoTrabajoResult.ErrorMessage!);

            var estadoPagoResult = ValidationHelper.ParseEnum<EstadoPago>(
                dto.EstadoPago, "Estado de pago no válido.");
            if (estadoPagoResult.IsFailure)
                return Result<OrdenTrabajo>.Failure(estadoPagoResult.ErrorCode!, estadoPagoResult.ErrorMessage!);

            var orden = OrdenTrabajo.Crear(
                dto.VehiculoId,
                dto.FechaIngreso,
                dto.EstadoVehiculo,
                estadoTrabajoResult.Value,
                estadoPagoResult.Value);

            foreach (var productoDto in dto.Productos.Where(p => p.ProductoId > 0 && p.Cantidad > 0))
            {
                var productoResult = await _productoRepository.GetByIdAsync(productoDto.ProductoId);
                if (productoResult.IsFailure)
                    return Result<OrdenTrabajo>.Failure(productoResult.ErrorCode!, productoResult.ErrorMessage!);

                var producto = productoResult.Value;
                if (producto == null)
                    return Result<OrdenTrabajo>.Failure(ErrorCodes.ValidationInvalidValue,
                        $"Producto con ID {productoDto.ProductoId} no encontrado.");

                if (producto.Stock < productoDto.Cantidad)
                    return Result<OrdenTrabajo>.Failure(ErrorCodes.ValidationInvalidValue,
                        $"Stock insuficiente para '{producto.Nombre}'. Stock actual: {producto.Stock}.");

                var precio = productoDto.PrecioUnitario.GetValueOrDefault(producto.Precio);
                if (precio < 0)
                    return Result<OrdenTrabajo>.Failure(ErrorCodes.ValidationInvalidValue,
                        "El precio del producto no puede ser negativo.");

                orden.AgregarProducto(producto.ProductoId, productoDto.Cantidad, precio);
            }

            foreach (var servicioDto in dto.Servicios.Where(s => s.ServicioId > 0 && s.Cantidad > 0))
            {
                var servicioResult = await _servicioRepository.GetByIdAsync(servicioDto.ServicioId);
                if (servicioResult.IsFailure)
                    return Result<OrdenTrabajo>.Failure(servicioResult.ErrorCode!, servicioResult.ErrorMessage!);

                var servicio = servicioResult.Value;
                if (servicio == null)
                    return Result<OrdenTrabajo>.Failure(ErrorCodes.ValidationInvalidValue,
                        $"Servicio con ID {servicioDto.ServicioId} no encontrado.");

                var precio = servicioDto.PrecioUnitario.GetValueOrDefault(servicio.Precio);
                if (precio < 0)
                    return Result<OrdenTrabajo>.Failure(ErrorCodes.ValidationInvalidValue,
                        "El precio del servicio no puede ser negativo.");

                orden.AgregarServicio(servicio.ServicioId, servicioDto.Cantidad, precio);
            }

            if (!dto.Productos.Any() && !dto.Servicios.Any())
                orden.ActualizarTotal(dto.Total);

            foreach (var mecanicoId in dto.MecanicosSeleccionados.Where(id => id > 0).Distinct())
                orden.AsignarMecanico(OrdenTrabajoMecanico.Crear(0, mecanicoId));

            var addResult = await _repository.AddAsync(orden);
            if (addResult.IsFailure)
                return Result<OrdenTrabajo>.Failure(addResult.ErrorCode!, addResult.ErrorMessage!);

            return Result<OrdenTrabajo>.Success(orden);
        }
    }
}
