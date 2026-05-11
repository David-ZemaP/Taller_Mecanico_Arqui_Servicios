using Taller_Mecanico_Users.Domain.ValueObjects;
using Taller_Mecanico_Users.Domain.Enums;

namespace Taller_Mecanico_Users.Application.DTOs.Users
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
}

