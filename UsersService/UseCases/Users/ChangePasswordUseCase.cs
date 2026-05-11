using Taller_Mecanico_Users.Domain.Ports;
using Taller_Mecanico_Users.Domain.Common;

namespace Taller_Mecanico_Users.UseCases.Users
{
    /// <summary>
    /// UseCase: Cambio de contraseña por el usuario.
    /// 
    /// Arquitectura:
    /// - PasswordSecurity: Validar nueva contraseña ✅ (IPasswordSecurity)
    /// - PasswordHasher: Encriptar contraseña ✅ (IPasswordHasher)
    /// - Repository: Persistencia ✅ (IUsuarioLoginRepository)
    /// 
    /// ANTES: Usaba PasswordSecurity.ValidatePassword() y BCrypt.Net.BCrypt.HashPassword() directamente
    /// AHORA: Inyecta servicios, permite testing y cambio de implementación
    /// </summary>
    public class ChangePasswordUseCase
    {
        private readonly IUsuarioLoginRepository _loginRepository;
        private readonly Domain.Ports.IPasswordSecurity _passwordSecurity;
        private readonly Domain.Ports.IPasswordHasher _passwordHasher;

        public ChangePasswordUseCase(
            IUsuarioLoginRepository loginRepository,
            Domain.Ports.IPasswordSecurity passwordSecurity,
            Domain.Ports.IPasswordHasher passwordHasher)
        {
            _loginRepository = loginRepository;
            _passwordSecurity = passwordSecurity;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result> ExecuteAsync(int usuarioLoginId, string currentPassword, string nuevoPassword, string confirmarPassword)
        {
            // 1. Validar que la contraseña actual es obligatoria
            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                return Result.Failure(ErrorCodes.ValidationRequired, "La contraseña actual es obligatoria.");
            }

            // 2. Obtener usuario del repositorio
            var userResult = await _loginRepository.GetByIdAsync(usuarioLoginId);
            if (userResult.IsFailure)
                return Result.Failure(userResult.ErrorCode ?? ErrorCodes.DbError, userResult.ErrorMessage ?? "Error al obtener usuario.");

            var user = userResult.Value;
            if (user == null)
                return Result.Failure(ErrorCodes.UsuarioLoginNotFound, "Usuario no encontrado.");

            // 3. Verificar contraseña actual PRIMERO (antes de cualquier otra validación)
            if (!_passwordHasher.VerifyPassword(currentPassword, user.PasswordHash))
            {
                return Result.Failure(ErrorCodes.ValidationInvalidValue, "La contraseña actual es incorrecta.");
            }

            // 4. Validar que la nueva contraseña cumple requisitos
            var validationResult = _passwordSecurity.ValidatePassword(nuevoPassword);
            if (validationResult.IsFailure)
            {
                return validationResult;
            }

            // 5. Verificar que la nueva contraseña coincide con la confirmación
            if (nuevoPassword != confirmarPassword)
            {
                return Result.Failure(ErrorCodes.ValidationInvalidValue, "Las contraseñas no coinciden.");
            }

            // 6. Hashear nueva contraseña y persistir
            var passwordHash = _passwordHasher.HashPassword(nuevoPassword);
            var changeResult = user.CambiarPassword(passwordHash);
            if (changeResult.IsFailure)
            {
                return changeResult;
            }
            
            return await _loginRepository.UpdateAsync(user);
        }
    }
}
