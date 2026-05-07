namespace OrdenTrabajoService.Domain.Entities
{
    public class ColorVehiculo
    {
        public int ColorVehiculoId { get; private set; }
        public string Nombre { get; private set; } = string.Empty;

        private ColorVehiculo() { }

        public static ColorVehiculo Crear(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del color es obligatorio.");
            return new ColorVehiculo { Nombre = nombre.Trim() };
        }

        public static ColorVehiculo Reconstituir(int colorVehiculoId, string nombre)
            => new() { ColorVehiculoId = colorVehiculoId, Nombre = nombre };

        public void ActualizarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del color es obligatorio.");
            Nombre = nombre.Trim();
        }
    }
}
