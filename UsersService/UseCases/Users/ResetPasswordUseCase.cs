using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Ports;
using Taller_Mecanico_Users.Framework.Services;

namespace Taller_Mecanico_Users.UseCases.Users
{
    public class ResetPasswordUseCase
    {
        private readonly IUsuarioLoginRepository _repository;
        private readonly IMailSender _mailSender;

        public ResetPasswordUseCase(IUsuarioLoginRepository repository, IMailSender mailSender)
        {
            _repository = repository;
            _mailSender = mailSender;
        }

        public async Task<Result> ExecuteAsync(int usuarioLoginId)
        {
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

            var temporaryPassword = PasswordSecurity.GenerateSecurePassword();
            user.ResetearPassword(BCrypt.Net.BCrypt.HashPassword(temporaryPassword));

            var updateResult = await _repository.UpdateAsync(user);
            if (updateResult.IsFailure)
            {
                return updateResult;
            }

            await _mailSender.SendPasswordResetAsync(user.Email, temporaryPassword);

            return Result.Success();
        }
    }
}