using System.ComponentModel.DataAnnotations;
using WebService.Models;

namespace WebService.DTOs
{
    public class EmpleadoDto
    {
        public int EmpleadoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public int Ci { get; set; }
        public string? CiComplemento { get; set; }
        public int Telefono { get; set; }
        public string? Email { get; set; }
        public DateTime FechaContratacion { get; set; }
        public string TipoEmpleado { get; set; } = string.Empty;
        public string EstadoLaboral { get; set; } = string.Empty;
        public string? Especialidad { get; set; }
        public decimal? SalarioPorHora { get; set; }
        public decimal? SalarioMensual { get; set; }
        public string? NivelAcceso { get; set; }

        public string NombreCompleto =>
            string.IsNullOrWhiteSpace(SegundoApellido)
                ? $"{Nombre} {PrimerApellido}"
                : $"{Nombre} {PrimerApellido} {SegundoApellido}";
    }

    public class EmpleadoFormDto
    {
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "El primer apellido es obligatorio.")]
        public string PrimerApellido { get; set; } = string.Empty;

        public string? SegundoApellido { get; set; }

        [Required(ErrorMessage = "El CI es obligatorio.")]
        [Range(100000, 99999999, ErrorMessage = "El CI debe tener entre 6 y 8 dígitos.")]
        public int CiNumero { get; set; }

        public string? CiComplemento { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public int Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "La fecha de contratación es obligatoria.")]
        public DateTime FechaContratacion { get; set; }

        public string TipoEmpleado { get; set; } = "Mecanico";
        public string EstadoLaboral { get; set; } = "Activo";
        public string? Especialidad { get; set; }
        public decimal? SalarioPorHora { get; set; }
        public decimal? SalarioMensual { get; set; }
        public string? NivelAcceso { get; set; }
    }

    public class UsuarioDto
    {
        public int UsuarioLoginId { get; set; }
        public int? EmpleadoId { get; set; }
        public int? ClienteId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime? UltimoAcceso { get; set; }
        public bool Activo { get; set; }
        public bool RequiereCambioPassword { get; set; }
        public bool EsCliente { get; set; }
    }

    public class UsuarioViewModel
    {
        public int UsuarioLoginId { get; set; }
        public string Email { get; set; } = string.Empty;
        public int EmpleadoId { get; set; }
        public string EmpleadoNombre { get; set; } = string.Empty;
        public DateTime? UltimoAcceso { get; set; }
        public bool Activo { get; set; }
        public NivelAcceso AdminNivelAcceso { get; set; }
    }

    public class UsuarioFormDto
    {
        public int UsuarioLoginId { get; set; }

        [Required(ErrorMessage = "El empleado es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un empleado válido.")]
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        public string Email { get; set; } = string.Empty;

        public string? Password { get; set; }
    }

    public class CrearEmpleadoFormDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public int Ci { get; set; }
        public string? CiComplemento { get; set; }
        public int Telefono { get; set; }
        public string? Email { get; set; }
        public string TipoEmpleado { get; set; } = "Mecanico";
        public string EstadoLaboral { get; set; } = "Activo";
        public string? NivelAcceso { get; set; }
        public string? Especialidad { get; set; }
        public decimal? SalarioPorHora { get; set; }
        public decimal? SalarioMensual { get; set; }
    }
}
