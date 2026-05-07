namespace Taller_Mecanico_Arqui.Domain.Entities;

public class Producto
{
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public double Precio { get; set; }
    public int Stock { get; set; }
    public bool Activo { get; set; } = true;

    public void ReducirStock(int cantidad)
    {
        if (cantidad <= 0) throw new ArgumentException("Cantidad debe ser positiva.");
        if (Stock < cantidad) throw new InvalidOperationException($"Stock insuficiente para '{Nombre}'.");
        Stock -= cantidad;
    }

    public void AumentarStock(int cantidad)
    {
        if (cantidad <= 0) throw new ArgumentException("Cantidad debe ser positiva.");
        Stock += cantidad;
    }
}
