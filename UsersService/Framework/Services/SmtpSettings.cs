using System;
using Microsoft.Extensions.Configuration;

namespace Taller_Mecanico_Users.Framework.Services
{
    /// <summary>
    /// Lectura de configuración SMTP desde la sección "Smtp" en appsettings.
    /// </summary>
    public class SmtpSettings
    {
        public bool Enabled { get; }
        public string Host { get; }
        public int Port { get; }
        public string? Username { get; }
        public string? Password { get; }
        public string From { get; }
        public bool EnableSsl { get; }
        public int TimeoutMs { get; }

        public SmtpSettings(IConfiguration configuration)
        {
            var section = configuration.GetSection("Smtp");

            static string? Env(string key) => Environment.GetEnvironmentVariable(key);

            var enabledRaw = section["Enabled"]
                ?? Env("Smtp__Enabled")
                ?? Env("SMTP_ENABLED");
            Enabled = bool.TryParse(enabledRaw, out var enabled) && enabled;

            Host = section["Host"]
                ?? Env("Smtp__Host")
                ?? Env("SMTP_HOST")
                ?? "smtp.gmail.com";

            var portRaw = section["Port"]
                ?? Env("Smtp__Port")
                ?? Env("SMTP_PORT");
            if (!int.TryParse(portRaw, out var port)) port = 587;
            Port = port;

            Username = section["Username"]
                ?? Env("Smtp__Username")
                ?? Env("SMTP_USERNAME")
                ?? Env("SmtpSettings__Username");

            Password = section["Password"]
                ?? Env("Smtp__Password")
                ?? Env("SMTP_PASSWORD")
                ?? Env("SmtpSettings__Password");

            From = section["From"]
                ?? Env("Smtp__From")
                ?? Env("SMTP_FROM")
                ?? Env("SmtpSettings__SenderEmail")
                ?? Username
                ?? "no-reply@example.com";

            var sslRaw = section["EnableSsl"]
                ?? Env("Smtp__EnableSsl")
                ?? Env("SMTP_ENABLE_SSL")
                ?? "true";
            EnableSsl = bool.TryParse(sslRaw, out var ssl) && ssl;

            var timeoutRaw = section["TimeoutMs"]
                ?? Env("Smtp__TimeoutMs")
                ?? Env("SMTP_TIMEOUT_MS");
            if (!int.TryParse(timeoutRaw, out var timeout)) timeout = 100000;
            TimeoutMs = timeout;
        }
    }
}
