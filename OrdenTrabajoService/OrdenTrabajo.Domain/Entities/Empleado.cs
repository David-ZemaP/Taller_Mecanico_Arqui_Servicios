using Taller_Mecanico_Arqui.Domain.Enums;
using Taller_Mecanico_Arqui.Domain.ValueObjects;

namespace Taller_Mecanico_Arqui.Domain.Entities
{
    public class Empleado : Persona
    {
        public int EmpleadoId { get; protected set; }
        public string TipoEmpleado { get; protected set; } = string.Empty;
        public DateTime FechaContratacion { get; protected set; }
        public EstadoLaboral EstadoLaboral { get; protected set; }

        protected Empleado() { }

        public static Empleado Crear(
            NombreCompleto nombreCompleto,
            DocumentoIdentidad ci,
            int telefono,
            string? email,
            DateTime fechaContratacion,
            string tipoEmpleado,
            EstadoLaboral estadoLaboral = EstadoLaboral.Activo)
        {
            return new Empleado
            {
                NombreCompleto = nombreCompleto,
                Ci = ci,
                Telefono = telefono,
                Email = email,
                TipoEmpleado = tipoEmpleado,
                FechaContratacion = fechaContratacion,
                EstadoLaboral = estadoLaboral
            };
        }

        public static Empleado Reconstituir(
            int empleadoId,
            NombreCompleto nombreCompleto,
            DocumentoIdentidad ci,
            int telefono,
            string? email,
            DateTime fechaContratacion,
            string tipoEmpleado,
            EstadoLaboral estadoLaboral,
            DateTime? fechaActualizacion = null,
            bool isDeleted = false)
        {
            return new Empleado
            {
                EmpleadoId = empleadoId,
                NombreCompleto = nombreCompleto,
                Ci = ci,
                Telefono = telefono,
                Email = email,
                TipoEmpleado = tipoEmpleado,
                FechaContratacion = fechaContratacion,
                EstadoLaboral = estadoLaboral,
                FechaActualizacion = fechaActualizacion,
                IsDeleted = isDeleted
            };
        }

        public void SetId(int id)
        {
            EmpleadoId = id;
        }

        public void ActualizarEstadoLaboral(EstadoLaboral estado)
        {
            EstadoLaboral = estado;
            FechaActualizacion = DateTime.UtcNow;
        }
    }
}
