namespace TallerMecanico.Core.Models;

public sealed class Vehiculo
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public string Marca { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public int Anio { get; set; }

    public string Placa { get; set; } = string.Empty;

    public string? Color { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedByUserId { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedByUserId { get; set; }
}