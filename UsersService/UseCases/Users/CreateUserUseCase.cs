using Taller_Mecanico_Users.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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
        public IReadOnlyList<string> NotificationRecipients { get; init; } = Array.Empty<string>();
    }

    public class CreateUserUseCase
    {
        private readonly IUsuarioLoginRepository _repository;
        private readonly IEmpleadoRepository _empleadoRepository;
        private readonly Domain.Ports.IMailSender _mailSender;
        private readonly Domain.Ports.IPasswordSecurity _passwordSecurity;
        private readonly Domain.Ports.IPasswordHasher _passwordHasher;
        private readonly ILogger<CreateUserUseCase> _logger;

        public CreateUserUseCase(
            IUsuarioLoginRepository repository,
            IEmpleadoRepository empleadoRepository,
            Domain.Ports.IMailSender mailSender,
            Domain.Ports.IPasswordSecurity passwordSecurity,
            Domain.Ports.IPasswordHasher passwordHasher,
            ILogger<CreateUserUseCase> logger)
        {
            _repository = repository;
            _empleadoRepository = empleadoRepository;
            _mailSender = mailSender;
            _passwordSecurity = passwordSecurity;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<Result<UserCreationResult>> ExecuteAsync(int empleadoId, string email, string? plainPasswordProvided = null)
        {
            var empleado = await _empleadoRepository.GetByIdAsync(empleadoId);
            if (empleado is null)
            {
                return Result<UserCreationResult>.Failure(ErrorCodes.EmpleadoNotFound, "El empleado no existe.");
            }

            if (string.IsNullOrWhiteSpace(empleado.Email))
            {
                return Result<UserCreationResult>.Failure(ErrorCodes.ValidationAdminEmailRequired, "El empleado seleccionado no tiene un correo configurado.");
            }

            var loginEmail = email.Trim();
            if (!IsValidEmail(loginEmail))
            {
                return Result<UserCreationResult>.Failure(ErrorCodes.ValidationInvalidValue, "El correo del usuario no es válido.");
            }

            var empleadoEmail = empleado.Email.Trim();
            if (!IsValidEmail(empleadoEmail))
            {
                return Result<UserCreationResult>.Failure(ErrorCodes.ValidationInvalidValue, "El correo del empleado no es válido.");
            }

            // 0. Validar que el empleado no tenga ya un login
            var existingByEmployee = await _repository.GetByEmpleadoIdAsync(empleadoId);
            if (existingByEmployee != null)
            {
                return Result<UserCreationResult>.Failure(ErrorCodes.UsuarioEmpleadoDuplicado, "El empleado ya tiene un usuario asignado.");
            }

            // 0. Validar email duplicado
            var existing = await _repository.GetByEmailAsync(loginEmail);
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
            var nuevoUsuarioResult = UsuarioLogin.Crear(empleadoId, loginEmail, passwordHash, requiereCambioPassword: true);
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
            string mailBody = $"Hola,\nTu cuenta ha sido creada exitosamente.\nTu contraseña temporal es: {plainPassword}\nPor favor, cámbiala al iniciar sesión por primera vez.";
            var recipients = BuildRecipients(empleadoEmail, loginEmail);
            try
            {
                foreach (var recipient in recipients)
                {
                    await _mailSender.SendEmailAsync(recipient, "Credenciales de Acceso - Taller Mecánico", mailBody);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo enviar el correo de bienvenida. EmpleadoId={EmpleadoId} Destinatarios={Recipients}", empleadoId, string.Join(", ", recipients));
            }

            return Result<UserCreationResult>.Success(new UserCreationResult
            {
                User = nuevoUsuario,
                PlainPassword = plainPassword,
                NotificationRecipients = recipients
            });
        }

        private static bool IsValidEmail(string value)
        {
            try
            {
                _ = new MailAddress(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static IReadOnlyList<string> BuildRecipients(string empleadoEmail, string loginEmail)
        {
            var recipients = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                empleadoEmail
            };

            recipients.Add(loginEmail);
            return recipients.ToList();
        }
    }
}
