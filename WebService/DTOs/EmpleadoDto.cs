namespace WebService.DTOs
{
    public class EmpleadoDto
    {
        public int UsuarioLoginId { get; set; }
        public int? EmpleadoId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime? UltimoAcceso { get; set; }
        public bool Activo { get; set; }
        public bool RequiereCambioPassword { get; set; }
        public bool EsCliente { get; set; }
    }
}
