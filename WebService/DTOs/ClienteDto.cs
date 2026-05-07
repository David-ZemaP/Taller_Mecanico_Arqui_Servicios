namespace WebService.DTOs
{
    public class ClienteDto
    {
        public int UsuarioLoginId { get; set; }
        public int? ClienteId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime? UltimoAcceso { get; set; }
        public bool Activo { get; set; }
        public bool RequiereCambioPassword { get; set; }
    }

    public class ClienteLookupDto
    {
        public int ClienteId { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public int CiNumero { get; set; }
        public string? CiComplemento { get; set; }
        public int Telefono { get; set; }
        public string? Email { get; set; }
        public string? FechaRegistro { get; set; }
        public string NombreCompleto => $"{Nombres} {PrimerApellido}{(string.IsNullOrWhiteSpace(SegundoApellido) ? "" : " " + SegundoApellido)}";
    }

    public class ClienteFormDto
    {
        public int ClienteId { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public int CiNumero { get; set; }
        public string? CiComplemento { get; set; }
        public int Telefono { get; set; }
        public string? Email { get; set; }
    }
}
