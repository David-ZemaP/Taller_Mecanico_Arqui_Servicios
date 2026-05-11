using System;
using Microsoft.Extensions.Configuration;

namespace Taller_Mecanico_Users.Application.Services
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
            Enabled = bool.TryParse(section["Enabled"], out var enabled) && enabled;
            Host = section["Host"] ?? throw new InvalidOperationException("Smtp:Host no configurado.");
            if (!int.TryParse(section["Port"], out var port)) port = 25;
            Port = port;
            Username = section["Username"];
            Password = section["Password"];
            From = section["From"] ?? Username ?? "no-reply@example.com";
            EnableSsl = bool.TryParse(section["EnableSsl"], out var ssl) && ssl;
            if (!int.TryParse(section["TimeoutMs"], out var timeout)) timeout = 100000;
            TimeoutMs = timeout;
        }
    }
}

