using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Application.DTOs.Clientes;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Enums;
using Taller_Mecanico_Arqui.Domain.ValueObjects;
using Taller_Mecanico_Arqui.Application.Common;
using Taller_Mecanico_Arqui.Domain.Services;

namespace Taller_Mecanico_Arqui.Application.UseCases.Clientes
{
    public class CreateClienteUseCase
    {
        private readonly IClienteRepository _repository;
        private readonly IUsersServiceClient _usersServiceClient;
        private readonly ICurrentUserService _currentUser;

        public CreateClienteUseCase(IClienteRepository repository, IUsersServiceClient usersServiceClient, ICurrentUserService currentUser)
        {
            _repository = repository;
            _usersServiceClient = usersServiceClient;
            _currentUser = currentUser;
        }

        public async Task<Result<Cliente>> ExecuteAsync(CreateClienteDto dto)
        {
            // Validate CI not duplicate
            var existingByCi = await _repository.GetByCiAsync(dto.CiNumero);
            if (existingByCi != null)
            {
                return Result<Cliente>.Failure(ErrorCodes.ClienteCiDuplicado, "Ya existe un cliente con ese CI.");
            }

            // Validate email not duplicate
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var existingByEmail = await _repository.GetByEmailAsync(dto.Email);
                if (existingByEmail != null)
                {
                    return Result<Cliente>.Failure(ErrorCodes.UsuarioEmailDuplicado, "Ya existe un cliente con ese email.");
                }
            }

            // Build value objects
            var nombreResult = NombreCompleto.Crear(dto.Nombres, dto.PrimerApellido, dto.SegundoApellido);
            if (nombreResult.IsFailure)
                return Result<Cliente>.Failure(nombreResult.ErrorCode ?? ErrorCodes.ValidationInvalidValue, nombreResult.ErrorMessage ?? "Nombre inválido.");

            var ciResult = DocumentoIdentidad.Crear(dto.CiNumero, dto.CiComplemento);
            if (ciResult.IsFailure)
                return Result<Cliente>.Failure(ciResult.ErrorCode ?? ErrorCodes.ValidationInvalidValue, ciResult.ErrorMessage ?? "CI inválido.");

            var tipoClienteResult = ValidationHelper.ParseEnum<TipoCliente>(dto.TipoCliente, "Tipo de cliente no válido.", removeSpaces: true);
            if (tipoClienteResult.IsFailure)
                return Result<Cliente>.Failure(tipoClienteResult.ErrorCode ?? ErrorCodes.ValidationInvalidValue, tipoClienteResult.ErrorMessage ?? "Tipo de cliente no válido.");

            var cliente = Cliente.Crear(nombreResult.Value, ciResult.Value, dto.Telefono, dto.Email, tipoClienteResult.Value);

            // Set auditoria
            var currentUser = _currentUser.GetCurrentUserId() ?? _currentUser.GetCurrentUserEmail() ?? "system";
            cliente.SetAuditoriaCreacion(currentUser);

            var addResult = await _repository.AddAsync(cliente);
            if (addResult.IsFailure)
                return Result<Cliente>.Failure(addResult.ErrorCode ?? ErrorCodes.DbError, addResult.ErrorMessage ?? "No se pudo registrar el cliente.");

            // Set the generated ID on the cliente entity
            cliente.GetType().GetProperty("ClienteId")?.SetValue(cliente, addResult.Value);

            // Try to create user account in UsersService (non-blocking)
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var userResult = await _usersServiceClient.CreateUsuarioForClienteAsync(addResult.Value, dto.Email);
                if (userResult.IsSuccess && userResult.Value > 0)
                {
                    cliente.AsignarUsuarioLogin(userResult.Value);
                    await _repository.UpdateAsync(cliente);
                }
                // If user creation fails, we still return the created cliente
                // The user can be created manually later
            }

            return Result<Cliente>.Success(cliente);
        }
    }

    public class UpdateClienteUseCase
    {
        private readonly IClienteRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public UpdateClienteUseCase(IClienteRepository repository, ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<Result> ExecuteAsync(UpdateClienteDto dto)
        {
            var existing = await _repository.GetByIdAsync(dto.ClienteId);
            if (existing.IsFailure || existing.Value == null)
            {
                return Result.Failure(ErrorCodes.ClienteNotFound, "Cliente no encontrado.");
            }

            // Check CI uniqueness (exclude current client)
            var byCi = await _repository.GetByCiAsync(dto.CiNumero);
            if (byCi != null && byCi.ClienteId != dto.ClienteId)
            {
                return Result.Failure(ErrorCodes.ClienteCiDuplicado, "Ya existe otro cliente con ese CI.");
            }

            var nombreResult = NombreCompleto.Crear(dto.Nombres, dto.PrimerApellido, dto.SegundoApellido);
            if (nombreResult.IsFailure)
                return Result.Failure(nombreResult.ErrorCode ?? ErrorCodes.ValidationInvalidValue, nombreResult.ErrorMessage ?? "Nombre inválido.");

            var ciResult = DocumentoIdentidad.Crear(dto.CiNumero, dto.CiComplemento);
            if (ciResult.IsFailure)
                return Result.Failure(ciResult.ErrorCode ?? ErrorCodes.ValidationInvalidValue, ciResult.ErrorMessage ?? "CI inválido.");

            var tipoClienteResult = ValidationHelper.ParseEnum<TipoCliente>(dto.TipoCliente, "Tipo de cliente no válido.", removeSpaces: true);
            if (tipoClienteResult.IsFailure)
                return Result.Failure(tipoClienteResult.ErrorCode ?? ErrorCodes.ValidationInvalidValue, tipoClienteResult.ErrorMessage ?? "Tipo de cliente no válido.");

            var cliente = existing.Value;
            cliente.ActualizarDatos(nombreResult.Value, ciResult.Value, dto.Telefono, dto.Email, tipoClienteResult.Value);

            // Set auditoria
            var currentUser = _currentUser.GetCurrentUserId() ?? _currentUser.GetCurrentUserEmail() ?? "system";
            cliente.SetAuditoriaActualizacion(currentUser);

            return await _repository.UpdateAsync(cliente);
        }
    }

    public class GetClienteByIdUseCase
    {
        private readonly IClienteRepository _repository;

        public GetClienteByIdUseCase(IClienteRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<Cliente?>> ExecuteAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }

    public class GetAllClientesUseCase
    {
        private readonly IClienteRepository _repository;

        public GetAllClientesUseCase(IClienteRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Cliente>> ExecuteAsync()
        {
            return await _repository.GetAllAsync();
        }
    }

    public class DeleteClienteUseCase
    {
        private readonly IClienteRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public DeleteClienteUseCase(IClienteRepository repository, ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<Result> ExecuteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing.IsFailure || existing.Value == null)
            {
                return Result.Failure(ErrorCodes.ClienteNotFound, "Cliente no encontrado.");
            }

            // Set auditoria before delete
            var currentUser = _currentUser.GetCurrentUserId() ?? _currentUser.GetCurrentUserEmail() ?? "system";
            existing.Value.MarcarEliminado(currentUser);

            await _repository.DeleteAsync(id, currentUser);
            return Result.Success();
        }
    }
}
