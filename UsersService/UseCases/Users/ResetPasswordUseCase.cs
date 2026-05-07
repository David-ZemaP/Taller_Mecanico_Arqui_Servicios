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
            var emailBody = $"Hola,\nTu contraseña fue restablecida exitosamente.\nTu contraseña temporal es: {temporaryPassword}\nPor favor, cámbiala al iniciar sesión por primera vez.";
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