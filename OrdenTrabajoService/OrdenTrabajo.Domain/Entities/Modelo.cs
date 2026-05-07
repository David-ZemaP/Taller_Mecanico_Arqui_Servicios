namespace Taller_Mecanico_Arqui.Domain.Entities
{
    public class Modelo
    {
        public int ModeloId { get; private set; }
        public int MarcaId { get; private set; }
        public string Nombre { get; private set; } = string.Empty;

        public Marca? Marca { get; private set; }

        private Modelo() { }

        public static Modelo Crear(int marcaId, string nombre)
        {
            return new Modelo { MarcaId = marcaId, Nombre = nombre };
        }

        public static Modelo Reconstituir(int modeloId, int marcaId, string nombre)
        {
            return new Modelo { ModeloId = modeloId, MarcaId = marcaId, Nombre = nombre };
        }

        public void ActualizarNombre(string nombre)
        {
            Nombre = nombre;
        }
    }
}
