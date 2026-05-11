namespace OrdenTrabajoService.Domain.Entities
{
    public class Vehiculo
    {
        public int VehiculoId { get; private set; }
        public int ClienteId { get; private set; }
        public string Placa { get; private set; } = string.Empty;
        public int MarcaId { get; private set; }
        public int ModeloId { get; private set; }
        public int ColorVehiculoId { get; private set; }
        public int Anio { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? FechaActualizacion { get; private set; }

        // Propiedades de visualización cargadas mediante JOIN
        public string ClienteNombre { get; private set; } = string.Empty;
        public string ClienteCi { get; private set; } = string.Empty;
        public string MarcaNombre { get; private set; } = string.Empty;
        public string ModeloNombre { get; private set; } = string.Empty;
        public string ColorNombre { get; private set; } = string.Empty;

        private Vehiculo() { }

        public static Vehiculo Reconstituir(
            int vehiculoId, int clienteId, string placa,
            int marcaId, int modeloId, int colorVehiculoId,
            int anio, DateTime? fechaActualizacion, bool isDeleted)
        {
            return new Vehiculo
            {
                VehiculoId = vehiculoId,
                ClienteId = clienteId,
                Placa = placa,
                MarcaId = marcaId,
                ModeloId = modeloId,
                ColorVehiculoId = colorVehiculoId,
                Anio = anio,
                FechaActualizacion = fechaActualizacion,
                IsDeleted = isDeleted
            };
        }

        public void SetDisplayInfo(string clienteNombre, string clienteCi, string marcaNombre, string modeloNombre, string colorNombre)
        {
            ClienteNombre = clienteNombre;
            ClienteCi = clienteCi;
            MarcaNombre = marcaNombre;
            ModeloNombre = modeloNombre;
            ColorNombre = colorNombre;
        }
    }
}

