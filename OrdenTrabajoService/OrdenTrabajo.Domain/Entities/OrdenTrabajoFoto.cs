namespace OrdenTrabajoService.Domain.Entities
{
    public class OrdenTrabajoFoto
    {
        public int OrdenTrabajoFotoId { get; private set; }
        public int OrdenTrabajoId { get; private set; }
        public byte[] Datos { get; private set; } = Array.Empty<byte>();
        public string ContentType { get; private set; } = string.Empty;
        public string NombreArchivo { get; private set; } = string.Empty;
        public DateTime FechaRegistro { get; private set; }

        private OrdenTrabajoFoto() { }

        public static OrdenTrabajoFoto Crear(int ordenTrabajoId, byte[] datos, string contentType, string nombreArchivo)
        {
            return new OrdenTrabajoFoto
            {
                OrdenTrabajoId = ordenTrabajoId,
                Datos = datos,
                ContentType = contentType,
                NombreArchivo = nombreArchivo,
                FechaRegistro = DateTime.UtcNow
            };
        }
    }
}
