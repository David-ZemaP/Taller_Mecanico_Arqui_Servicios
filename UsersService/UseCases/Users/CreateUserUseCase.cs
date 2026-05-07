using Taller_Mecanico_Users.Domain.Common;
using System;
using System.Threading.Tasks;
using Taller_Mecanico_Users.Domain.Entities;
using Taller_Mecanico_Users.Domain.Ports;
using Microsoft.Extensions.Logging;

namespace Taller_Mecanico_Users.UseCases.Users
{
    public class UserCreationResult
    {
        public UsuarioLogin User { get; init; } = null!;
        public string PlainPassword { get; init; } = string.Empty;
    }

    public class CreateUserUseCase
    {
        private readonly IUsuarioLoginRepository _repository;
        private readonly Domain.Ports.IMailSender _mailSender;
        private readonly Domain.Ports.IPasswordSecurity _passwordSecurity;
        private readonly Domain.Ports.IPasswordHasher _passwordHasher;
        private readonly ILogger<CreateUserUseCase> _logger;

        public CreateUserUseCase(
            IUsuarioLoginRepository repository,
            Domain.Ports.IMailSender mailSender,
            Domain.Ports.IPasswordSecurity passwordSecurity,
            Domain.Ports.IPasswordHasher passwordHasher,
            ILogger<CreateUserUseCase> logger)
        {
            _repository = repository;
            _mailSender = mailSender;
            _passwordSecurity = passwordSecurity;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<Result<UserCreationResult>> ExecuteAsync(int empleadoId, string email, string? plainPasswordProvided = null)
        {
            // 0. Validar que el empleado no tenga ya un login
            var existingByEmployee = await _repository.GetByEmpleadoIdAsync(empleadoId);
            if (existingByEmployee != null)
            {
                return Result<UserCreationResult>.Failure(ErrorCodes.UsuarioEmpleadoDuplicado, "El empleado ya tiene un usuario asignado.");
            }

            // 0. Validar email duplicado
            var existing = await _repository.GetByEmailAsync(email);
            if (existing != null)
            {
                return Result<UserCreationResult>.Failure(ErrorCodes.UsuarioEmailDuplicado, "El email ya está registrado.");
            }

            // 1. Usar la contraseña provista o generar una segura temporal
            string plainPassword = string.IsNullOrWhiteSpace(plainPasswordProvided)
                ? _passwordSecurity.GenerateSecurePassword()
                : plainPasswordProvided;
            
            // 2. Hashear la contraseña
            string passwordHash = _passwordHasher.HashPassword(plainPassword);

            // 3. Crear entidad forzando cambio de contraseña en primer acceso
            var nuevoUsuarioResult = UsuarioLogin.Crear(empleadoId, email, passwordHash, requiereCambioPassword: true);
            if (nuevoUsuarioResult.IsFailure)
            {
                return Result<UserCreationResult>.Failure(nuevoUsuarioResult.ErrorCode!, nuevoUsuarioResult.ErrorMessage!);
            }

            var nuevoUsuario = nuevoUsuarioResult.Value!;

            // 4. Persistir en el repositorio
            var addResult = await _repository.AddAsync(nuevoUsuario);
            if (addResult.IsFailure)
            {
                return Result<UserCreationResult>.Failure(addResult.ErrorCode ?? ErrorCodes.DbError, addResult.ErrorMessage ?? "Error al crear usuario.");
            }

            // 5. Enviar credenciales por correo (fallo de correo no cancela la creación)
            string mailBody = $@"📧 Bienvenido al Taller Mecánico

Se ha creado tu cuenta de usuario exitosamente.

🔑 Credenciales de acceso:
   Usuario: {email}
   Contraseña temporal: {plainPassword}

⚠️ IMPORTANTE: Debes cambiar tu contraseña al iniciar sesión por primera vez.

📞 ¿Necesitas ayuda? Contacta al administrador del sistema.";

            try
            {
                await _mailSender.SendEmailAsync(email, "🔧 Credenciales de Acceso - Taller Mecánico", mailBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo enviar el correo de bienvenida a {Email}", email);
            }

            return Result<UserCreationResult>.Success(new UserCreationResult { User = nuevoUsuario, PlainPassword = plainPassword });
        }
    }
}