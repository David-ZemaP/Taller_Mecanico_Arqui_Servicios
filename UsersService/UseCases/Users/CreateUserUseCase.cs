using Taller_Mecanico_Users.App.Services;
using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Entities;
using Taller_Mecanico_Users.Domain.Ports;
using Taller_Mecanico_Users.Framework.DTOs;
using Taller_Mecanico_Users.Framework.Services;

namespace Taller_Mecanico_Users.UseCases.Users
{
    public class CreateUserUseCase
    {
        private readonly IUsuarioLoginRepository _repository;
        private readonly IMailSender _mailSender;
        private readonly UsernameGenerator _usernameGenerator;

        public CreateUserUseCase(
            IUsuarioLoginRepository repository,
            IMailSender mailSender,
            UsernameGenerator usernameGenerator)
        {
            _repository = repository;
            _mailSender = mailSender;
            _usernameGenerator = usernameGenerator;
        }

        public async Task<Result<UsuarioLogin>> ExecuteAsync(CreateUserRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombres))
                return Result<UsuarioLogin>.Failure(ErrorCodes.ValidationRequired, "El nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.PrimerApellido))
                return Result<UsuarioLogin>.Failure(ErrorCodes.ValidationRequired, "El primer apellido es obligatorio.");
            if (string.IsNullOrWhiteSpace(dto.Email))
                return Result<UsuarioLogin>.Failure(ErrorCodes.ValidationRequired, "El correo electrónico es obligatorio.");

            var existente = await _repository.GetByEmailAsync(dto.Email);
            if (existente != null)
                return Result<UsuarioLogin>.Failure(ErrorCodes.UsuarioEmailDuplicado, "Ya existe un usuario con ese correo.");

            var username = await _usernameGenerator.GenerateAsync(dto.Nombres, dto.PrimerApellido);

            string plainPassword = PasswordSecurity.GenerateSecurePassword();
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword);

            var nuevoUsuario = UsuarioLogin.Crear(
                dto.EmpleadoId ?? 0,
                dto.Email,
                passwordHash,
                username,
                requiereCambioPassword: true
            );

            if (dto.EsCliente && dto.ClienteId.HasValue)
            {
                nuevoUsuario = UsuarioLogin.CrearParaCliente(dto.ClienteId.Value, dto.Email, passwordHash, username);
            }

            var addResult = await _repository.AddAsync(nuevoUsuario);
            if (addResult.IsFailure)
                return Result<UsuarioLogin>.Failure(addResult.ErrorCode ?? ErrorCodes.DbError, addResult.ErrorMessage ?? "Error al crear usuario.");

            await _mailSender.SendUserCredentialsAsync(dto.Email, username, plainPassword);

            return Result<UsuarioLogin>.Success(nuevoUsuario);
        }
    }
}
