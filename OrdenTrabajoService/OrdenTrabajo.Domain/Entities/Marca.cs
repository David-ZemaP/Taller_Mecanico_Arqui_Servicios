namespace OrdenTrabajoService.Domain.Entities
{
    public class Marca
    {
        public int MarcaId { get; private set; }
        public string Nombre { get; private set; } = string.Empty;

        private Marca() { }

        public static Marca Crear(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la marca es obligatorio.");
            return new Marca { Nombre = nombre.Trim() };
        }

        public static Marca Reconstituir(int marcaId, string nombre)
            => new() { MarcaId = marcaId, Nombre = nombre };

        public void ActualizarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de la marca es obligatorio.");
            Nombre = nombre.Trim();
        }
    }
}

