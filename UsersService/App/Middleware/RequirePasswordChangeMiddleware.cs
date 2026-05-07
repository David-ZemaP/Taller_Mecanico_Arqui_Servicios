using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Taller_Mecanico_Users.App.Middleware
{
    public class RequirePasswordChangeMiddleware
    {
        private readonly RequestDelegate _next;

        public RequirePasswordChangeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ILogger<RequirePasswordChangeMiddleware> logger)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
                var isLoginPath = path.StartsWith("/api/auth/login");
                var isChangePasswordPath = path.StartsWith("/api/users/") && path.Contains("/change-password");

                if (!isLoginPath && !isChangePasswordPath)
                {
                    var requiresPasswordChangeClaim = context.User.FindFirst("RequiereCambio")?.Value;
                    if (bool.TryParse(requiresPasswordChangeClaim, out var requiresPasswordChange) && requiresPasswordChange)
                    {
                        logger.LogInformation("Bloqueando acceso hasta que el usuario cambie su contraseña.");

                        context.Response.StatusCode = StatusCodes.Status428PreconditionRequired;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonSerializer.Serialize(new
                        {
                            code = "PASSWORD_CHANGE_REQUIRED",
                            message = "Debe cambiar su contraseña antes de continuar."
                        }));
                        return;
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
