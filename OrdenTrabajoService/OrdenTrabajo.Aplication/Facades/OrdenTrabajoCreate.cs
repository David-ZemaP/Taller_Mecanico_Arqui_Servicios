using System.Globalization;
using OrdenTrabajoService.Application.DTOs.OrdenTrabajo;
using OrdenTrabajoService.Application.DTOs.Vehiculo;
using OrdenTrabajoService.Application.UseCases;
using OrdenTrabajoService.Domain.Interfaces;
using Taller_Mecanico_Users.Domain.Common;

namespace OrdenTrabajoService.Application.Facades
{
    public class OrdenTrabajoCreate
    {
        private readonly GetAllOrdenesTrabajoUseCase _getAllUseCase;
        private readonly GetOrdenTrabajoByIdUseCase _getByIdUseCase;
        private readonly CreateOrdenTrabajoUseCase _createUseCase;
        private readonly UpdateOrdenTrabajoUseCase _updateUseCase;
        private readonly IVehiculoRepository _vehiculoRepository;

        public OrdenTrabajoCreate(
            GetAllOrdenesTrabajoUseCase getAllUseCase,
            GetOrdenTrabajoByIdUseCase getByIdUseCase,
            CreateOrdenTrabajoUseCase createUseCase,
            UpdateOrdenTrabajoUseCase updateUseCase,
            IVehiculoRepository vehiculoRepository)
        {
            _getAllUseCase = getAllUseCase;
            _getByIdUseCase = getByIdUseCase;
            _createUseCase = createUseCase;
            _updateUseCase = updateUseCase;
            _vehiculoRepository = vehiculoRepository;
        }

        public async Task<IEnumerable<OrdenTrabajoListDto>> GetAllAsync()
        {
            var ordenes = await _getAllUseCase.ExecuteAsync();
            return ordenes.Select(o => new OrdenTrabajoListDto
            {
                OrdenTrabajoId = o.OrdenTrabajoId,
                VehiculoId = o.VehiculoId,
                VehiculoPlaca = o.Vehiculo?.Placa ?? $"Vehículo #{o.VehiculoId}",
                FechaIngreso = o.FechaIngreso,
                FechaEntrega = o.FechaEntrega,
                EstadoTrabajo = o.EstadoTrabajo.ToString(),
                EstadoPago = o.EstadoPago.ToString(),
                EstadoVehiculo = o.EstadoVehiculo,
                Total = o.ProductosUsados.Sum(p => p.Subtotal) + o.ServiciosRealizados.Sum(s => s.Subtotal),
                IsDeleted = o.IsDeleted
            }).ToList();
        }

        public async Task<Result<OrdenTrabajoDetalleDto>> GetDetalleAsync(int id)
        {
            var result = await _getByIdUseCase.ExecuteAsync(id);
            if (result.IsFailure)
                return Result<OrdenTrabajoDetalleDto>.Failure(result.ErrorCode!, result.ErrorMessage!);

            var o = result.Value!;
            var totalProductos = o.ProductosUsados.Sum(p => p.Subtotal);
            var totalServicios = o.ServiciosRealizados.Sum(s => s.Subtotal);

            var dto = new OrdenTrabajoDetalleDto
            {
                OrdenTrabajoId = o.OrdenTrabajoId,
                ClienteId = o.Vehiculo?.ClienteId ?? 0,
                ClienteCi = o.Vehiculo?.ClienteCi ?? "No disponible",
                ClienteNombre = o.Vehiculo?.ClienteNombre ?? "No disponible",
                VehiculoId = o.VehiculoId,
                Placa = o.Vehiculo?.Placa ?? "No disponible",
                FechaIngreso = o.FechaIngreso.ToString("yyyy-MM-dd"),
                FechaEntrega = o.FechaEntrega?.ToString("yyyy-MM-dd"),
                EstadoTrabajo = o.EstadoTrabajo.ToString(),
                EstadoPago = o.EstadoPago.ToString(),
                EstadoVehiculo = o.EstadoVehiculo,
                Total = totalProductos + totalServicios,
                IsDeleted = o.IsDeleted,
                Productos = o.ProductosUsados.Select(p => new OrdenTrabajoDetalleProductoDto
                {
                    ProductoId = p.ProductoId,
                    Nombre = p.Nombre,
                    Cantidad = p.Cantidad,
                    PrecioUnitario = p.PrecioUnitario,
                    Subtotal = p.Subtotal
                }).ToList(),
                Servicios = o.ServiciosRealizados.Select(s => new OrdenTrabajoDetalleServicioDto
                {
                    ServicioId = s.ServicioId,
                    Nombre = s.Nombre,
                    Cantidad = s.Cantidad,
                    PrecioUnitario = s.PrecioUnitario,
                    Subtotal = s.Subtotal
                }).ToList()
            };

            return Result<OrdenTrabajoDetalleDto>.Success(dto);
        }

        public async Task<IEnumerable<VehiculoLookupDto>> BuscarVehiculosAsync(string? term, int? clienteId = null)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Array.Empty<VehiculoLookupDto>();

            var vehiculos = await _vehiculoRepository.BuscarPorPlacaAsync(term, clienteId);

            return vehiculos
                .Select(v => new VehiculoLookupDto { Id = v.VehiculoId, Text = v.Placa })
                .Take(15)
                .ToList();
        }

        public async Task<Result<int>> RegistrarAsync(CreateOrdenTrabajoDto dto)
        {
            var result = await _createUseCase.ExecuteAsync(dto);
            if (result.IsFailure)
                return Result<int>.Failure(result.ErrorCode!, result.ErrorMessage!);
            return Result<int>.Success(result.Value!.OrdenTrabajoId);
        }

        public Task<Result> ActualizarAsync(UpdateOrdenTrabajoDto dto)
            => _updateUseCase.ExecuteAsync(dto);
    }
}
