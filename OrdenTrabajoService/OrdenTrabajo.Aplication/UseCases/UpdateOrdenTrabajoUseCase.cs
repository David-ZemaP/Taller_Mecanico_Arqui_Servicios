using OrdenTrabajoService.Application.Common;
using OrdenTrabajoService.Application.DTOs.OrdenTrabajo;
using OrdenTrabajoService.Domain.Enums;
using OrdenTrabajoService.Domain.Interfaces;
using Taller_Mecanico_Users.Domain.Common;

namespace OrdenTrabajoService.Application.UseCases
{
    public class UpdateOrdenTrabajoUseCase
    {
        private readonly IOrdenTrabajoRepository _repository;

        public UpdateOrdenTrabajoUseCase(IOrdenTrabajoRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result> ExecuteAsync(UpdateOrdenTrabajoDto dto)
        {
            var ordenResult = await _repository.GetByIdAsync(dto.OrdenTrabajoId);
            if (ordenResult.IsFailure)
                return Result.Failure(ordenResult.ErrorCode!, ordenResult.ErrorMessage!);

            if (ordenResult.Value == null)
                return Result.Failure(ErrorCodes.OrdenTrabajoNotFound,
                    $"Orden de trabajo con ID {dto.OrdenTrabajoId} no encontrada.");

            var estadoTrabajoResult = ValidationHelper.ParseEnum<EstadoTrabajo>(
                dto.EstadoTrabajo, "Estado de trabajo no válido.", removeSpaces: true);
            if (estadoTrabajoResult.IsFailure)
                return Result.Failure(estadoTrabajoResult.ErrorCode!, estadoTrabajoResult.ErrorMessage!);

            var estadoPagoResult = ValidationHelper.ParseEnum<EstadoPago>(
                dto.EstadoPago, "Estado de pago no válido.");
            if (estadoPagoResult.IsFailure)
                return Result.Failure(estadoPagoResult.ErrorCode!, estadoPagoResult.ErrorMessage!);

            var orden = ordenResult.Value;
            orden.ActualizarEstadoTrabajo(estadoTrabajoResult.Value);
            orden.ActualizarEstadoPago(estadoPagoResult.Value);
            orden.ActualizarTotal(dto.Total);

            return await _repository.UpdateAsync(orden);
        }
    }
}

