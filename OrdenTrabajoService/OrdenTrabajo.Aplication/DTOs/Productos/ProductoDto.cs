namespace Taller_Mecanico_Arqui.Application.DTOs.Productos
{
    public class CreateProductoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public double Precio { get; set; }
        public int Stock { get; set; }
    }

    public class UpdateProductoDto
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public double Precio { get; set; }
        public int Stock { get; set; }
    }

    public class ProductoListDto
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public double Precio { get; set; }
        public int Stock { get; set; }
    }
}
