namespace Taller_Mecanico_Users.Framework.Services
{
    /// <summary>
    /// Configuración de JWT encapsulada en un servicio.
    /// Responsabilidad: Proporcionar acceso a valores de configuración JWT de forma segura.
    /// Inyectable - La presentación no accede directamente a IConfiguration.
    /// </summary>
    public interface IJwtSettings
    {
        string Secret { get; }
        string Issuer { get; }
        string Audience { get; }
        int ExpirationInMinutes { get; }
    }
}
