using System;
using System.Collections.Generic;
using System.Linq;
using Taller_Mecanico_Arqui.Domain.Enums;

namespace Taller_Mecanico_Arqui.Domain.Entities
{
    public class OrdenTrabajo
    {
        public int OrdenTrabajoId { get; private set; }
        public int VehiculoId { get; private set; }
        public DateTime FechaIngreso { get; private set; }
        public DateTime? FechaEntrega { get; private set; }
        public EstadoTrabajo EstadoTrabajo { get; private set; }
        public EstadoPago EstadoPago { get; private set; }
        public string EstadoVehiculo { get; private set; } = string.Empty;
        public double Total { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime? FechaActualizacion { get; private set; }

        public Vehiculo? Vehiculo { get; private set; }

        private readonly List<OrdenTrabajoFoto> _fotosVehiculo = new();
        public IReadOnlyCollection<OrdenTrabajoFoto> FotosVehiculo => _fotosVehiculo.AsReadOnly();

        private readonly List<OrdenTrabajoMecanico> _mecanicosAsignados = new();
        public IReadOnlyCollection<OrdenTrabajoMecanico> MecanicosAsignados => _mecanicosAsignados.AsReadOnly();

        // --- NUEVAS LISTAS PARA PRODUCTOS Y SERVICIOS ---
        private readonly List<OrdenTrabajoProducto> _productosUsados = new();
        public IReadOnlyCollection<OrdenTrabajoProducto> ProductosUsados => _productosUsados.AsReadOnly();

        private readonly List<OrdenTrabajoServicio> _serviciosRealizados = new();
        public IReadOnlyCollection<OrdenTrabajoServicio> ServiciosRealizados => _serviciosRealizados.AsReadOnly();

        private OrdenTrabajo() { }

        public static OrdenTrabajo Crear(
            int vehiculoId,
            DateTime fechaIngreso,
            string estadoVehiculo,
            EstadoTrabajo estadoTrabajo = EstadoTrabajo.Recibido,
            EstadoPago estadoPago = EstadoPago.Pendiente)
        {
            return new OrdenTrabajo
            {
                VehiculoId = vehiculoId,
                FechaIngreso = fechaIngreso,
                EstadoVehiculo = estadoVehiculo,
                EstadoTrabajo = estadoTrabajo,
                EstadoPago = estadoPago,
                Total = 0
            };
        }

        public static OrdenTrabajo Reconstituir(
            int ordenTrabajoId,
            int vehiculoId,
            DateTime fechaIngreso,
            DateTime? fechaEntrega,
            EstadoTrabajo estadoTrabajo,
            EstadoPago estadoPago,
            string estadoVehiculo,
            double total,
            bool isDeleted,
            DateTime? fechaActualizacion,
            Vehiculo? vehiculo = null)
        {
            return new OrdenTrabajo
            {
                OrdenTrabajoId = ordenTrabajoId,
                VehiculoId = vehiculoId,
                FechaIngreso = fechaIngreso,
                FechaEntrega = fechaEntrega,
                EstadoTrabajo = estadoTrabajo,
                EstadoPago = estadoPago,
                EstadoVehiculo = estadoVehiculo,
                Total = total,
                IsDeleted = isDeleted,
                FechaActualizacion = fechaActualizacion,
                Vehiculo = vehiculo
            };
        }

        // --- NUEVOS M�TODOS PARA AGREGAR DETALLES ---
        public void AgregarProducto(int productoId, int cantidad, double precioUnitario)
        {
            var subtotal = cantidad * precioUnitario;
            _productosUsados.Add(new OrdenTrabajoProducto(this.OrdenTrabajoId, productoId, cantidad, precioUnitario, subtotal));
            RecalcularTotal();
        }

        public void AgregarServicio(int servicioId, int cantidad, double precioUnitario)
        {
            var subtotal = cantidad * precioUnitario;
            _serviciosRealizados.Add(new OrdenTrabajoServicio(this.OrdenTrabajoId, servicioId, cantidad, precioUnitario, subtotal));
            RecalcularTotal();
        }

        // M�todo privado para encapsular la matem�tica de la factura
        private void RecalcularTotal()
        {
            double totalProductos = _productosUsados.Sum(p => p.Subtotal);
            double totalServicios = _serviciosRealizados.Sum(s => s.Subtotal);

            ActualizarTotal(totalProductos + totalServicios);
        }

        public void ActualizarEstadoTrabajo(EstadoTrabajo estado)
        {
            EstadoTrabajo = estado;
            FechaActualizacion = DateTime.UtcNow;

            if (estado == EstadoTrabajo.Entregado)
            {
                FechaEntrega = DateTime.UtcNow;
            }
        }

        public void ActualizarEstadoPago(EstadoPago estado)
        {
            EstadoPago = estado;
            FechaActualizacion = DateTime.UtcNow;
        }

        public void ActualizarTotal(double total)
        {
            if (total < 0)
                throw new ArgumentException("El total no puede ser negativo");

            Total = total;
            FechaActualizacion = DateTime.UtcNow;
        }

        public void AgregarFoto(OrdenTrabajoFoto foto)
        {
            _fotosVehiculo.Add(foto);
        }

        public void AsignarMecanico(OrdenTrabajoMecanico mecanicoAsignado)
        {
            if (!_mecanicosAsignados.Any(m => m.MecanicoId == mecanicoAsignado.MecanicoId))
            {
                _mecanicosAsignados.Add(mecanicoAsignado);
            }
        }

        public void CargarProducto(OrdenTrabajoProducto producto)
        {
            _productosUsados.Add(producto);
        }

        public void CargarServicio(OrdenTrabajoServicio servicio)
        {
            _serviciosRealizados.Add(servicio);
        }

        public void MarcarEliminado()
        {
            IsDeleted = true;
            FechaActualizacion = DateTime.UtcNow;
        }
    }
}