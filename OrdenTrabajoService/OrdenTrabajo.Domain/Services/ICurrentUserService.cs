namespace Taller_Mecanico_Arqui.Domain.Services
{
    public interface ICurrentUserService
    {
        string? GetCurrentUserId();
        string? GetCurrentUserEmail();
        string? GetCurrentUserRole();
        int? GetCurrentEmpleadoId();
        int? GetCurrentClienteId();
    }
}