namespace OrdenTrabajoService.Domain.Entities
{
    public class Modelo
    {
        public int ModeloId { get; private set; }
        public int MarcaId { get; private set; }
        public string Nombre { get; private set; } = string.Empty;

        private Modelo() { }

        public static Modelo Crear(int marcaId, string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del modelo es obligatorio.");
            return new Modelo { MarcaId = marcaId, Nombre = nombre.Trim() };
        }

        public static Modelo Reconstituir(int modeloId, int marcaId, string nombre)
            => new() { ModeloId = modeloId, MarcaId = marcaId, Nombre = nombre };

        public void ActualizarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del modelo es obligatorio.");
            Nombre = nombre.Trim();
        }
    }
}
