using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Domain.Enums;
using Taller_Mecanico_Arqui.Application.DTOs.OrdenTrabajo;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Application.Common;
using Taller_Mecanico_Arqui.Domain.Services;

namespace Taller_Mecanico_Arqui.Application.UseCases.OrdenTrabajo
{
    public class UpdateOrdenTrabajoUseCase
    {
        private readonly IOrdenTrabajoRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public UpdateOrdenTrabajoUseCase(IOrdenTrabajoRepository repository, ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<Result> ExecuteAsync(UpdateOrdenTrabajoDto dto)
        {
            var ordenResult = await _repository.GetByIdAsync(dto.OrdenTrabajoId);
            if (ordenResult.IsFailure)
                return Result.Failure(ordenResult.ErrorCode ?? ErrorCodes.DbError, ordenResult.ErrorMessage ?? "Error al obtener orden de trabajo.");

            if (ordenResult.Value == null)
                return Result.Failure(ErrorCodes.OrdenTrabajoNotFound, $"Orden de trabajo con ID {dto.OrdenTrabajoId} no encontrada");

            var orden = ordenResult.Value;

            var estadoTrabajoResult = ValidationHelper.ParseEnum<EstadoTrabajo>(
                dto.EstadoTrabajo,
                "Estado de trabajo no válido.",
                removeSpaces: true);

            if (estadoTrabajoResult.IsFailure)
                return Result.Failure(estadoTrabajoResult.ErrorCode ?? ErrorCodes.ValidationInvalidValue, estadoTrabajoResult.ErrorMessage ?? "Estado de trabajo no válido.");

            var estadoPagoResult = ValidationHelper.ParseEnum<EstadoPago>(
                dto.EstadoPago,
                "Estado de pago no válido.");

            if (estadoPagoResult.IsFailure)
                return Result.Failure(estadoPagoResult.ErrorCode ?? ErrorCodes.ValidationInvalidValue, estadoPagoResult.ErrorMessage ?? "Estado de pago no válido.");

            var estadoTrabajo = estadoTrabajoResult.Value;
            var estadoPago = estadoPagoResult.Value;

            orden.ActualizarEstadoTrabajo(estadoTrabajo);
            orden.ActualizarEstadoPago(estadoPago);
            orden.ActualizarTotal(dto.Total);

            // Set auditoria
            var currentUser = _currentUser.GetCurrentUserId() ?? _currentUser.GetCurrentUserEmail() ?? "system";
            orden.SetAuditoriaActualizacion(currentUser);

            return await _repository.UpdateAsync(orden);
        }
    }
}
