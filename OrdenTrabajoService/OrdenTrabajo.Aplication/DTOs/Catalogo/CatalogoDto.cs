namespace OrdenTrabajoService.Application.DTOs.Catalogo
{
    public class MarcaDto
    {
        public int MarcaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class CreateMarcaDto
    {
        public string Nombre { get; set; } = string.Empty;
    }

    public class ModeloDto
    {
        public int ModeloId { get; set; }
        public int MarcaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class CreateModeloDto
    {
        public int MarcaId { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class ColorVehiculoDto
    {
        public int ColorVehiculoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class CreateColorVehiculoDto
    {
        public string Nombre { get; set; } = string.Empty;
    }
}

