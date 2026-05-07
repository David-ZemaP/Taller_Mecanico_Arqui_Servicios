namespace Taller_Mecanico_Users.Domain.Ports
{
    public interface IEmpleadoRepository
    {
        Task<IEnumerable<EmpleadoRecord>> GetAllAsync();
        Task<EmpleadoRecord?> GetByIdAsync(int id);
        Task<int> CreateAsync(NuevoEmpleadoRecord data);
        Task UpdateAsync(int id, NuevoEmpleadoRecord data);
        Task DeleteAsync(int id);
    }

    public sealed record EmpleadoRecord(
        int EmpleadoId,
        string Nombre,
        string PrimerApellido,
        string? SegundoApellido,
        int Ci,
        string? CiComplemento,
        int Telefono,
        string? Email,
        DateTime FechaContratacion,
        string TipoEmpleado,
        string EstadoLaboral,
        string? Especialidad,
        decimal? SalarioPorHora,
        decimal? SalarioMensual,
        string? NivelAcceso
    );

    public sealed record NuevoEmpleadoRecord(
        string Nombre,
        string PrimerApellido,
        string? SegundoApellido,
        int Ci,
        string? CiComplemento,
        int Telefono,
        string? Email,
        DateTime FechaContratacion,
        string TipoEmpleado,
        string EstadoLaboral,
        string? Especialidad,
        decimal? SalarioPorHora,
        decimal? SalarioMensual,
        string? NivelAcceso,
        string? CreadoPor
    );
}
