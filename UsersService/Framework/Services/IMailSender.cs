namespace Taller_Mecanico_Users.Framework.Services
{
    public interface IMailSender
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}