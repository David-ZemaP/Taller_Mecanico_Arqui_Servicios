namespace Taller_Mecanico_WebService.Helpers;

/// <summary>
/// Helper para gestionar información de auditoría en reportes
/// Proporciona datos del usuario generador y fecha/hora actual
/// </summary>
public class AuditInfoHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditInfoHelper> _logger;

    public AuditInfoHelper(IHttpContextAccessor httpContextAccessor, ILogger<AuditInfoHelper> logger)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene la información de auditoría formateada para reportes
    /// Formato: "Reporte Generado por: [usuario] - [fecha y hora actual]"
    /// </summary>
    public string GetAuditInfo()
    {
        try
        {
            var usuario = ObtenerNombreUsuario();
            var fechaHora = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            return $"Reporte Generado por: {usuario} - {fechaHora}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error obteniendo información de auditoría");
            return $"Reporte Generado por: Sistema - {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
        }
    }

    /// <summary>
    /// Obtiene solo el nombre del usuario actual
    /// </summary>
    public string ObtenerNombreUsuario()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.Name != null)
            {
                return httpContext.User.Identity.Name;
            }

            // Intenta obtener del claim "nom_user"
            var nombreClaim = httpContext?.User?.FindFirst("nom_user")?.Value;
            if (!string.IsNullOrEmpty(nombreClaim))
            {
                return nombreClaim;
            }

            // Intenta obtener de UPN o Email
            var upnClaim = httpContext?.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn")?.Value;
            if (!string.IsNullOrEmpty(upnClaim))
            {
                return upnClaim.Split('@')[0];
            }

            return "admin_sistema";
        }
        catch
        {
            return "admin_sistema";
        }
    }

    /// <summary>
    /// Obtiene la fecha y hora actual formateada
    /// </summary>
    public string ObtenerFechaHoraActual()
    {
        return DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
    }

    /// <summary>
    /// Obtiene metadatos de auditoría como objeto
    /// </summary>
    public Dictionary<string, string> GetAuditMetadata()
    {
        return new Dictionary<string, string>
        {
            { "usuario", ObtenerNombreUsuario() },
            { "fecha_reporte", DateTime.Now.ToString("dd/MM/yyyy") },
            { "hora_reporte", DateTime.Now.ToString("HH:mm:ss") },
            { "fecha_completa", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
        };
    }
}
