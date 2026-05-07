using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Framework.Services;

namespace Taller_Mecanico_Users.App.Services;

public class DummyMailSender : IMailSender
{
    private readonly ILogger<DummyMailSender> _logger;

    public DummyMailSender(ILogger<DummyMailSender> logger)
    {
        _logger = logger;
    }

    public async Task<Result> SendUserCredentialsAsync(string toEmail, string username, string temporaryPassword)
    {
        _logger.LogInformation("═══════════════════════════════════════════════════════════════════");
        _logger.LogInformation("[DUMMY MAIL] Credenciales enviadas a: {Email}", toEmail);
        _logger.LogInformation("Usuario: {Username} | Contraseña temporal: {Password}", username, temporaryPassword);
        _logger.LogInformation("HTML:");
        _logger.LogInformation(GenerateCredentialsHtml(toEmail, username, temporaryPassword));
        _logger.LogInformation("═══════════════════════════════════════════════════════════════════");
        await Task.Delay(50);
        return Result.Success();
    }

    public async Task<Result> SendPasswordResetAsync(string toEmail, string temporaryPassword)
    {
        _logger.LogWarning("═══════════════════════════════════════════════════════════════════");
        _logger.LogWarning("[DUMMY MAIL - RESET] Enviado a: {Email}", toEmail);
        _logger.LogWarning("Nueva contraseña temporal: {Password}", temporaryPassword);
        _logger.LogWarning("═══════════════════════════════════════════════════════════════════");
        await Task.Delay(50);
        return Result.Success();
    }

    private static string GenerateCredentialsHtml(string toEmail, string username, string password)
    {
        return $@"
<!DOCTYPE html><html lang='es'><head><meta charset='UTF-8'>
<style>
  body{{font-family:Arial,sans-serif;background:#f4f4f4;}}
  .c{{max-width:600px;margin:20px auto;background:#fff;border-radius:8px;}}
  .h{{background:linear-gradient(135deg,#1a1a2e,#20c997);color:#fff;padding:25px;text-align:center;}}
  .b{{padding:25px;}}
  .box{{background:#f9f9f9;border-left:4px solid #20c997;padding:15px;margin:15px 0;}}
  .w{{background:#fff3cd;border:1px solid #ffc107;padding:10px;border-radius:4px;font-size:13px;}}
</style></head><body>
<div class='c'>
  <div class='h'><h1>PITSTOP Taller Mecánico</h1><p>Credenciales de Acceso</p></div>
  <div class='b'>
    <p>Destinatario: <strong>{toEmail}</strong></p>
    <p>Fecha: <strong>{DateTime.Now:dd/MM/yyyy HH:mm:ss}</strong></p>
    <div class='box'>
      <table width='100%'>
        <tr><td><b>Usuario:</b></td><td><code>{username}</code></td></tr>
        <tr><td><b>Contraseña:</b></td><td><code>{password}</code></td></tr>
      </table>
    </div>
    <div class='w'>⚠️ Cambia tu contraseña en el primer acceso. Requiere: ≥8 caracteres, mayúscula, minúscula, dígito y carácter especial.</div>
  </div>
</div></body></html>";
    }
}
