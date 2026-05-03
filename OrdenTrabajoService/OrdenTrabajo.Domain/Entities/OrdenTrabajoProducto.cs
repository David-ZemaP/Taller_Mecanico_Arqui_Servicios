namespace Taller_Mecanico_Arqui.Domain.Entities
{
    public class OrdenTrabajoProducto
    {
        public int OrdenTrabajoProductoId { get; private set; }
        public int OrdenTrabajoId { get; private set; }
        public int ProductoId { get; private set; }
        public int Cantidad { get; private set; }
        public double PrecioUnitario { get; private set; }
        public double Subtotal { get; private set; }
        public string Nombre { get; private set; } = string.Empty;

        private OrdenTrabajoProducto() { }

        public OrdenTrabajoProducto(int ordenTrabajoId, int productoId, int cantidad, double precioUnitario, double subtotal)
        {
            OrdenTrabajoId = ordenTrabajoId;
            ProductoId = productoId;
            Cantidad = cantidad;
            PrecioUnitario = precioUnitario;
            Subtotal = subtotal;
        }

        public static OrdenTrabajoProducto Reconstituir(int ordenTrabajoProductoId, int ordenTrabajoId, int productoId, int cantidad, double precioUnitario, double subtotal, string? nombre = null)
        {
            return new OrdenTrabajoProducto(ordenTrabajoId, productoId, cantidad, precioUnitario, subtotal)
            {
                OrdenTrabajoProductoId = ordenTrabajoProductoId,
                Nombre = nombre ?? string.Empty
            };
        }
    }
}