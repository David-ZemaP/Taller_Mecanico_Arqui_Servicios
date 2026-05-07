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
}
