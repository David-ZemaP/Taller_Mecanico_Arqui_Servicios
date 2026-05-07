using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Ports;

namespace Taller_Mecanico_Users.UseCases.Users
{
    /// <summary>
    /// UseCase: Reset administrativo de contraseña.
    /// 
    /// Arquitectura:
    /// - PasswordSecurity: Generar contraseña temporal ✅ (IPasswordSecurity)
    /// - PasswordHasher: Encriptar contraseña ✅ (IPasswordHasher)
    /// - Repository: Persistencia ✅ (IUsuarioLoginRepository)
    /// - MailSender: Enviar credenciales ✅ (IMailSender)
    /// 
    /// ANTES: Usaba PasswordSecurity.GenerateSecurePassword() y BCrypt.Net.BCrypt.HashPassword() directamente
    /// AHORA: Inyecta servicios, mejora testabilidad y flexibilidad
    /// </summary>
    public class ResetPasswordUseCase
    {
        private readonly IUsuarioLoginRepository _repository;
        private readonly Domain.Ports.IMailSender _mailSender;
        private readonly Domain.Ports.IPasswordSecurity _passwordSecurity;
        private readonly Domain.Ports.IPasswordHasher _passwordHasher;

        public ResetPasswordUseCase(
            IUsuarioLoginRepository repository,
            Domain.Ports.IMailSender mailSender,
            Domain.Ports.IPasswordSecurity passwordSecurity,
            Domain.Ports.IPasswordHasher passwordHasher)
        {
            _repository = repository;
            _mailSender = mailSender;
            _passwordSecurity = passwordSecurity;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result> ExecuteAsync(int usuarioLoginId)
        {
            // 1. Obtener usuario
            var userResult = await _repository.GetByIdAsync(usuarioLoginId);
            if (userResult.IsFailure)
            {
                return Result.Failure(userResult.ErrorCode ?? ErrorCodes.DbError, userResult.ErrorMessage ?? "Error al obtener usuario.");
            }

            var user = userResult.Value;
            if (user == null)
            {
                return Result.Failure(ErrorCodes.UsuarioLoginNotFound, "Usuario no encontrado.");
            }

            // 2. Generar nueva contraseña temporal
            var temporaryPassword = _passwordSecurity.GenerateSecurePassword();
            
            // 3. Hashear la nueva contraseña
            var passwordHash = _passwordHasher.HashPassword(temporaryPassword);
            
            // 4. Aplicar reset en la entidad (activa RequiereCambioPassword = true)
            var resetResult = user.ResetearPassword(passwordHash);
            if (resetResult.IsFailure)
            {
                return resetResult;
            }

            // 5. Persistir cambios
            var updateResult = await _repository.UpdateAsync(user);
            if (updateResult.IsFailure)
            {
                return updateResult;
            }

            // 6. Enviar nueva contraseña por correo
            var emailBody = $"Hola,\nTu contraseña fue restablecida exitosamente.\nTu contraseña temporal es: {temporaryPassword}\nPor favor, cámbiala al iniciar sesión por primera vez.";
            await _mailSender.SendEmailAsync(user.Email, "Restablecimiento de contraseña - Taller Mecánico", emailBody);

            return Result.Success();
        }
    }
}