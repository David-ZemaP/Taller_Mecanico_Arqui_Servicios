namespace Taller_Mecanico_Users.Framework.DTOs.Users
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
        public string TipoEmpleado { get; set; } = "Mecanico";
        public string EstadoLaboral { get; set; } = "Activo";
        public string? Especialidad { get; set; }
        public decimal? SalarioPorHora { get; set; }
        public decimal? SalarioMensual { get; set; }
        public string? NivelAcceso { get; set; }
    }
}
