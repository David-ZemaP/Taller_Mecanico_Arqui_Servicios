using System.Security.Claims;

namespace Taller_Mecanico_WebService.Middleware;

public class RequirePasswordChangeMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly HashSet<string> _exemptPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/login",
        "/logout",
        "/accesodenegado",
        "/changepassword"
    };

    public RequirePasswordChangeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        var isExempt = _exemptPaths.Contains(path)
            || path.StartsWith("/_", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/css", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/js", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/lib", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase);

        if (!isExempt
            && context.User.Identity?.IsAuthenticated == true
            && context.User.FindFirst("RequiereCambio")?.Value is "True" or "true")
        {
            context.Response.Redirect("/ChangePassword");
            return;
        }

        await _next(context);
    }
}
