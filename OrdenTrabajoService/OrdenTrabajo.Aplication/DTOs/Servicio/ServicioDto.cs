namespace OrdenTrabajoService.Application.DTOs.Servicio
{
    public class ServicioDto
    {
        public int ServicioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public double Precio { get; set; }
    }

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
}

