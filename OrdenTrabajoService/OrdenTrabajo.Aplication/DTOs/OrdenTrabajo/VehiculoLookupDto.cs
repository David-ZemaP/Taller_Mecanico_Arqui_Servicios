namespace Taller_Mecanico_Arqui.Application.DTOs.OrdenTrabajo
{
    public class VehiculoLookupDto
    {
        public int VehiculoId { get; set; }
        public string Placa { get; set; } = string.Empty;
        // Legacy compatibility
        public int Id { get => VehiculoId; set => VehiculoId = value; }
        public string Text { get => Placa; set => Placa = value; }
    }
}
