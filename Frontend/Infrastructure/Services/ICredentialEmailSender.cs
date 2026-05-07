namespace Taller_Mecanico_Arqui.Infrastructure.Services;

public interface ICredentialEmailSender
{
    Task SendCredentialsAsync(string email, string username, string password);
}
