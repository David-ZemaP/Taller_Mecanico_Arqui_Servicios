namespace Taller_Mecanico_Arqui.Domain.Entities
{
    public class ColorVehiculo
    {
        public int ColorVehiculoId { get; private set; }
        public string Nombre { get; private set; } = string.Empty;

        private ColorVehiculo() { }

        public static ColorVehiculo Crear(string nombre)
        {
            return new ColorVehiculo { Nombre = nombre };
        }

        public static ColorVehiculo Reconstituir(int colorVehiculoId, string nombre)
        {
            return new ColorVehiculo { ColorVehiculoId = colorVehiculoId, Nombre = nombre };
        }

        public void ActualizarNombre(string nombre)
        {
            Nombre = nombre;
        }
    }
}
