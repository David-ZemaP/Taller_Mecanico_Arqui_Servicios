using System.Threading.Tasks;

namespace Taller_Mecanico_Users.Domain.Ports
{
    public interface IMailSender
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
