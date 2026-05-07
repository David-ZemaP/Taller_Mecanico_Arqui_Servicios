namespace Taller_Mecanico_Arqui.Domain.Entities
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

        // Auditoría
        public string? CreadoPor { get; private set; }
        public string? ActualizadoPor { get; private set; }
        public string? EliminadoPor { get; private set; }

        private readonly List<OrdenTrabajo> _ordenes = new();
        public IReadOnlyCollection<OrdenTrabajo> Ordenes => _ordenes.AsReadOnly();

        public Cliente? Cliente { get; private set; }
        public Marca? Marca { get; private set; }
        public Modelo? Modelo { get; private set; }
        public ColorVehiculo? ColorVehiculo { get; private set; }

        private Vehiculo() { }

        public static Vehiculo Crear(
            int clienteId,
            string placa,
            int marcaId,
            int modeloId,
            int colorVehiculoId,
            int anio)
        {
            return new Vehiculo
            {
                ClienteId = clienteId,
                Placa = placa,
                MarcaId = marcaId,
                ModeloId = modeloId,
                ColorVehiculoId = colorVehiculoId,
                Anio = anio
            };
        }

        public static Vehiculo Reconstituir(
            int vehiculoId,
            int clienteId,
            string placa,
            int marcaId,
            int modeloId,
            int colorVehiculoId,
            int anio,
            DateTime? fechaActualizacion = null,
            bool isDeleted = false)
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
                IsDeleted = isDeleted,
                FechaActualizacion = fechaActualizacion
            };
        }

        public void SetNavigationProperties(Cliente? cliente, Marca? marca, Modelo? modelo, ColorVehiculo? color)
        {
            Cliente = cliente;
            Marca = marca;
            Modelo = modelo;
            ColorVehiculo = color;
        }

        public void ActualizarDatos(
            string placa,
            int marcaId,
            int modeloId,
            int colorVehiculoId,
            int anio)
        {
            Placa = placa;
            MarcaId = marcaId;
            ModeloId = modeloId;
            ColorVehiculoId = colorVehiculoId;
            Anio = anio;
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
