using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Application.DTOs.Vehiculos;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Services;

namespace Taller_Mecanico_Arqui.Application.UseCases.Vehiculos
{
    public class CreateVehiculoUseCase
    {
        private readonly IVehiculoRepository _repository;
        private readonly IClienteRepository _clienteRepository;
        private readonly ICurrentUserService _currentUser;

        public CreateVehiculoUseCase(IVehiculoRepository repository, IClienteRepository clienteRepository, ICurrentUserService currentUser)
        {
            _repository = repository;
            _clienteRepository = clienteRepository;
            _currentUser = currentUser;
        }

        public async Task<Result<int>> ExecuteAsync(CreateVehiculoDto dto)
        {
            var cliente = await _clienteRepository.GetByIdAsync(dto.ClienteId);
            if (cliente.IsFailure || cliente.Value == null)
            {
                return Result<int>.Failure(ErrorCodes.ClienteNotFound, "El cliente no existe.");
            }

            if (await _repository.GetByPlacaAsync(dto.Placa) != null)
            {
                return Result<int>.Failure(ErrorCodes.VehiculoPlacaDuplicada, "Ya existe un vehículo con esa placa.");
            }

            var vehiculo = Vehiculo.Crear(dto.ClienteId, dto.Placa, dto.MarcaId, dto.ModeloId, dto.ColorVehiculoId, dto.Anio);
            
            // Set auditoria
            var currentUser = _currentUser.GetCurrentUserId() ?? _currentUser.GetCurrentUserEmail() ?? "system";
            vehiculo.SetAuditoriaCreacion(currentUser);
            
            var result = await _repository.AddAsync(vehiculo);

            if (result.IsFailure)
                return Result<int>.Failure(result.ErrorCode ?? ErrorCodes.DbError, result.ErrorMessage ?? "No se pudo registrar el vehículo.");

            return result;
        }
    }

    public class UpdateVehiculoUseCase
    {
        private readonly IVehiculoRepository _repository;
        private readonly IClienteRepository _clienteRepository;
        private readonly ICurrentUserService _currentUser;

        public UpdateVehiculoUseCase(IVehiculoRepository repository, IClienteRepository clienteRepository, ICurrentUserService currentUser)
        {
            _repository = repository;
            _clienteRepository = clienteRepository;
            _currentUser = currentUser;
        }

        public async Task<Result> ExecuteAsync(UpdateVehiculoDto dto)
        {
            var existing = await _repository.GetByIdAsync(dto.VehiculoId);
            if (existing.IsFailure || existing.Value == null)
            {
                return Result.Failure(ErrorCodes.VehiculoNotFound, "Vehículo no encontrado.");
            }

            var cliente = await _clienteRepository.GetByIdAsync(dto.ClienteId);
            if (cliente.IsFailure || cliente.Value == null)
            {
                return Result.Failure(ErrorCodes.ClienteNotFound, "El cliente no existe.");
            }

            var vehiculo = existing.Value;
            vehiculo.ActualizarDatos(dto.Placa, dto.MarcaId, dto.ModeloId, dto.ColorVehiculoId, dto.Anio);

            // Set auditoria
            var currentUser = _currentUser.GetCurrentUserId() ?? _currentUser.GetCurrentUserEmail() ?? "system";
            vehiculo.SetAuditoriaActualizacion(currentUser);

            return await _repository.UpdateAsync(vehiculo);
        }
    }

    public class GetVehiculoByIdUseCase
    {
        private readonly IVehiculoRepository _repository;

        public GetVehiculoByIdUseCase(IVehiculoRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<Vehiculo?>> ExecuteAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }
    }

    public class GetAllVehiculosUseCase
    {
        private readonly IVehiculoRepository _repository;

        public GetAllVehiculosUseCase(IVehiculoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Vehiculo>> ExecuteAsync()
        {
            return await _repository.GetAllAsync();
        }
    }

    public class GetVehiculosByClienteIdUseCase
    {
        private readonly IVehiculoRepository _repository;

        public GetVehiculosByClienteIdUseCase(IVehiculoRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Vehiculo>> ExecuteAsync(int clienteId)
        {
            return await _repository.GetByClienteIdAsync(clienteId);
        }
    }

    public class DeleteVehiculoUseCase
    {
        private readonly IVehiculoRepository _repository;
        private readonly ICurrentUserService _currentUser;

        public DeleteVehiculoUseCase(IVehiculoRepository repository, ICurrentUserService currentUser)
        {
            _repository = repository;
            _currentUser = currentUser;
        }

        public async Task<Result> ExecuteAsync(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing.IsFailure || existing.Value == null)
            {
                return Result.Failure(ErrorCodes.VehiculoNotFound, "Vehículo no encontrado.");
            }

            // Set auditoria
            var currentUser = _currentUser.GetCurrentUserId() ?? _currentUser.GetCurrentUserEmail() ?? "system";
            existing.Value.MarcarEliminado(currentUser);

            await _repository.DeleteAsync(id, currentUser);
            return Result.Success();
        }
    }
}
