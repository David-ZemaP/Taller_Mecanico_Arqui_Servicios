namespace Taller_Mecanico_Arqui.Application.DTOs.Vehiculos
{
    public class CreateVehiculoDto
    {
        public int ClienteId { get; set; }
        public string Placa { get; set; } = string.Empty;
        public int MarcaId { get; set; }
        public int ModeloId { get; set; }
        public int ColorVehiculoId { get; set; }
        public int Anio { get; set; }
    }

    public class UpdateVehiculoDto
    {
        public int VehiculoId { get; set; }
        public int ClienteId { get; set; }
        public string Placa { get; set; } = string.Empty;
        public int MarcaId { get; set; }
        public int ModeloId { get; set; }
        public int ColorVehiculoId { get; set; }
        public int Anio { get; set; }
    }

    public class VehiculoListDto
    {
        public int VehiculoId { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int Anio { get; set; }
    }

    public class VehiculoDetalleDto
    {
        public int VehiculoId { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public int MarcaId { get; set; }
        public string Marca { get; set; } = string.Empty;
        public int ModeloId { get; set; }
        public string Modelo { get; set; } = string.Empty;
        public int ColorVehiculoId { get; set; }
        public string Color { get; set; } = string.Empty;
        public int Anio { get; set; }
    }
}
