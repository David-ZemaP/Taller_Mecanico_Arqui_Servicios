using Taller_Mecanico_Users.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Taller_Mecanico_Users.Domain.Entities;
using Taller_Mecanico_Users.Domain.Ports;
using Taller_Mecanico_Users.Application.Services;
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
        private readonly IAuthenticationHelper _authHelper;

        public CreateUserUseCase(
            IUsuarioLoginRepository repository,
            IEmpleadoRepository empleadoRepository,
            Domain.Ports.IMailSender mailSender,
            Domain.Ports.IPasswordSecurity passwordSecurity,
            Domain.Ports.IPasswordHasher passwordHasher,
            ILogger<CreateUserUseCase> logger,
            IAuthenticationHelper authHelper)
        {
            _repository = repository;
            _empleadoRepository = empleadoRepository;
            _mailSender = mailSender;
            _passwordSecurity = passwordSecurity;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _authHelper = authHelper;
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

            // 4. Registrar auditoría de creación
            var actor = _authHelper.GetCurrentAuditActor();
            nuevoUsuario.RegistrarCreacion(actor);

            // 5. Persistir en el repositorio
            var addResult = await _repository.AddAsync(nuevoUsuario);
            if (addResult.IsFailure)
            {
                return Result<UserCreationResult>.Failure(addResult.ErrorCode ?? ErrorCodes.DbError, addResult.ErrorMessage ?? "Error al crear usuario.");
            }

            // 6. Enviar credenciales por correo (fallo de correo no cancela la creación)
            string mailBody = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>
<body style='margin: 0; padding: 0; font-family: Arial, Helvetica, sans-serif; background-color: #f4f4f4;'>
    <div style='max-width: 600px; margin: 40px auto; background-color: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
        <div style='background: linear-gradient(135deg, #2c3e50, #34495e); padding: 30px; text-align: center;'>
            <h1 style='color: #ffffff; margin: 0; font-size: 24px;'>Taller Mecánico</h1>
            <p style='color: #bdc3c7; margin: 10px 0 0; font-size: 14px;'>Sistema de Gestión</p>
        </div>
        <div style='padding: 30px;'>
            <h2 style='color: #2c3e50; margin-top: 0;'>Credenciales de Acceso</h2>
            <p style='color: #555; line-height: 1.6;'>Su cuenta ha sido creada exitosamente. A continuación encontrará sus datos de acceso:</p>
            <div style='background-color: #f8f9fa; border-left: 4px solid #27ae60; padding: 15px 20px; margin: 20px 0;'>
                <p style='margin: 5px 0;'><strong style='color: #2c3e50;'>Correo electrónico:</strong></p>
                <p style='margin: 0; font-size: 16px; color: #2980b9;'>{loginEmail}</p>
                <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 15px 0;'>
                <p style='margin: 5px 0;'><strong style='color: #2c3e50;'>Contraseña temporal:</strong></p>
                <p style='margin: 0; font-size: 18px; font-weight: bold; color: #27ae60; letter-spacing: 2px;'>{plainPassword}</p>
            </div>
            <div style='background-color: #fff3cd; border: 1px solid #ffc107; border-radius: 6px; padding: 15px; margin: 20px 0;'>
                <p style='margin: 0; color: #856404; font-size: 14px;'>
                    <strong>⚠️ Importante:</strong> Por su seguridad, debe cambiar su contraseña al iniciar sesión por primera vez.
                </p>
            </div>
            <p style='color: #7f8c8d; font-size: 12px; text-align: center; margin-top: 30px;'>
                Este es un mensaje automático. No responda directamente a este correo.
            </p>
        </div>
    </div>
</body>
</html>";
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
