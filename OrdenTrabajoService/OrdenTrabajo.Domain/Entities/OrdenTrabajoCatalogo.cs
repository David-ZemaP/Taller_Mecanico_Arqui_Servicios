namespace OrdenTrabajoService.Domain.Entities
{
    public class OrdenTrabajoCatalogo
    {
        public int OrdenTrabajoCatalogoId { get; private set; }
        public int OrdenTrabajoId { get; private set; }
        public int ProductoId { get; private set; }
        public int CantidadUtilizada { get; private set; }
        public decimal PrecioUnitario { get; private set; }
        public DateTime FechaRegistro { get; private set; }

        private OrdenTrabajoCatalogo() { }

        public static OrdenTrabajoCatalogo Crear(int ordenTrabajoId, int productoId, int cantidadUtilizada, decimal precioUnitario)
        {
            return new OrdenTrabajoCatalogo
            {
                OrdenTrabajoId = ordenTrabajoId,
                ProductoId = productoId,
                CantidadUtilizada = cantidadUtilizada,
                PrecioUnitario = precioUnitario,
                FechaRegistro = DateTime.UtcNow
            };
        }
    }
}

