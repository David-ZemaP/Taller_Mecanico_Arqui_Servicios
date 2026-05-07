using Taller_Mecanico_Arqui.Domain.Enums;
using Taller_Mecanico_Arqui.Domain.ValueObjects;

namespace Taller_Mecanico_Arqui.Domain.Entities
{
    public class Administrador : Empleado
    {
        public decimal SalarioMensual { get; private set; }
        public NivelAcceso NivelAcceso { get; private set; } = NivelAcceso.Completo;

        private Administrador() { }

        public static Administrador Crear(
            NombreCompleto nombreCompleto,
            DocumentoIdentidad ci,
            int telefono,
            string? email,
            DateTime fechaContratacion,
            decimal salarioMensual,
            NivelAcceso nivelAcceso = NivelAcceso.Completo,
            EstadoLaboral estadoLaboral = EstadoLaboral.Activo)
        {
            var empleado = Empleado.Crear(nombreCompleto, ci, telefono, email, fechaContratacion, nameof(Administrador), estadoLaboral);
            return new Administrador
            {
                EmpleadoId = empleado.EmpleadoId,
                NombreCompleto = empleado.NombreCompleto,
                Ci = empleado.Ci,
                Telefono = empleado.Telefono,
                Email = empleado.Email,
                TipoEmpleado = empleado.TipoEmpleado,
                FechaContratacion = empleado.FechaContratacion,
                EstadoLaboral = empleado.EstadoLaboral,
                FechaActualizacion = empleado.FechaActualizacion,
                IsDeleted = empleado.IsDeleted,
                SalarioMensual = salarioMensual,
                NivelAcceso = nivelAcceso
            };
        }

        public static Administrador Reconstituir(
            int empleadoId,
            NombreCompleto nombreCompleto,
            DocumentoIdentidad ci,
            int telefono,
            string? email,
            DateTime fechaContratacion,
            EstadoLaboral estadoLaboral,
            decimal salarioMensual,
            NivelAcceso nivelAcceso,
            DateTime? fechaActualizacion = null,
            bool isDeleted = false)
        {
            return new Administrador
            {
                EmpleadoId = empleadoId,
                NombreCompleto = nombreCompleto,
                Ci = ci,
                Telefono = telefono,
                Email = email,
                TipoEmpleado = nameof(Administrador),
                FechaContratacion = fechaContratacion,
                EstadoLaboral = estadoLaboral,
                FechaActualizacion = fechaActualizacion,
                IsDeleted = isDeleted,
                SalarioMensual = salarioMensual,
                NivelAcceso = nivelAcceso
            };
        }

        public void ActualizarNivelAcceso(NivelAcceso nivelAcceso)
        {
            NivelAcceso = nivelAcceso;
            FechaActualizacion = DateTime.UtcNow;
        }

        public void ActualizarSalarioMensual(decimal salario)
        {
            SalarioMensual = salario;
            FechaActualizacion = DateTime.UtcNow;
        }
    }
}
