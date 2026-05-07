using Taller_Mecanico_Arqui.Domain.ValueObjects;

namespace Taller_Mecanico_Arqui.Domain.Entities
{
    public abstract class Persona
    {
        public NombreCompleto NombreCompleto { get; protected set; } = null!;
        public DocumentoIdentidad Ci { get; protected set; } = null!;
        public int Telefono { get; protected set; }
        public string? Email { get; protected set; }
        public DateTime? FechaActualizacion { get; protected set; }
        public bool IsDeleted { get; protected set; }

        // Auditoría
        public string? CreadoPor { get; protected set; }
        public string? ActualizadoPor { get; protected set; }
        public string? EliminadoPor { get; protected set; }

        protected void ActualizarEmail(string? email)
        {
            Email = email;
            FechaActualizacion = DateTime.UtcNow;
        }

        protected void ActualizarTelefono(int telefono)
        {
            Telefono = telefono;
            FechaActualizacion = DateTime.UtcNow;
        }

        public void MarcarEliminado(string? eliminadoPor = null)
        {
            IsDeleted = true;
            EliminadoPor = eliminadoPor;
            FechaActualizacion = DateTime.UtcNow;
        }

        public void SetAuditoriaCreacion(string? creadoPor)
        {
            CreadoPor = creadoPor;
        }

        public void SetAuditoriaActualizacion(string? actualizadoPor)
        {
            ActualizadoPor = actualizadoPor;
            FechaActualizacion = DateTime.UtcNow;
        }
    }
}
