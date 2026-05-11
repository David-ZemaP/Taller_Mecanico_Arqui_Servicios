using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Ports;
using Microsoft.Extensions.Logging;

namespace Taller_Mecanico_Users.UseCases.Users
{
    public class ResetPasswordUseCase
    {
        private readonly IUsuarioLoginRepository _repository;
        private readonly Domain.Ports.IMailSender _mailSender;
        private readonly Domain.Ports.IPasswordSecurity _passwordSecurity;
        private readonly Domain.Ports.IPasswordHasher _passwordHasher;
        private readonly ILogger<ResetPasswordUseCase> _logger;

        public ResetPasswordUseCase(
            IUsuarioLoginRepository repository,
            Domain.Ports.IMailSender mailSender,
            Domain.Ports.IPasswordSecurity passwordSecurity,
            Domain.Ports.IPasswordHasher passwordHasher,
            ILogger<ResetPasswordUseCase> logger)
        {
            _repository = repository;
            _mailSender = mailSender;
            _passwordSecurity = passwordSecurity;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<Result<string>> ExecuteAsync(int usuarioLoginId)
        {
            // 1. Obtener usuario
            var userResult = await _repository.GetByIdAsync(usuarioLoginId);
            if (userResult.IsFailure)
            {
                return Result<string>.Failure(userResult.ErrorCode ?? ErrorCodes.DbError, userResult.ErrorMessage ?? "Error al obtener usuario.");
            }

            var user = userResult.Value;
            if (user == null)
            {
                return Result<string>.Failure(ErrorCodes.UsuarioLoginNotFound, "Usuario no encontrado.");
            }

            // 2. Generar nueva contraseña temporal
            var temporaryPassword = _passwordSecurity.GenerateSecurePassword();

            // 3. Hashear la nueva contraseña
            var passwordHash = _passwordHasher.HashPassword(temporaryPassword);

            // 4. Aplicar reset en la entidad (activa RequiereCambioPassword = true)
            var resetResult = user.ResetearPassword(passwordHash);
            if (resetResult.IsFailure)
            {
                return Result<string>.Failure(resetResult.ErrorCode ?? ErrorCodes.DbError, resetResult.ErrorMessage ?? "Error al resetear contraseña.");
            }

            // 5. Persistir cambios
            var updateResult = await _repository.UpdateAsync(user);
            if (updateResult.IsFailure)
            {
                return Result<string>.Failure(updateResult.ErrorCode ?? ErrorCodes.DbError, updateResult.ErrorMessage ?? "Error al persistir cambios.");
            }

            // 6. Enviar nueva contraseña por correo (fallo de correo no cancela el reset)
            var emailBody = $@"
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
            <h2 style='color: #2c3e50; margin-top: 0;'>Contraseña Restablecida</h2>
            <p style='color: #555; line-height: 1.6;'>Su contraseña ha sido restablecida exitosamente. A continuación encontrará sus nuevas credenciales de acceso:</p>
            <div style='background-color: #f8f9fa; border-left: 4px solid #e74c3c; padding: 15px 20px; margin: 20px 0;'>
                <p style='margin: 5px 0;'><strong style='color: #2c3e50;'>Correo electrónico:</strong></p>
                <p style='margin: 0; font-size: 16px; color: #2980b9;'>{user.Email}</p>
                <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 15px 0;'>
                <p style='margin: 5px 0;'><strong style='color: #2c3e50;'>Nueva contraseña temporal:</strong></p>
                <p style='margin: 0; font-size: 18px; font-weight: bold; color: #e74c3c; letter-spacing: 2px;'>{temporaryPassword}</p>
            </div>
            <div style='background-color: #fff3cd; border: 1px solid #ffc107; border-radius: 6px; padding: 15px; margin: 20px 0;'>
                <p style='margin: 0; color: #856404; font-size: 14px;'>
                    <strong>⚠️ Importante:</strong> Por su seguridad, debe cambiar su contraseña al iniciar sesión por primera vez.
                </p>
            </div>
            <p style='color: #7f8c8d; font-size: 12px; text-align: center; margin-top: 30px;'>
                Si no solicitó este cambio, comuníquese con el administrador del sistema.
            </p>
        </div>
    </div>
</body>
</html>";
            try
            {
                await _mailSender.SendEmailAsync(user.Email, "Restablecimiento de contraseña - Taller Mecánico", emailBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo enviar el correo de restablecimiento a {Email}", user.Email);
            }

            return Result<string>.Success(temporaryPassword);
        }
    }
}