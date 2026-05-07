using System.ComponentModel.DataAnnotations;

namespace Taller_Mecanico_Arqui.Application.DTOs.OrdenTrabajo
{
    public class OrdenTrabajoFormDto
    {
        public int OrdenTrabajoId { get; set; }

        public int ClienteId { get; set; }

        public int VehiculoId { get; set; }

        [Required(ErrorMessage = "La fecha de ingreso es obligatoria.")]
        public DateTime FechaIngreso { get; set; }

        public DateTime? FechaEntrega { get; set; }

        [Required(ErrorMessage = "El estado del trabajo es obligatorio.")]
        public string EstadoTrabajo { get; set; } = "Recibido";

        [Required(ErrorMessage = "El estado de pago es obligatorio.")]
        public string EstadoPago { get; set; } = "Pendiente";

        [Required(ErrorMessage = "El estado del vehículo es obligatorio.")]
        [StringLength(500, ErrorMessage = "El estado del vehículo no puede tener más de 500 caracteres.")]
        public string EstadoVehiculo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El total es obligatorio.")]
        [Range(0, double.MaxValue, ErrorMessage = "El total no puede ser negativo.")]
        public double Total { get; set; }

        public string ProductosJson { get; set; } = "[]";
        public string ServiciosJson { get; set; } = "[]";
    }
}
