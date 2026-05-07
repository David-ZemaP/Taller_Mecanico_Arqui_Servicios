using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Ports;

namespace Taller_Mecanico_Users.UseCases.Users
{
    public class ChangePasswordUseCase
    {
        private readonly IUsuarioLoginRepository _loginRepository;

        public ChangePasswordUseCase(IUsuarioLoginRepository loginRepository)
        {
            _loginRepository = loginRepository;
        }

        public async Task<Result> ExecuteAsync(int usuarioLoginId, string passwordActual, string nuevoPassword)
        {
            var userResult = await _loginRepository.GetByIdAsync(usuarioLoginId);
            if (userResult.IsFailure)
                return Result.Failure(userResult.ErrorCode ?? ErrorCodes.DbError, userResult.ErrorMessage ?? "Error al obtener usuario.");

            var user = userResult.Value;
            if (user == null)
                return Result.Failure(ErrorCodes.UsuarioLoginNotFound, "Usuario no encontrado.");

            if (!user.RequiereCambioPassword)
            {
                if (!BCrypt.Net.BCrypt.Verify(passwordActual, user.PasswordHash))
                    return Result.Failure(ErrorCodes.ValidationInvalidValue, "La contraseña actual es incorrecta.");
            }

            var validationResult = PasswordSecurity.ValidatePassword(nuevoPassword);
            if (validationResult.IsFailure)
                return validationResult;

            user.CambiarPassword(BCrypt.Net.BCrypt.HashPassword(nuevoPassword));
            return await _loginRepository.UpdateAsync(user);
        }
    }
}
