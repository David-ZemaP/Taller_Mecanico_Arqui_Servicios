using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Taller_Mecanico_Arqui.Frontend.Authorization;

public enum NivelAcceso
{
    Cliente = 0,
    Parcial = 1,
    Completo = 2,
    Gerente = 3
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireAccessLevelAttribute : Attribute, IAsyncPageFilter
{
    public NivelAcceso RequiredLevel { get; }
    public IReadOnlyCollection<NivelAcceso> AllowedLevels { get; }
    private readonly bool _useExplicitAllowedLevels;

    public RequireAccessLevelAttribute(NivelAcceso requiredLevel, params NivelAcceso[] allowedLevels)
    {
        RequiredLevel = requiredLevel;
        _useExplicitAllowedLevels = allowedLevels.Length > 0;
        AllowedLevels = allowedLevels.Length > 0 ? allowedLevels : new[] { requiredLevel };
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        return Task.CompletedTask;
    }

    public Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var user = context.HttpContext.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            context.Result = new ChallengeResult();
            return Task.CompletedTask;
        }

        var userLevel = ResolveUserLevel(user);
        var hasAccess = _useExplicitAllowedLevels
            ? AllowedLevels.Contains(userLevel)
            : userLevel >= RequiredLevel;

        if (!hasAccess)
        {
            context.Result = new RedirectToPageResult("/AccesoDenegado");
            return Task.CompletedTask;
        }

        return next();
    }

    private static NivelAcceso ResolveUserLevel(ClaimsPrincipal user)
    {
        var claimValue = user.FindFirst("NivelAcceso")?.Value ?? user.FindFirst(ClaimTypes.Role)?.Value;

        if (Enum.TryParse(claimValue, true, out NivelAcceso level))
        {
            return level;
        }

        if (string.Equals(claimValue, "Admin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(claimValue, "Administrador", StringComparison.OrdinalIgnoreCase))
        {
            return NivelAcceso.Completo;
        }

        if (string.Equals(claimValue, "Empleado", StringComparison.OrdinalIgnoreCase))
        {
            return NivelAcceso.Parcial;
        }

        return NivelAcceso.Parcial;
    }
}