namespace OrdenTrabajoService.Application.DTOs.Vehiculo
{
    public class VehiculoLookupDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    public class VehiculoListDto
    {
        public int VehiculoId { get; set; }
        public string Placa { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public int Anio { get; set; }
        public string MarcaNombre { get; set; } = string.Empty;
        public string ModeloNombre { get; set; } = string.Empty;
        public string ColorNombre { get; set; } = string.Empty;
    }
}

