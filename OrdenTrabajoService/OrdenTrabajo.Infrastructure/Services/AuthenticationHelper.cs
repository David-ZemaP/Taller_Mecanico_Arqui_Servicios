using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Taller_Mecanico_Arqui.Infrastructure.Services;

public class AuthenticationHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentAuditActor()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.FindFirst(ClaimTypes.Email)?.Value
            ?? user?.FindFirst(ClaimTypes.Name)?.Value
            ?? "sistema";
    }
}
