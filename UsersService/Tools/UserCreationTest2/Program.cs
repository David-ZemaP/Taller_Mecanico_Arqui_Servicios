using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Taller_Mecanico_Users.App.Infrastructure;
using Taller_Mecanico_Users.App.Services;
using Taller_Mecanico_Users.Data.Repositories;
using Taller_Mecanico_Users.Framework.Services;
using Taller_Mecanico_Users.UseCases.Users;

class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== User Creation Test with SMTP ===");
            
            var baseDir = Directory.GetCurrentDirectory();
            var appSettingsPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "App", "appsettings.json"));
            Console.WriteLine($"Loading config from: {appSettingsPath}");

            var configuration = new ConfigurationBuilder()
                .AddJsonFile(appSettingsPath, optional: false, reloadOnChange: false)
                .Build();

            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

            // Create services
            Console.WriteLine("Initializing services...");
            var mockAuthHelper = new MockAuthenticationHelper();
            var sqlConnFactory = new SqlConnectionFactory(configuration);
            var smtpSettings = new SmtpSettings(configuration);
            var logger = loggerFactory.CreateLogger<SmtpMailSender>();
            var mailSender = new SmtpMailSender(smtpSettings, logger);
            var auditService = new AuditService(mockAuthHelper);
            var repository = new UsuarioLoginRepository(sqlConnFactory, auditService);
            var passwordSecurity = new PasswordSecurityService();
            var passwordHasher = new BcryptPasswordHasher();

            var createUserUseCase = new CreateUserUseCase(repository, mailSender, passwordSecurity, passwordHasher);

            // Test email address
            var testEmail = "testuser" + DateTime.Now.Ticks + "@example.com";
            Console.WriteLine($"\nCreating user with email: {testEmail}");
            
            var result = await createUserUseCase.ExecuteAsync(10, testEmail);

            if (result.IsFailure)
            {
                Console.WriteLine($"FAILED: {result.ErrorCode} - {result.ErrorMessage}");
                return 1;
            }
            else
            {
                Console.WriteLine($"SUCCESS: User created with ID {result.Value?.UsuarioLoginId}");
                Console.WriteLine($"Email with temporary password should have been sent to: {testEmail}");
                return 0;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex}");
            return 2;
        }
    }
}

class MockAuthenticationHelper : IAuthenticationHelper
{
    public string GetCurrentAuditActor()
    {
        return "test_user@test.com";
    }
}
