using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Npgsql;
using Taller_Mecanico_Users.Domain.Ports;

namespace Taller_Mecanico_Users.App.Middleware
{
    public class RequirePasswordChangeMiddleware
    {
        private readonly RequestDelegate _next;
        private const string DEFAULT_ADMIN_EMAIL = "administrador.principal@taller.com";

        public RequirePasswordChangeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IUsuarioLoginRepository loginRepository,
            ILogger<RequirePasswordChangeMiddleware> logger)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

                var allowedPaths = new[] 
                { 
                    "/changepassword", 
                    "/logout", 
                    "/login",
                    "/lib/",
                    "/css/",
                    "/js/",
                    "/fotos/"
                };

                bool isAllowedPath = allowedPaths.Any(allowed => path.StartsWith(allowed));

                if (!isAllowedPath)
                {
                    var clienteIdClaim = context.User.FindFirst("ClienteId")?.Value;
                    
                    if (!string.IsNullOrEmpty(clienteIdClaim) && int.TryParse(clienteIdClaim, out int clienteId))
                    {
                        try
                        {
                            var allLogins = await loginRepository.GetAllAsync();
                            var usuario = allLogins.FirstOrDefault(u => u.ClienteId == clienteId);

                            if (usuario != null && usuario.RequiereCambioPassword)
                            {
                                context.Response.Redirect("/ChangePassword");
                                return;
                            }
                        }
                        catch (NpgsqlException ex)
                        {
                            logger.LogWarning(ex, "No se pudo verificar el estado de cambio de contraseña por indisponibilidad de PostgreSQL.");
                        }
                    }
                    else
                    {
                        var empleadoIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                        if (!string.IsNullOrEmpty(empleadoIdClaim) && int.TryParse(empleadoIdClaim, out int empleadoId))
                        {
                            try
                            {
                                var allLogins = await loginRepository.GetAllAsync();
                                var usuario = allLogins.FirstOrDefault(u => u.EmpleadoId == empleadoId);

                                if (usuario != null)
                                {
                                    if (usuario.RequiereCambioPassword && usuario.Email != DEFAULT_ADMIN_EMAIL)
                                    {
                                        context.Response.Redirect("/ChangePassword");
                                        return;
                                    }
                                }
                            }
                            catch (NpgsqlException ex)
                            {
                                logger.LogWarning(ex, "No se pudo verificar el estado de cambio de contraseña por indisponibilidad de PostgreSQL.");
                            }
                        }
                    }
                }
            }

            await _next(context);
        }
    }

    public static class RequirePasswordChangeMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequirePasswordChange(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequirePasswordChangeMiddleware>();
        }
    }
}
