using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Taller_Mecanico_Arqui.Frontend.Adapters;
using Taller_Mecanico_Arqui.Frontend.DTOs.OrdenTrabajo;
using System.Linq;

namespace Taller_Mecanico_Arqui.Pages.Clientes
{
    public class PerfilModel : PageModel
    {
        private readonly IClienteAdapter _clienteAdapter;
        private readonly IOrdenTrabajoAdapter _ordenTrabajoAdapter;

        public PerfilModel(
            IClienteAdapter clienteAdapter,
            IOrdenTrabajoAdapter ordenTrabajoAdapter)
        {
            _clienteAdapter = clienteAdapter;
            _ordenTrabajoAdapter = ordenTrabajoAdapter;
        }

        public ClienteDetalleDto? Cliente { get; set; }
        public List<OrdenTrabajoListDto> Ordenes { get; set; } = new();

        public async Task OnGetAsync()
        {
            var clienteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(clienteIdClaim) || !int.TryParse(clienteIdClaim, out var clienteId))
            {
                RedirectToPage("/Login");
                return;
            }

            Cliente = await _clienteAdapter.GetByIdAsync(clienteId);
            if (Cliente == null)
            {
                RedirectToPage("/Login");
                return;
            }

            var todasOrdenes = await _ordenTrabajoAdapter.GetAllAsync();
            var vehiculosIds = Cliente.Vehiculos.Select(v => v.VehiculoId).ToHashSet();

            Ordenes = todasOrdenes
                .Where(o => vehiculosIds.Contains(o.VehiculoId))
                .OrderByDescending(o => o.FechaIngreso)
                .ToList();
        }
    }
}