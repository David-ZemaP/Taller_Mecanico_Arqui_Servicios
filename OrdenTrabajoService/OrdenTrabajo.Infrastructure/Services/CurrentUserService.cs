using System.Security.Claims;
using Taller_Mecanico_Arqui.Domain.Services;

namespace OrdenTrabajoService.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        public string? GetCurrentUserEmail()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
        }

        public string? GetCurrentUserRole()
        {
            return _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
        }

        public int? GetCurrentEmpleadoId()
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue("EmpleadoId");
            return int.TryParse(value, out var id) ? id : null;
        }

        public int? GetCurrentClienteId()
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue("ClienteId");
            return int.TryParse(value, out var id) ? id : null;
        }
    }
}