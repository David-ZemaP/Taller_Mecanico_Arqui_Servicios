namespace OrdenTrabajoService.Domain.Entities
{
    public class Cliente
    {
        public int ClienteId { get; private set; }
        public string Nombre { get; private set; } = string.Empty;
        public string PrimerApellido { get; private set; } = string.Empty;
        public string? SegundoApellido { get; private set; }
        public int Ci { get; private set; }
        public string? CiComplemento { get; private set; }
        public int Telefono { get; private set; }
        public string Email { get; private set; } = string.Empty;
        public bool IsDeleted { get; private set; }
        public DateTime FechaRegistro { get; private set; }

        private Cliente() { }

        public static Cliente Crear(string nombre, string primerApellido, string? segundoApellido, int ci, string? ciComplemento, int telefono, string email)
        {
            return new Cliente
            {
                Nombre = nombre,
                PrimerApellido = primerApellido,
                SegundoApellido = string.IsNullOrWhiteSpace(segundoApellido) ? null : segundoApellido,
                Ci = ci,
                CiComplemento = string.IsNullOrWhiteSpace(ciComplemento) ? null : ciComplemento,
                Telefono = telefono,
                Email = email,
                FechaRegistro = DateTime.UtcNow
            };
        }

        public static Cliente Reconstituir(int clienteId, string nombre, string primerApellido, string? segundoApellido, int ci, string? ciComplemento, int telefono, string email, bool isDeleted, DateTime fechaRegistro)
        {
            return new Cliente
            {
                ClienteId = clienteId,
                Nombre = nombre,
                PrimerApellido = primerApellido,
                SegundoApellido = segundoApellido,
                Ci = ci,
                CiComplemento = ciComplemento,
                Telefono = telefono,
                Email = email,
                IsDeleted = isDeleted,
                FechaRegistro = fechaRegistro
            };
        }

        public void Actualizar(string nombre, string primerApellido, string? segundoApellido, int ci, string? ciComplemento, int telefono, string email)
        {
            Nombre = nombre;
            PrimerApellido = primerApellido;
            SegundoApellido = string.IsNullOrWhiteSpace(segundoApellido) ? null : segundoApellido;
            Ci = ci;
            CiComplemento = string.IsNullOrWhiteSpace(ciComplemento) ? null : ciComplemento;
            Telefono = telefono;
            Email = email;
        }
    }
}

