using System.Net;
using System.Net.Mail;
using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Framework.Settings;

namespace Taller_Mecanico_Users.App.Services;

/// <summary>
/// Implementación real de IMailSender usando SMTP.
/// Lee configuración de appsettings.json sección "MailSettings"
/// </summary>
public class SmtpMailSender : IMailSender
{
    private readonly MailSettings _mailSettings;
    private readonly ILogger<SmtpMailSender> _logger;

    public SmtpMailSender(MailSettings mailSettings, ILogger<SmtpMailSender> logger)
    {
        _mailSettings = mailSettings ?? throw new ArgumentNullException(nameof(mailSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> SendUserCredentialsAsync(string toEmail, string username, string temporaryPassword)
    {
        try
        {
            var htmlBody = GenerateCredentialsEmailHtml(username, temporaryPassword);

            using (var smtp = new SmtpClient(_mailSettings.Host, _mailSettings.Port))
            {
                smtp.Credentials = new NetworkCredential(_mailSettings.Username, _mailSettings.Password);
                smtp.EnableSsl = _mailSettings.EnableSsl;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_mailSettings.FromEmail, "PITSTOP - Taller Mecánico"),
                    Subject = "🔑 Bienvenido a PITSTOP - Tus Credenciales",
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await smtp.SendMailAsync(mailMessage);

                _logger.LogInformation(
                    $"✅ Email enviado exitosamente a {toEmail} | Usuario: {username} | Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                return Result.Success();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"❌ Error enviando email a {toEmail} | Exception: {ex.Message}");
            return Result.Failure("MAIL_ERROR", $"No fue posible enviar email: {ex.Message}");
        }
    }

    public async Task<Result> SendPasswordResetAsync(string toEmail, string temporaryPassword)
    {
        try
        {
            var htmlBody = GeneratePasswordResetEmailHtml(temporaryPassword);

            using (var smtp = new SmtpClient(_mailSettings.Host, _mailSettings.Port))
            {
                smtp.Credentials = new NetworkCredential(_mailSettings.Username, _mailSettings.Password);
                smtp.EnableSsl = _mailSettings.EnableSsl;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_mailSettings.FromEmail, "PITSTOP - Taller Mecánico"),
                    Subject = "🔄 Reseteo de Contraseña",
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);
                await smtp.SendMailAsync(mailMessage);

                _logger.LogInformation($"✅ Email de reseteo enviado a {toEmail}");
                return Result.Success();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Error enviando reseteo a {toEmail}");
            return Result.Failure("MAIL_ERROR", ex.Message);
        }
    }

    private static string GenerateCredentialsEmailHtml(string username, string temporaryPassword)
    {
        return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: 'Helvetica Neue', Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 20px auto; background-color: white; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #c41e3a 0%, #8b0000 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .header p {{ margin: 10px 0 0 0; font-size: 14px; opacity: 0.9; }}
        .content {{ padding: 30px; }}
        .content h2 {{ color: #333; margin-top: 0; }}
        .credentials {{ background-color: #f9f9f9; border-left: 4px solid #c41e3a; padding: 15px; margin: 20px 0; border-radius: 4px; }}
        .credentials table {{ width: 100%; border-collapse: collapse; }}
        .credentials td {{ padding: 12px; border-bottom: 1px solid #eee; }}
        .credentials .label {{ font-weight: bold; color: #555; width: 40%; }}
        .credentials .value {{ font-family: 'Courier New', monospace; color: #c41e3a; font-weight: bold; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffc107; color: #856404; padding: 12px; border-radius: 4px; margin: 20px 0; font-size: 13px; }}
        .footer {{ background-color: #f4f4f4; border-top: 1px solid #eee; padding: 15px; text-align: center; font-size: 12px; color: #666; border-radius: 0 0 8px 8px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔧 PITSTOP</h1>
            <p>Taller Mecánico</p>
        </div>
        <div class='content'>
            <h2>¡Bienvenido al Sistema!</h2>
            <p>Has sido registrado exitosamente en PITSTOP. A continuación encontrarás tus credenciales de acceso:</p>
            
            <div class='credentials'>
                <table>
                    <tr>
                        <td class='label'>Usuario:</td>
                        <td class='value'>{username}</td>
                    </tr>
                    <tr>
                        <td class='label'>Contraseña Temporal:</td>
                        <td class='value'>{temporaryPassword}</td>
                    </tr>
                    <tr>
                        <td class='label'>Fecha Creación:</td>
                        <td>{DateTime.Now:dd/MM/yyyy HH:mm:ss}</td>
                    </tr>
                </table>
            </div>
            
            <div class='warning'>
                <strong>⚠️ IMPORTANTE:</strong> Debes cambiar tu contraseña temporal en el primer acceso. La contraseña debe cumplir con los siguientes requisitos:
                <ul style='margin: 8px 0; padding-left: 20px;'>
                    <li>Mínimo 8 caracteres</li>
                    <li>Al menos una mayúscula (A-Z)</li>
                    <li>Al menos una minúscula (a-z)</li>
                    <li>Al menos un dígito (0-9)</li>
                    <li>Al menos un carácter especial (!@#$%^&*)</li>
                </ul>
            </div>
            
            <p>Para acceder al sistema, utiliza estas credenciales en <strong>https://tusistema.com/login</strong></p>
            <p>¿Preguntas? Contacta al equipo de soporte.</p>
        </div>
        <div class='footer'>
            <p>© 2026 PITSTOP - Taller Mecánico. Todos los derechos reservados.</p>
            <p>Este es un correo automático generado por el sistema. No responda a este email.</p>
        </div>
    </div>
</body>
</html>";
    }

    private static string GeneratePasswordResetEmailHtml(string temporaryPassword)
    {
        return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: 'Helvetica Neue', Arial, sans-serif; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 20px auto; background-color: white; border-radius: 8px; }}
        .header {{ background: linear-gradient(135deg, #ffc107 0%, #ff9800 100%); color: white; padding: 30px; text-align: center; }}
        .content {{ padding: 30px; }}
        .credentials {{ background-color: #f9f9f9; border-left: 4px solid #ff9800; padding: 15px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔄 Reseteo de Contraseña</h1>
        </div>
        <div class='content'>
            <h2>Tu contraseña ha sido reiniciada</h2>
            <p>Se ha generado una nueva contraseña temporal para tu cuenta:</p>
            
            <div class='credentials'>
                <strong>Contraseña Temporal:</strong><br>
                <code>{temporaryPassword}</code>
            </div>
            
            <p>Por favor, cambia esta contraseña en tu próximo acceso.</p>
        </div>
    </div>
</body>
</html>";
    }
}
