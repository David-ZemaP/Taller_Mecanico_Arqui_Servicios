namespace Taller_Mecanico_Arqui.Domain.Entities
{
    public class OrdenTrabajoServicio
    {
        public int OrdenTrabajoServicioId { get; private set; }
        public int OrdenTrabajoId { get; private set; }
        public int ServicioId { get; private set; }
        public int Cantidad { get; private set; }
        public double PrecioUnitario { get; private set; }
        public double Subtotal { get; private set; }
        public string Nombre { get; private set; } = string.Empty;

        private OrdenTrabajoServicio() { }

        public OrdenTrabajoServicio(int ordenTrabajoId, int servicioId, int cantidad, double precioUnitario, double subtotal)
        {
            OrdenTrabajoId = ordenTrabajoId;
            ServicioId = servicioId;
            Cantidad = cantidad;
            PrecioUnitario = precioUnitario;
            Subtotal = subtotal;
        }

        public static OrdenTrabajoServicio Reconstituir(int ordenTrabajoServicioId, int ordenTrabajoId, int servicioId, int cantidad, double precioUnitario, double subtotal, string? nombre = null)
        {
            return new OrdenTrabajoServicio(ordenTrabajoId, servicioId, cantidad, precioUnitario, subtotal)
            {
                OrdenTrabajoServicioId = ordenTrabajoServicioId,
                Nombre = nombre ?? string.Empty
            };
        }
    }
}