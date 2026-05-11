namespace OrdenTrabajoService.Domain.Entities
{
    public class Producto
    {
        public int ProductoId { get; private set; }
        public string Nombre { get; private set; } = string.Empty;
        public double Precio { get; private set; }
        public int Stock { get; private set; }
        public bool IsDeleted { get; private set; }

        private Producto() { }

        public static Producto Crear(string nombre, double precio, int stock)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del producto es obligatorio.");
            if (precio < 0)
                throw new ArgumentException("El precio no puede ser negativo.");
            if (stock < 0)
                throw new ArgumentException("El stock no puede ser negativo.");
            return new Producto { Nombre = nombre.Trim(), Precio = precio, Stock = stock };
        }

        public static Producto Reconstituir(int productoId, string nombre, double precio, int stock, bool isDeleted = false)
            => new() { ProductoId = productoId, Nombre = nombre, Precio = precio, Stock = stock, IsDeleted = isDeleted };

        public void ActualizarDatos(string nombre, double precio, int stock)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del producto es obligatorio.");
            if (precio < 0)
                throw new ArgumentException("El precio no puede ser negativo.");
            if (stock < 0)
                throw new ArgumentException("El stock no puede ser negativo.");
            Nombre = nombre.Trim();
            Precio = precio;
            Stock = stock;
        }

        public void ReducirStock(int cantidad)
        {
            if (Stock < cantidad)
                throw new ArgumentException($"Stock insuficiente para '{Nombre}'. Stock actual: {Stock}.");
            Stock -= cantidad;
        }

        public void AumentarStock(int cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad a reponer debe ser mayor a 0.");
            Stock += cantidad;
        }
    }
}

