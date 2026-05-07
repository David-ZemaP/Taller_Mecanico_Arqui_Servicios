using Microsoft.Extensions.Configuration;

namespace Taller_Mecanico_Users.Framework.Services
{
    /// <summary>
    /// Implementación concreta de IJwtSettings.
    /// Lee los valores de IConfiguration en el constructor y los expone como propiedades.
    /// De esta forma, los consumers no necesitan inyectar IConfiguration directamente.
    /// </summary>
    public class JwtSettings : IJwtSettings
    {
        public string Secret { get; }
        public string Issuer { get; }
        public string Audience { get; }
        public int ExpirationInMinutes { get; }

        public JwtSettings(IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            Secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret no configurado.");
            Issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer no configurado.");
            Audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience no configurado.");
            
            if (!int.TryParse(jwtSettings["ExpirationInMinutes"], out int expirationMinutes))
            {
                throw new InvalidOperationException("JWT ExpirationInMinutes debe ser un número válido.");
            }
            
            ExpirationInMinutes = expirationMinutes;
        }
    }
}
