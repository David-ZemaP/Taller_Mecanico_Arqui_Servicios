namespace Taller_Mecanico_Arqui.Frontend.DTOs
{
    /// <summary>
    /// DTO para el formulario de crear/editar producto
    /// </summary>
    public class ProductoFormDto
    {
        public int ProductoId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public int StockMinimo { get; set; }

        public bool Activo { get; set; } = true;
    }
}