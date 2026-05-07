using System.Text.Json.Serialization;

namespace Taller_Mecanico_Arqui.Application.DTOs.Empleados
{
    public class CreateEmpleadoDto
    {
        public string Nombres { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public int CiNumero { get; set; }
        public string? CiComplemento { get; set; }
        public int Telefono { get; set; }
        public string? Email { get; set; }
        public DateTime FechaContratacion { get; set; }
        [JsonPropertyName("cargo")]
        public string TipoEmpleado { get; set; } = "Mecanico";
        public string EstadoLaboral { get; set; } = "Activo";
        public string? Especialidad { get; set; }
        public decimal? SalarioPorHora { get; set; }
        public decimal? SalarioMensual { get; set; }
        public string? NivelAcceso { get; set; }
    }

    public class UpdateEmpleadoDto
    {
        public int EmpleadoId { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public int CiNumero { get; set; }
        public string? CiComplemento { get; set; }
        public int Telefono { get; set; }
        public string? Email { get; set; }
        public DateTime FechaContratacion { get; set; }
        [JsonPropertyName("cargo")]
        public string TipoEmpleado { get; set; } = "Mecanico";
        public string EstadoLaboral { get; set; } = "Activo";
        public string? Especialidad { get; set; }
        public decimal? SalarioPorHora { get; set; }
        public decimal? SalarioMensual { get; set; }
        public string? NivelAcceso { get; set; }
    }

    public class EmpleadoListDto
    {
        public int EmpleadoId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Ci { get; set; } = string.Empty;
        public int Telefono { get; set; }
        public string? Email { get; set; }
        [JsonPropertyName("cargo")]
        public string TipoEmpleado { get; set; } = string.Empty;
        public string EstadoLaboral { get; set; } = string.Empty;
        public DateTime FechaContratacion { get; set; }
    }
}
