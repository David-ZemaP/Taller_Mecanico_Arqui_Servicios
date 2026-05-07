using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            var baseDir = Directory.GetCurrentDirectory();
            // appsettings is located in ../../App/appsettings.json relative to Tools/SmtpTest
            var appSettingsPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "App", "appsettings.json"));
            Console.WriteLine($"Loading configuration from: {appSettingsPath}");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(appSettingsPath, optional: false, reloadOnChange: false)
                .Build();

            var smtpSettings = new Taller_Mecanico_Users.Framework.Services.SmtpSettings(configuration);

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = loggerFactory.CreateLogger<Taller_Mecanico_Users.App.Services.SmtpMailSender>();

            var mailer = new Taller_Mecanico_Users.App.Services.SmtpMailSender(smtpSettings, logger);

            var to = configuration["Smtp:TestRecipient"] ?? configuration["Smtp:Username"] ?? throw new InvalidOperationException("No recipient configured");
            var subject = "TallerMecanico - Prueba SMTP";
            var body = "<p>Este es un correo de prueba enviado por SmtpMailSender.</p>";

            await mailer.SendEmailAsync(to, subject, body);
            Console.WriteLine("Email send attempted.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error sending test email: {ex}");
            return 2;
        }
    }
}
