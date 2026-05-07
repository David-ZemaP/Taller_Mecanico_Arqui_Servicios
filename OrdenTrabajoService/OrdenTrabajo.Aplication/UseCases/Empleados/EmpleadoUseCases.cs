using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Application.DTOs.Empleados;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Enums;
using Taller_Mecanico_Arqui.Domain.ValueObjects;
using Taller_Mecanico_Arqui.Application.Common;
using Taller_Mecanico_Arqui.Domain.Services;

namespace Taller_Mecanico_Arqui.Application.UseCases.Empleados
{
    public class CreateEmpleadoUseCase
    {
        private readonly IEmpleadoRepository _repository;
        private readonly IUsersServiceClient _usersServiceClient;
        private readonly ICurrentUserService _currentUser;

        public CreateEmpleadoUseCase(IEmpleadoRepository repository, IUsersServiceClient usersServiceClient, ICurrentUserService currentUser)
        {
            _repository = repository;
            _usersServiceClient = usersServiceClient;
            _currentUser = currentUser;
        }

        public async Task<Result<Empleado>> ExecuteAsync(CreateEmpleadoDto dto)
        {
            var existingByCi = await _repository.GetByCiAsync(dto.CiNumero);
            if (existingByCi != null)
            {
                return Result<Empleado>.Failure(ErrorCodes.EmpleadoCiDuplicado, "Ya existe un empleado con ese CI.");
            }

            var nombreResult = NombreCompleto.Crear(dto.Nombres, dto.PrimerApellido, dto.SegundoApellido);
            if (nombreResult.IsFailure)
                return Result<Empleado>.Failure(nombreResult.ErrorCode ?? ErrorCodes.ValidationInvalidValue, nombreResult.ErrorMessage ?? "Nombre inválido.");

            var ciResult = DocumentoIdentidad.Crear(dto.CiNumero, dto.CiComplemento);
            if (ciResult.IsFailure)
                return Result<Empleado>.Failure(ciResult.ErrorCode ?? ErrorCodes.ValidationInvalidValue, ciResult.ErrorMessage ?? "CI inválido.");

            var estadoLaboralResult = ValidationHelper.ParseEnum<EstadoLaboral>(dto.EstadoLaboral, "Estado laboral no válido.", removeSpaces: true);
            if (estadoLaboralResult.IsFailure)
                return Result<Empleado>.Failure(estadoLaboralResult.ErrorCode ?? ErrorCodes.ValidationInvalidValue, estadoLaboralResult.ErrorMessage ?? "Estado laboral no válido.");

            var empleado = Empleado.Crear(nombreResult.Value, ciResult.Value, dto.Telefono, dto.Email, dto.FechaContratacion, dto.TipoEmpleado, estadoLaboralResult.Value);

            var currentUser = _currentUser.GetCurrentUserId() ?? _currentUser.GetCurrentUserEmail() ?? "system";
            empleado.SetAuditoriaCreacion(currentUser);

            var addResult = await _repository.AddAsync(empleado);
            if (addResult.IsFailure)
                return Result<Empleado>.Failure(addResult.ErrorCode ?? ErrorCodes.DbError, addResult.ErrorMessage ?? "No se pudo registrar el empleado.");

            empleado.SetId(addResult.Value);

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                await _usersServiceClient.CreateUsuarioForEmpleadoAsync(addResult.Value, dto.Email);
            }

            return Result<Empleado>.Success(empleado);
        }
    }

    public class UpdateEmpleadoUseCase
    {
        private readonly IEmpleadoRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public UpdateEmpleadoUseCase(IEmpleadoRepository repository, ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<Result> ExecuteAsync(UpdateEmpleadoDto dto)
        {
            var existing = await _repository.GetByIdAsync(dto.EmpleadoId);
            if (existing.IsFailure || existing.Value == null)
                return Result.Failure(ErrorCodes.EmpleadoNotFound, "Empleado no encontrado.");

            var byCi = await _repository.GetByCiAsync(dto.CiNumero);
            if (byCi != null && byCi.EmpleadoId != dto.EmpleadoId)
                return Result.Failure(ErrorCodes.EmpleadoCiDuplicado, "Ya existe otro empleado con ese CI.");

            var nombreResult = NombreCompleto.Crear(dto.Nombres, dto.PrimerApellido, dto.SegundoApellido);
            if (nombreResult.IsFailure)
                return Result.Failure(nombreResult.ErrorCode ?? ErrorCodes.ValidationInvalidValue, nombreResult.ErrorMessage ?? "Nombre inválido.");

            var ciResult = DocumentoIdentidad.Crear(dto.CiNumero, dto.CiComplemento);
            if (ciResult.IsFailure)
                return Result.Failure(ciResult.ErrorCode ?? ErrorCodes.ValidationInvalidValue, ciResult.ErrorMessage ?? "CI inválido.");

            var estadoLaboralResult = ValidationHelper.ParseEnum<EstadoLaboral>(dto.EstadoLaboral, "Estado laboral no válido.", removeSpaces: true);
            if (estadoLaboralResult.IsFailure)
                return Result.Failure(estadoLaboralResult.ErrorCode ?? ErrorCodes.ValidationInvalidValue, estadoLaboralResult.ErrorMessage ?? "Estado laboral no válido.");

            var empleado = Empleado.Crear(
                nombreResult.Value,
                ciResult.Value,
                dto.Telefono,
                dto.Email,
                dto.FechaContratacion,
                dto.TipoEmpleado,
                estadoLaboralResult.Value);
            empleado.SetId(existing.Value.EmpleadoId);

            var currentUser = _currentUser.GetCurrentUserId() ?? _currentUser.GetCurrentUserEmail() ?? "system";
            empleado.SetAuditoriaActualizacion(currentUser);

            return await _repository.UpdateAsync(empleado);
        }
    }

    public class GetEmpleadoByIdUseCase
    {
        private readonly IEmpleadoRepository _repository;

        public GetEmpleadoByIdUseCase(IEmpleadoRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<Empleado?>> ExecuteAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }

    public class GetAllEmpleadosUseCase
    {
        private readonly IEmpleadoRepository _repository;

        public GetAllEmpleadosUseCase(IEmpleadoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Empleado>> ExecuteAsync()
        {
            return await _repository.GetAllAsync();
        }
    }

    public class DeleteEmpleadoUseCase
    {
        private readonly IEmpleadoRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public DeleteEmpleadoUseCase(IEmpleadoRepository repository, ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<Result> ExecuteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing.IsFailure || existing.Value == null)
                return Result.Failure(ErrorCodes.EmpleadoNotFound, "Empleado no encontrado.");

            var currentUser = _currentUser.GetCurrentUserId() ?? _currentUser.GetCurrentUserEmail() ?? "system";
            existing.Value.MarcarEliminado(currentUser);

            await _repository.DeleteAsync(id, currentUser);
            return Result.Success();
        }
    }
}