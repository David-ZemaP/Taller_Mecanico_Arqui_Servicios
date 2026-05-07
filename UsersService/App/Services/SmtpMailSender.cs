using System.Net;
using System.Net.Mail;
using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Framework.Services;
using Taller_Mecanico_Users.Framework.Settings;

namespace Taller_Mecanico_Users.App.Services;

public class SmtpMailSender : IMailSender
{
    private readonly MailSettings _mailSettings;
    private readonly ILogger<SmtpMailSender> _logger;

    public SmtpMailSender(MailSettings mailSettings, ILogger<SmtpMailSender> logger)
    {
        _mailSettings = mailSettings;
        _logger = logger;
    }

    public async Task<Result> SendUserCredentialsAsync(string toEmail, string username, string temporaryPassword)
    {
        try
        {
            using var smtp = new SmtpClient(_mailSettings.Host, _mailSettings.Port);
            smtp.Credentials = new NetworkCredential(_mailSettings.Username, _mailSettings.Password);
            smtp.EnableSsl = _mailSettings.EnableSsl;

            var mail = new MailMessage
            {
                From = new MailAddress(_mailSettings.FromEmail, "PITSTOP Taller Mecánico"),
                Subject = "Credenciales de acceso - PITSTOP",
                Body = $"<p>Usuario: <b>{username}</b><br>Contraseña temporal: <b>{temporaryPassword}</b></p><p>Cambia tu contraseña en el primer acceso.</p>",
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            await smtp.SendMailAsync(mail);
            _logger.LogInformation("Email enviado a {Email}", toEmail);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando email a {Email}", toEmail);
            return Result.Failure("MAIL_ERROR", ex.Message);
        }
    }

    public async Task<Result> SendPasswordResetAsync(string toEmail, string temporaryPassword)
    {
        try
        {
            using var smtp = new SmtpClient(_mailSettings.Host, _mailSettings.Port);
            smtp.Credentials = new NetworkCredential(_mailSettings.Username, _mailSettings.Password);
            smtp.EnableSsl = _mailSettings.EnableSsl;

            var mail = new MailMessage
            {
                From = new MailAddress(_mailSettings.FromEmail, "PITSTOP Taller Mecánico"),
                Subject = "Reseteo de contraseña - PITSTOP",
                Body = $"<p>Tu nueva contraseña temporal es: <b>{temporaryPassword}</b></p><p>Cámbiala en tu próximo acceso.</p>",
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            await smtp.SendMailAsync(mail);
            _logger.LogInformation("Email de reset enviado a {Email}", toEmail);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando reset a {Email}", toEmail);
            return Result.Failure("MAIL_ERROR", ex.Message);
        }
    }
}
