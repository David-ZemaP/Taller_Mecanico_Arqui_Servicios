using Taller_Mecanico_Arqui.Domain.Enums;
using Taller_Mecanico_Arqui.Application.DTOs.OrdenTrabajo;

namespace Taller_Mecanico_Arqui.Application.DTOs.Clientes
{
    public class CreateClienteDto
    {
        public string Nombres { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public int CiNumero { get; set; }
        public string? CiComplemento { get; set; }
        public int Telefono { get; set; }
        public string Email { get; set; } = string.Empty;
        public string TipoCliente { get; set; } = "Regular";
    }

    public class UpdateClienteDto
    {
        public int ClienteId { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public int CiNumero { get; set; }
        public string? CiComplemento { get; set; }
        public int Telefono { get; set; }
        public string Email { get; set; } = string.Empty;
        public string TipoCliente { get; set; } = "Regular";
    }

    public class ClienteListDto
    {
        public int ClienteId { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Ci { get; set; } = string.Empty;
        public int Telefono { get; set; }
        public string? Email { get; set; }
        public string TipoCliente { get; set; } = string.Empty;
        public int VehiculoCount { get; set; }
    }

    public class ClienteDetalleDto
    {
        public int ClienteId { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public string Ci { get; set; } = string.Empty;
        public int Telefono { get; set; }
        public string? Email { get; set; }
        public string TipoCliente { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public int? UsuarioLoginId { get; set; }
        public List<VehiculoLookupDto> Vehiculos { get; set; } = new();
    }
}
