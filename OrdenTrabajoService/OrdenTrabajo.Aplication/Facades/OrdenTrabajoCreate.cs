using System.Globalization;
using System.Text.Json;
using Taller_Mecanico_Arqui.Application.Common;
using Taller_Mecanico_Arqui.Application.DTOs.OrdenTrabajo;
using Taller_Mecanico_Arqui.Application.UseCases.OrdenTrabajo;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Enums;
using Taller_Mecanico_Arqui.Domain.Ports;

namespace Taller_Mecanico_Arqui.Application.Facades
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
            return ordenes.Select(ToListDto).ToList();
        }

        public async Task<Result<OrdenTrabajoDetalleDto>> GetDetalleAsync(int id)
        {
            var ordenResult = await _getByIdUseCase.ExecuteAsync(id);
            if (ordenResult.IsFailure)
                return Result<OrdenTrabajoDetalleDto>.Failure(ordenResult.ErrorCode ?? ErrorCodes.DbError, ordenResult.ErrorMessage ?? "Error al consultar orden.");

            return Result<OrdenTrabajoDetalleDto>.Success(ToDetalleDto(ordenResult.Value!));
        }

        public async Task<IEnumerable<VehiculoLookupDto>> BuscarVehiculosAsync(string? term, int? clienteId = null)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Array.Empty<VehiculoLookupDto>();

            term = term.ToLower(CultureInfo.InvariantCulture);

            var vehiculos = await _vehiculoRepository.GetAllAsync();
            var query = vehiculos
                .Where(v => !v.IsDeleted && v.Placa.ToLower(CultureInfo.InvariantCulture).Contains(term, StringComparison.OrdinalIgnoreCase));

            if (clienteId.HasValue && clienteId.Value > 0)
            {
                query = query.Where(v => v.ClienteId == clienteId.Value);
            }

            return query
                .Select(v => new VehiculoLookupDto
                {
                    Id = v.VehiculoId,
                    Text = v.Placa
                })
                .Take(15)
                .ToList();
        }

        public async Task<Result<int>> RegistrarProcesoPrincipalAsync(OrdenTrabajoFormDto dto)
        {
            if (dto.OrdenTrabajoId == 0)
            {
                var createDto = ToCreateDto(dto);
                var createResult = await _createUseCase.ExecuteAsync(createDto);
                if (createResult.IsFailure)
                    return Result<int>.Failure(createResult.ErrorCode ?? ErrorCodes.DbError, createResult.ErrorMessage ?? "No se pudo registrar la orden de trabajo.");

                return Result<int>.Success(createResult.Value);
            }

            var updateResult = await _updateUseCase.ExecuteAsync(ToUpdateDto(dto));
            if (updateResult.IsFailure)
                return Result<int>.Failure(updateResult.ErrorCode ?? ErrorCodes.DbError, updateResult.ErrorMessage ?? "No se pudo actualizar la orden de trabajo.");

            return Result<int>.Success(dto.OrdenTrabajoId);
        }

        public Task<Result<int>> SaveAsync(OrdenTrabajoFormDto dto)
            => RegistrarProcesoPrincipalAsync(dto);

        public List<string> GetEstadoTrabajoOptions()
            => Enum.GetNames<EstadoTrabajo>().ToList();

        public List<string> GetEstadoPagoOptions()
            => Enum.GetNames<EstadoPago>().ToList();

        private static CreateOrdenTrabajoDto ToCreateDto(OrdenTrabajoFormDto dto)
        {
            var productos = DeserializeOrEmpty<CreateOrdenTrabajoProductoDto>(dto.ProductosJson);
            var servicios = DeserializeOrEmpty<CreateOrdenTrabajoServicioDto>(dto.ServiciosJson);
            var totalProductos = productos.Sum(p => p.Cantidad * (p.PrecioUnitario ?? 0));
            var totalServicios = servicios.Sum(s => s.Cantidad * (s.PrecioUnitario ?? 0));

            return new CreateOrdenTrabajoDto
            {
                VehiculoId = dto.VehiculoId,
                FechaIngreso = dto.FechaIngreso,
                EstadoVehiculo = dto.EstadoVehiculo,
                EstadoTrabajo = dto.EstadoTrabajo,
                EstadoPago = dto.EstadoPago,
                Total = totalProductos + totalServicios,
                Productos = productos,
                Servicios = servicios
            };
        }

        private static UpdateOrdenTrabajoDto ToUpdateDto(OrdenTrabajoFormDto dto)
        {
            var productos = DeserializeOrEmpty<CreateOrdenTrabajoProductoDto>(dto.ProductosJson);
            var servicios = DeserializeOrEmpty<CreateOrdenTrabajoServicioDto>(dto.ServiciosJson);
            var totalProductos = productos.Sum(p => p.Cantidad * (p.PrecioUnitario ?? 0));
            var totalServicios = servicios.Sum(s => s.Cantidad * (s.PrecioUnitario ?? 0));

            return new UpdateOrdenTrabajoDto
            {
                OrdenTrabajoId = dto.OrdenTrabajoId,
                VehiculoId = dto.VehiculoId,
                FechaIngreso = dto.FechaIngreso,
                FechaEntrega = dto.FechaEntrega,
                EstadoTrabajo = dto.EstadoTrabajo,
                EstadoPago = dto.EstadoPago,
                EstadoVehiculo = dto.EstadoVehiculo,
                Total = totalProductos + totalServicios
            };
        }

        private static OrdenTrabajoListDto ToListDto(OrdenTrabajo orden)
        {
            return new OrdenTrabajoListDto
            {
                OrdenTrabajoId = orden.OrdenTrabajoId,
                VehiculoId = orden.VehiculoId,
                VehiculoPlaca = orden.Vehiculo?.Placa ?? $"Vehículo #{orden.VehiculoId}",
                FechaIngreso = orden.FechaIngreso,
                FechaEntrega = orden.FechaEntrega,
                EstadoTrabajo = orden.EstadoTrabajo.ToString(),
                EstadoPago = orden.EstadoPago.ToString(),
                EstadoVehiculo = orden.EstadoVehiculo,
                Total = orden.ProductosUsados.Sum(p => p.Subtotal) + orden.ServiciosRealizados.Sum(s => s.Subtotal)
            };
        }

        private static OrdenTrabajoDetalleDto ToDetalleDto(OrdenTrabajo orden)
        {
            var totalProductos = orden.ProductosUsados.Sum(p => p.Subtotal);
            var totalServicios = orden.ServiciosRealizados.Sum(s => s.Subtotal);

            return new OrdenTrabajoDetalleDto
            {
                OrdenTrabajoId = orden.OrdenTrabajoId,
                ClienteId = orden.Vehiculo?.ClienteId ?? 0,
                ClienteCi = orden.Vehiculo?.Cliente?.Ci?.ToString() ?? "No disponible",
                VehiculoId = orden.VehiculoId,
                Placa = orden.Vehiculo?.Placa ?? "No disponible",
                ClienteNombre = orden.Vehiculo?.Cliente?.NombreCompleto?.ToString() ?? "No disponible",
                FechaIngreso = orden.FechaIngreso.ToString("yyyy-MM-dd"),
                FechaEntrega = orden.FechaEntrega?.ToString("yyyy-MM-dd"),
                EstadoTrabajo = orden.EstadoTrabajo.ToString(),
                EstadoPago = orden.EstadoPago.ToString(),
                EstadoVehiculo = orden.EstadoVehiculo,
                Total = totalProductos + totalServicios,
                Productos = orden.ProductosUsados.Select(p => new OrdenTrabajoDetalleProductoDto
                {
                    ProductoId = p.ProductoId,
                    Nombre = p.Nombre,
                    Cantidad = p.Cantidad,
                    PrecioUnitario = p.PrecioUnitario,
                    Subtotal = p.Subtotal
                }).ToList(),
                Servicios = orden.ServiciosRealizados.Select(s => new OrdenTrabajoDetalleServicioDto
                {
                    ServicioId = s.ServicioId,
                    Nombre = s.Nombre,
                    Cantidad = s.Cantidad,
                    PrecioUnitario = s.PrecioUnitario,
                    Subtotal = s.Subtotal
                }).ToList()
            };
        }

        private static List<T> DeserializeOrEmpty<T>(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<T>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<T>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<T>();
            }
            catch
            {
                return new List<T>();
            }
        }
    }
}