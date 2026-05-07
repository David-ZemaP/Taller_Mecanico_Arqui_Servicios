using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Application.DTOs.Servicios;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Services;

namespace Taller_Mecanico_Arqui.Application.UseCases.Servicios
{
    public class CreateServicioUseCase
    {
        private readonly IServicioRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public CreateServicioUseCase(IServicioRepository repository, ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<Result<int>> ExecuteAsync(CreateServicioDto dto)
        {
            if (await _repository.GetByNombreAsync(dto.Nombre) != null)
            {
                return Result<int>.Failure(ErrorCodes.ValidationDuplicateValue, "Ya existe un servicio con ese nombre.");
            }

            var servicio = Servicio.Crear(dto.Nombre, dto.Precio);
            
            // Set auditoria
            var currentUser = _currentUser.GetCurrentUserId() ?? _currentUser.GetCurrentUserEmail() ?? "system";
            servicio.SetAuditoriaCreacion(currentUser);
            
            var result = await _repository.AddAsync(servicio);

            if (result.IsFailure)
                return Result<int>.Failure(result.ErrorCode ?? ErrorCodes.DbError, result.ErrorMessage ?? "No se pudo registrar el servicio.");

            return result;
        }
    }

    public class UpdateServicioUseCase
    {
        private readonly IServicioRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public UpdateServicioUseCase(IServicioRepository repository, ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<Result> ExecuteAsync(UpdateServicioDto dto)
        {
            var existing = await _repository.GetByIdAsync(dto.ServicioId);
            if (existing.IsFailure || existing.Value == null)
            {
                return Result.Failure(ErrorCodes.NotFound, "Servicio no encontrado.");
            }

            var servicio = existing.Value;
            servicio.ActualizarDatos(dto.Nombre, dto.Precio);

            // Set auditoria
            var currentUser = _currentUser.GetCurrentUserId() ?? _currentUser.GetCurrentUserEmail() ?? "system";
            servicio.SetAuditoriaActualizacion(currentUser);

            return await _repository.UpdateAsync(servicio);
        }
    }

    public class GetServicioByIdUseCase
    {
        private readonly IServicioRepository _repository;

        public GetServicioByIdUseCase(IServicioRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<Servicio?>> ExecuteAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }

    public class GetAllServiciosUseCase
    {
        private readonly IServicioRepository _repository;

        public GetAllServiciosUseCase(IServicioRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Servicio>> ExecuteAsync()
        {
            return await _repository.GetAllAsync();
        }
    }

    public class DeleteServicioUseCase
    {
        private readonly IServicioRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public DeleteServicioUseCase(IServicioRepository repository, ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<Result> ExecuteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing.IsFailure || existing.Value == null)
            {
                return Result.Failure(ErrorCodes.NotFound, "Servicio no encontrado.");
            }

            // Set auditoria
            var currentUser = _currentUser.GetCurrentUserId() ?? _currentUser.GetCurrentUserEmail() ?? "system";
            existing.Value.MarcarEliminado(currentUser);

            await _repository.DeleteAsync(id, currentUser);
            return Result.Success();
        }
    }
}
