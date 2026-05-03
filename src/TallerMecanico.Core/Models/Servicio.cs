namespace TallerMecanico.Core.Models;

public sealed class Servicio
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public bool IsDeleted { get; set; }
}