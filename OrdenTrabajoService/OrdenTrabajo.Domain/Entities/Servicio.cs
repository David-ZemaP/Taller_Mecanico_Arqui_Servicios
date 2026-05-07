namespace Taller_Mecanico_Arqui.Domain.Entities
{
    public class Servicio
    {
        public int ServicioId { get; private set; }
        public string Nombre { get; private set; } = string.Empty;
        public double Precio { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? FechaActualizacion { get; private set; }

        // Auditoría
        public string? CreadoPor { get; private set; }
        public string? ActualizadoPor { get; private set; }
        public string? EliminadoPor { get; private set; }

        private Servicio() { }

        public static Servicio Crear(string nombre, double precio)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ArgumentException("El nombre del servicio es obligatorio.");
            }

            if (precio < 0)
            {
                throw new ArgumentException("El precio del servicio (Bs.) no puede ser negativo.");
            }

            return new Servicio
            {
                Nombre = nombre.Trim(),
                Precio = precio
            };
        }

        public static Servicio Reconstituir(int servicioId, string nombre, double precio)
        {
            return new Servicio
            {
                ServicioId = servicioId,
                Nombre = nombre,
                Precio = precio
            };
        }

        public void ActualizarDatos(string nombre, double precio)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ArgumentException("El nombre del servicio es obligatorio.");
            }

            if (precio < 0)
            {
                throw new ArgumentException("El precio del servicio (Bs.) no puede ser negativo.");
            }

            Nombre = nombre.Trim();
            Precio = precio;
            FechaActualizacion = DateTime.UtcNow;
        }

        public void MarcarEliminado(string? eliminadoPor = null)
        {
            IsDeleted = true;
            EliminadoPor = eliminadoPor;
            FechaActualizacion = DateTime.UtcNow;
        }

        public void SetAuditoriaCreacion(string? creadoPor)
        {
            CreadoPor = creadoPor;
        }

        public void SetAuditoriaActualizacion(string? actualizadoPor)
        {
            ActualizadoPor = actualizadoPor;
            FechaActualizacion = DateTime.UtcNow;
        }
    }
}
