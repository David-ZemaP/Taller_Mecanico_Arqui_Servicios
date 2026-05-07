using Taller_Mecanico_Arqui.Domain.Enums;
using Taller_Mecanico_Arqui.Domain.ValueObjects;

namespace Taller_Mecanico_Arqui.Domain.Entities
{
    public class Cliente : Persona
    {
        public int ClienteId { get; private set; }
        public int? UsuarioLoginId { get; private set; }
        public DateTime FechaRegistro { get; private set; }
        public TipoCliente TipoCliente { get; private set; }

        private readonly List<Vehiculo> _vehiculos = new();
        public IReadOnlyCollection<Vehiculo> Vehiculos => _vehiculos.AsReadOnly();

        private Cliente() { }

        public static Cliente Crear(
            NombreCompleto nombreCompleto,
            DocumentoIdentidad ci,
            int telefono,
            string email,
            TipoCliente tipoCliente = TipoCliente.Regular)
        {
            return new Cliente
            {
                NombreCompleto = nombreCompleto,
                Ci = ci,
                Telefono = telefono,
                Email = email,
                FechaRegistro = DateTime.UtcNow,
                TipoCliente = tipoCliente
            };
        }

        public static Cliente Reconstituir(
            int clienteId,
            NombreCompleto nombreCompleto,
            DocumentoIdentidad ci,
            int telefono,
            string email,
            DateTime fechaRegistro,
            TipoCliente tipoCliente,
            int? usuarioLoginId = null,
            DateTime? fechaActualizacion = null,
            bool isDeleted = false)
        {
            return new Cliente
            {
                ClienteId = clienteId,
                NombreCompleto = nombreCompleto,
                Ci = ci,
                Telefono = telefono,
                Email = email,
                FechaRegistro = fechaRegistro,
                TipoCliente = tipoCliente,
                UsuarioLoginId = usuarioLoginId,
                FechaActualizacion = fechaActualizacion,
                IsDeleted = isDeleted
            };
        }

        public void AsignarUsuarioLogin(int usuarioLoginId)
        {
            UsuarioLoginId = usuarioLoginId;
            FechaActualizacion = DateTime.UtcNow;
        }

        public void ActualizarDatos(
            NombreCompleto nombreCompleto,
            DocumentoIdentidad ci,
            int telefono,
            string email,
            TipoCliente tipoCliente)
        {
            NombreCompleto = nombreCompleto;
            Ci = ci;
            Telefono = telefono;
            Email = email;
            TipoCliente = tipoCliente;
            FechaActualizacion = DateTime.UtcNow;
        }

        public void AgregarVehiculo(Vehiculo vehiculo)
        {
            if (!_vehiculos.Contains(vehiculo))
            {
                _vehiculos.Add(vehiculo);
            }
        }

        public bool TieneVehiculos() => _vehiculos.Count > 0;
    }
}
