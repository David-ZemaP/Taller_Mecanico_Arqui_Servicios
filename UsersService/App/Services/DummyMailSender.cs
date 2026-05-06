using Taller_Mecanico_Users.Domain.Common;

namespace Taller_Mecanico_Users.App.Services;

/// <summary>
/// Implementación de demostración de IMailSender.
/// Escribe en consola en lugar de enviar emails reales.
/// Útil para desarrollo y demostración (defensa).
/// Enriquecido con HTML formal e ILogger para auditoría.
/// </summary>
public class DummyMailSender : IMailSender
{
    private readonly ILogger<DummyMailSender> _logger;

    public DummyMailSender(ILogger<DummyMailSender> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> SendUserCredentialsAsync(string toEmail, string username, string temporaryPassword)
    {
        var htmlBody = GenerateCredentialsEmailHtml(username, temporaryPassword, toEmail);
        
        _logger.LogInformation(
            "═══════════════════════════════════════════════════════════════════════════════");
        _logger.LogInformation(
            $"📧 [DUMMY MAIL SENDER] Email enviado a: {toEmail}");
        _logger.LogInformation(
            $"👤 Usuario: {username}");
        _logger.LogInformation(
            $"🔑 Contraseña Temporal: {temporaryPassword}");
        _logger.LogInformation(
            $"⏰ Timestamp: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        _logger.LogInformation(
            "───────────────────────────────────────────────────────────────────────────────");
        _logger.LogInformation("📄 HTML Content:");
        _logger.LogInformation(htmlBody);
        _logger.LogInformation(
            "═══════════════════════════════════════════════════════════════════════════════");

        await Task.Delay(100);  // Simular pequeño delay de "envío"
        return Result.Success();
    }

    public async Task<Result> SendPasswordResetAsync(string toEmail, string temporaryPassword)
    {
        var htmlBody = GeneratePasswordResetEmailHtml(temporaryPassword, toEmail);
        
        _logger.LogWarning(
            "═══════════════════════════════════════════════════════════════════════════════");
        _logger.LogWarning(
            $"📧 [DUMMY MAIL SENDER - PASSWORD RESET] Email enviado a: {toEmail}");
        _logger.LogWarning(
            $"🔄 Nueva Contraseña Temporal: {temporaryPassword}");
        _logger.LogWarning(
            $"⏰ Timestamp: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        _logger.LogWarning(
            "───────────────────────────────────────────────────────────────────────────────");
        _logger.LogWarning("📄 HTML Content:");
        _logger.LogWarning(htmlBody);
        _logger.LogWarning(
            "═══════════════════════════════════════════════════════════════════════════════");

        await Task.Delay(100);
        return Result.Success();
    }

    private static string GenerateCredentialsEmailHtml(string username, string temporaryPassword, string toEmail)
    {
        return $@"
╔═══════════════════════════════════════════════════════════════════════════════╗
║ 🔧 PITSTOP - TALLER MECÁNICO - CREDENCIALES DE ACCESO                       ║
╚═══════════════════════════════════════════════════════════════════════════════╝

Destinatario: {toEmail}
Fecha Generación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}
───────────────────────────────────────────────────────────────────────────────

Asunto: 🔑 Bienvenido a PITSTOP - Tus Credenciales

Cuerpo HTML:
───────────────────────────────────────────────────────────────────────────────

<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: 'Helvetica Neue', Arial, sans-serif; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 20px auto; background-color: white; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #c41e3a 0%, #8b0000 100%); color: white; padding: 30px; text-align: center; }}
        .content {{ padding: 30px; }}
        .credentials {{ background-color: #f9f9f9; border-left: 4px solid #c41e3a; padding: 15px; margin: 20px 0; }}
        .credentials table {{ width: 100%; border-collapse: collapse; }}
        .credentials td {{ padding: 10px; border-bottom: 1px solid #eee; }}
        .credentials .label {{ font-weight: bold; color: #555; width: 40%; }}
        .credentials .value {{ font-family: 'Courier New', monospace; color: #c41e3a; font-weight: bold; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffc107; color: #856404; padding: 12px; border-radius: 4px; margin: 20px 0; font-size: 13px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔧 PITSTOP</h1>
            <p>Taller Mecánico - Gestión de Órdenes</p>
        </div>
        <div class='content'>
            <h2>¡Bienvenido!</h2>
            <p>Has sido registrado exitosamente en PITSTOP. Tus credenciales iniciales son:</p>
            
            <div class='credentials'>
                <table>
                    <tr>
                        <td class='label'>👤 Usuario:</td>
                        <td class='value'>{username}</td>
                    </tr>
                    <tr>
                        <td class='label'>🔑 Contraseña:</td>
                        <td class='value'>{temporaryPassword}</td>
                    </tr>
                    <tr>
                        <td class='label'>📧 Email:</td>
                        <td>{toEmail}</td>
                    </tr>
                    <tr>
                        <td class='label'>📅 Fecha:</td>
                        <td>{DateTime.Now:dd/MM/yyyy HH:mm:ss}</td>
                    </tr>
                </table>
            </div>
            
            <div class='warning'>
                <strong>⚠️ IMPORTANTE:</strong> Cambia tu contraseña en el primer acceso.
                Requiere: ≥8 caracteres, mayúscula, minúscula, dígito, carácter especial
            </div>
        </div>
    </div>
</body>
</html>

───────────────────────────────────────────────────────────────────────────────
FIN DEL EMAIL
═══════════════════════════════════════════════════════════════════════════════";
    }

    private static string GeneratePasswordResetEmailHtml(string temporaryPassword, string toEmail)
    {
        return $@"
╔═══════════════════════════════════════════════════════════════════════════════╗
║ 🔄 PITSTOP - RESETEO DE CONTRASEÑA                                          ║
╚═══════════════════════════════════════════════════════════════════════════════╝

Destinatario: {toEmail}
Fecha Generación: {DateTime.Now:dd/MM/yyyy HH:mm:ss}
───────────────────────────────────────────────────────────────────────────────

Asunto: 🔄 Tu Contraseña ha sido Reiniciada

Cuerpo HTML:
───────────────────────────────────────────────────────────────────────────────

<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{ font-family: 'Helvetica Neue', Arial, sans-serif; background-color: #f4f4f4; }}
        .container {{ max-width: 600px; margin: 20px auto; background-color: white; border-radius: 8px; }}
        .header {{ background: linear-gradient(135deg, #ffc107 0%, #ff9800 100%); color: white; padding: 30px; text-align: center; }}
        .content {{ padding: 30px; }}
        .alert {{ background-color: #fff3cd; border: 1px solid #ffc107; padding: 12px; border-radius: 4px; color: #856404; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔄 Reseteo de Contraseña</h1>
        </div>
        <div class='content'>
            <h2>Tu Contraseña ha sido Reiniciada</h2>
            <div class='alert'>
                <strong>Tu nueva contraseña temporal es:</strong><br>
                <code style='font-size: 16px; color: #ff9800;'>{temporaryPassword}</code>
            </div>
            <p>Por favor, cambia esta contraseña en tu próximo acceso.</p>
        </div>
    </div>
</body>
</html>

───────────────────────────────────────────────────────────────────────────────
FIN DEL EMAIL
═══════════════════════════════════════════════════════════════════════════════";
    }
}
