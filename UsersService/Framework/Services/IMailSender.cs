using Taller_Mecanico_Users.Domain.Common;

namespace Taller_Mecanico_Users.Framework.Services
{
    public interface IMailSender
    {
        Task<Result> SendUserCredentialsAsync(string toEmail, string username, string temporaryPassword);
        Task<Result> SendPasswordResetAsync(string toEmail, string temporaryPassword);
    }
}
