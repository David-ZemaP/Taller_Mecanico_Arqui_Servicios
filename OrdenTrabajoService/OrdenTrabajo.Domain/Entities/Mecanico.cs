using Taller_Mecanico_Arqui.Domain.Enums;
using Taller_Mecanico_Arqui.Domain.ValueObjects;

namespace Taller_Mecanico_Arqui.Domain.Entities
{
    public class Mecanico : Empleado
    {
        public string Especialidad { get; private set; } = string.Empty;
        public decimal SalarioPorHora { get; private set; }

        private Mecanico() { }

        public static Mecanico Crear(
            NombreCompleto nombreCompleto,
            DocumentoIdentidad ci,
            int telefono,
            string? email,
            DateTime fechaContratacion,
            string especialidad,
            decimal salarioPorHora,
            EstadoLaboral estadoLaboral = EstadoLaboral.Activo)
        {
            var empleado = Empleado.Crear(nombreCompleto, ci, telefono, email, fechaContratacion, estadoLaboral);
            return new Mecanico
            {
                EmpleadoId = empleado.EmpleadoId,
                NombreCompleto = empleado.NombreCompleto,
                Ci = empleado.Ci,
                Telefono = empleado.Telefono,
                Email = empleado.Email,
                FechaContratacion = empleado.FechaContratacion,
                EstadoLaboral = empleado.EstadoLaboral,
                FechaActualizacion = empleado.FechaActualizacion,
                IsDeleted = empleado.IsDeleted,
                Especialidad = especialidad,
                SalarioPorHora = salarioPorHora
            };
        }

        public static Mecanico Reconstituir(
            int empleadoId,
            NombreCompleto nombreCompleto,
            DocumentoIdentidad ci,
            int telefono,
            string? email,
            DateTime fechaContratacion,
            EstadoLaboral estadoLaboral,
            string especialidad,
            decimal salarioPorHora,
            DateTime? fechaActualizacion = null,
            bool isDeleted = false)
        {
            return new Mecanico
            {
                EmpleadoId = empleadoId,
                NombreCompleto = nombreCompleto,
                Ci = ci,
                Telefono = telefono,
                Email = email,
                FechaContratacion = fechaContratacion,
                EstadoLaboral = estadoLaboral,
                FechaActualizacion = fechaActualizacion,
                IsDeleted = isDeleted,
                Especialidad = especialidad,
                SalarioPorHora = salarioPorHora
            };
        }

        public void ActualizarEspecialidad(string especialidad)
        {
            Especialidad = especialidad;
            FechaActualizacion = DateTime.UtcNow;
        }

        public void ActualizarSalarioPorHora(decimal salario)
        {
            SalarioPorHora = salario;
            FechaActualizacion = DateTime.UtcNow;
        }
    }
}
