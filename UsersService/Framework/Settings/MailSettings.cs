namespace Taller_Mecanico_Users.Framework.Settings;

/// <summary>
/// Configuración de correo SMTP.
/// Lee desde appsettings.json sección "MailSettings"
/// </summary>
public class MailSettings
{
    public bool UseSmtp { get; set; } = false;
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
}
