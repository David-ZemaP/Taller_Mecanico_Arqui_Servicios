namespace Taller_Mecanico_Arqui.Application.DTOs.Servicios
{
    public class CreateServicioDto
    {
        public string Nombre { get; set; } = string.Empty;
        public double Precio { get; set; }
    }

    public class UpdateServicioDto
    {
        public int ServicioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public double Precio { get; set; }
    }

    public class ServicioListDto
    {
        public int ServicioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public double Precio { get; set; }
    }
}
