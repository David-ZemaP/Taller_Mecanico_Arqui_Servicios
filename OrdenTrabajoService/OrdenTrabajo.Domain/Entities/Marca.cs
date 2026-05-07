namespace Taller_Mecanico_Arqui.Domain.Entities
{
    public class Marca
    {
        public int MarcaId { get; private set; }
        public string Nombre { get; private set; } = string.Empty;

        private readonly List<Modelo> _modelos = new();
        public IReadOnlyCollection<Modelo> Modelos => _modelos.AsReadOnly();

        private Marca() { }

        public static Marca Crear(string nombre)
        {
            return new Marca { Nombre = nombre };
        }

        public static Marca Reconstituir(int marcaId, string nombre)
        {
            return new Marca { MarcaId = marcaId, Nombre = nombre };
        }

        public void ActualizarNombre(string nombre)
        {
            Nombre = nombre;
        }
    }
}
