namespace Taller_Mecanico_Arqui.Frontend.DTOs
{
    /// <summary>
    /// DTO para el formulario de crear/editar servicio
    /// </summary>
    public class ServicioFormDto
    {
        public int ServicioId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public decimal Precio { get; set; }

        public bool Activo { get; set; } = true;
    }
}