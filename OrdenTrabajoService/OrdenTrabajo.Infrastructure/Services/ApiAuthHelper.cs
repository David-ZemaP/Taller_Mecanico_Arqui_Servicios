using System.Security.Claims;
using Taller_Mecanico_Users.Framework.Services;

namespace OrdenTrabajoService.Infrastructure.Services
{
    public class ApiAuthHelper : IAuthenticationHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiAuthHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetCurrentAuditActor()
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx?.User?.Identity?.IsAuthenticated != true)
                return "sistema";

            var empleadoId = ctx.User.FindFirst("EmpleadoId")?.Value;
            if (!string.IsNullOrWhiteSpace(empleadoId))
                return empleadoId;

            var email = ctx.User.FindFirst(ClaimTypes.Email)?.Value;
            if (!string.IsNullOrWhiteSpace(email))
                return email;

            return ctx.User.FindFirst(ClaimTypes.Name)?.Value ?? "sistema";
        }
    }
}
