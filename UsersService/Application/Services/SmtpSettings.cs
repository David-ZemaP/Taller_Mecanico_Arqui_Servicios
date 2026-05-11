using System;
using Microsoft.Extensions.Configuration;

namespace Taller_Mecanico_Users.Application.Services
{
    /// <summary>
    /// Lectura de configuración SMTP desde Environment variables con fallback a appsettings.json.
    /// Las variables de entorno tienen prioridad sobre appsettings.json.
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
            // Leer de Environment variables primero, luego fallback a IConfiguration
            Enabled = GetBoolEnv("SMTP_ENABLED") 
                ?? bool.TryParse(configuration["Smtp:Enabled"], out var enabled) && enabled;
            
            Host = Environment.GetEnvironmentVariable("SMTP_HOST") 
                ?? configuration["Smtp:Host"] 
                ?? throw new InvalidOperationException("Smtp:Host no configurado.");
            
            Port = GetIntEnv("SMTP_PORT") 
                ?? (int.TryParse(configuration["Smtp:Port"], out var p) ? p : 25);
            
            Username = Environment.GetEnvironmentVariable("SMTP_USERNAME") 
                ?? configuration["Smtp:Username"];
            
            Password = Environment.GetEnvironmentVariable("SMTP_PASSWORD") 
                ?? configuration["Smtp:Password"];
            
            From = Environment.GetEnvironmentVariable("SMTP_FROM") 
                ?? configuration["Smtp:From"] 
                ?? Username 
                ?? "no-reply@example.com";
            
            EnableSsl = GetBoolEnv("SMTP_ENABLESSL") 
                ?? bool.TryParse(configuration["Smtp:EnableSsl"], out var s) && s;
            
            TimeoutMs = GetIntEnv("SMTP_TIMEOUTMS") 
                ?? (int.TryParse(configuration["Smtp:TimeoutMs"], out var t) ? t : 100000);
        }

        private static bool? GetBoolEnv(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            return string.IsNullOrEmpty(value) ? null : bool.TryParse(value, out var result) ? result : null;
        }

        private static int? GetIntEnv(string name)
        {
            var value = Environment.GetEnvironmentVariable(name);
            return string.IsNullOrEmpty(value) ? null : int.TryParse(value, out var result) ? result : null;
        }
    }
}