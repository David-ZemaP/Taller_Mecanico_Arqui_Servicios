namespace OrdenTrabajoService.Domain.Entities
{
    public class Servicio
    {
        public int ServicioId { get; private set; }
        public string Nombre { get; private set; } = string.Empty;
        public double Precio { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? FechaActualizacion { get; private set; }

        private Servicio() { }

        public static Servicio Crear(string nombre, double precio)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del servicio es obligatorio.");
            if (precio < 0)
                throw new ArgumentException("El precio no puede ser negativo.");
            return new Servicio { Nombre = nombre.Trim(), Precio = precio };
        }

        public static Servicio Reconstituir(int servicioId, string nombre, double precio, bool isDeleted = false)
            => new() { ServicioId = servicioId, Nombre = nombre, Precio = precio, IsDeleted = isDeleted };

        public void ActualizarDatos(string nombre, double precio)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del servicio es obligatorio.");
            if (precio < 0)
                throw new ArgumentException("El precio no puede ser negativo.");
            Nombre = nombre.Trim();
            Precio = precio;
            FechaActualizacion = DateTime.UtcNow;
        }
    }
}
