using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Taller_Mecanico_Arqui.Application.DTOs.OrdenTrabajo;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Domain.Entities;

namespace Taller_Mecanico_Arqui.Pages.Clientes
{
    public class PerfilModel : PageModel
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IOrdenTrabajoRepository _ordenTrabajoRepository;

        public PerfilModel(
            IClienteRepository clienteRepository,
            IOrdenTrabajoRepository ordenTrabajoRepository)
        {
            _clienteRepository = clienteRepository;
            _ordenTrabajoRepository = ordenTrabajoRepository;
        }

        public Cliente? Cliente { get; set; }
        public IEnumerable<Domain.Entities.Vehiculo> Vehiculos { get; set; } = new List<Domain.Entities.Vehiculo>();
        public IList<OrdenTrabajoListDto> Ordenes { get; set; } = new List<OrdenTrabajoListDto>();

        public async Task OnGetAsync()
        {
            var clienteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(clienteIdClaim) || !int.TryParse(clienteIdClaim, out var clienteId))
            {
                RedirectToPage("/Login");
                return;
            }

            var clienteResult = await _clienteRepository.GetByIdAsync(clienteId);
            if (clienteResult.IsFailure || clienteResult.Value == null)
            {
                RedirectToPage("/Login");
                return;
            }

            Cliente = clienteResult.Value;
            Vehiculos = Cliente.Vehiculos;

            var todasOrdenes = await _ordenTrabajoRepository.GetAllAsync();
            var vehiculosIds = Cliente.Vehiculos.Select(v => v.VehiculoId).ToHashSet();

            Ordenes = todasOrdenes
                .Where(o => vehiculosIds.Contains(o.VehiculoId))
                .Select(o => new OrdenTrabajoListDto
                {
                    OrdenTrabajoId = o.OrdenTrabajoId,
                    VehiculoId = o.VehiculoId,
                    VehiculoPlaca = Cliente.Vehiculos.FirstOrDefault(v => v.VehiculoId == o.VehiculoId)?.Placa ?? "",
                    FechaIngreso = o.FechaIngreso,
                    FechaEntrega = o.FechaEntrega,
                    EstadoTrabajo = o.EstadoTrabajo.ToString(),
                    EstadoPago = o.EstadoPago.ToString(),
                    EstadoVehiculo = o.EstadoVehiculo.ToString(),
                    Total = o.Total
                })
                .OrderByDescending(o => o.FechaIngreso)
                .ToList();
        }
    }
}