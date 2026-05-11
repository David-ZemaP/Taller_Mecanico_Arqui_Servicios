namespace Taller_Mecanico_Users.Application.Services
{
    public interface IMailSender
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}