using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Taller_Mecanico_Users.Domain.Enums;
using Taller_Mecanico_Users.Framework.Services;

namespace Taller_Mecanico_Users.App.Services;

public class AuthenticationHelper : IAuthenticationHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public NivelAcceso? GetCurrentUserAccessLevel()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var nivelAccesoClaim = httpContext.User.FindFirst("NivelAcceso");
        if (nivelAccesoClaim != null && Enum.TryParse<NivelAcceso>(nivelAccesoClaim.Value, out var level))
        {
            return level;
        }

        return null;
    }

    public int? GetCurrentUserEmployeeId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var employeeIdClaim = httpContext.User.FindFirst("EmpleadoId")?.Value
            ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrWhiteSpace(employeeIdClaim) && int.TryParse(employeeIdClaim, out var employeeId))
        {
            return employeeId;
        }

        return null;
    }

    public string GetCurrentAuditActor()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return "sistema";
        }

        var employeeIdClaim = httpContext.User.FindFirst("EmpleadoId")?.Value;
        if (!string.IsNullOrWhiteSpace(employeeIdClaim))
        {
            return employeeIdClaim;
        }

        var clientIdClaim = httpContext.User.FindFirst("ClienteId")?.Value;
        if (!string.IsNullOrWhiteSpace(clientIdClaim))
        {
            return clientIdClaim;
        }

        var emailClaim = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrWhiteSpace(emailClaim))
        {
            return emailClaim;
        }

        var nameClaim = httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        return string.IsNullOrWhiteSpace(nameClaim) ? "sistema" : nameClaim;
    }

    public async Task ForceLogoutAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }

    public async Task UpdateAccessLevelClaimAsync(NivelAcceso newLevel)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var existingClaims = httpContext.User.Claims
            .Where(c => c.Type != "NivelAcceso")
            .ToList();

        existingClaims.Add(new Claim("NivelAcceso", newLevel.ToString()));

        var claimsIdentity = new ClaimsIdentity(existingClaims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties { IsPersistent = true };

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }

    public bool CanModifyAdmin(NivelAcceso targetAdminLevel)
    {
        var currentLevel = GetCurrentUserAccessLevel();
        if (currentLevel == null)
        {
            return false;
        }

        return currentLevel.Value switch
        {
            NivelAcceso.Gerente => true,
            NivelAcceso.Completo => targetAdminLevel == NivelAcceso.Parcial,
            NivelAcceso.Parcial => false,
            _ => false
        };
    }

    public bool CanCreateAdmin(NivelAcceso newAdminLevel)
    {
        var currentLevel = GetCurrentUserAccessLevel();
        if (currentLevel == null)
        {
            return false;
        }

        return currentLevel.Value switch
        {
            NivelAcceso.Gerente => newAdminLevel != NivelAcceso.Gerente,
            NivelAcceso.Completo => newAdminLevel == NivelAcceso.Parcial,
            NivelAcceso.Parcial => false,
            _ => false
        };
    }
}
