using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebService.Adapters;
using WebService.DTOs;

namespace WebService.Pages.Empleados
{
    [Authorize]
    public class PerfilModel : PageModel
    {
        private readonly EmpleadosAdapter _empleadosAdapter;
        private readonly OrdenTrabajoAdapter _ordenTrabajoAdapter;

        public EmpleadoDto? Empleado { get; set; }
        public List<OrdenTrabajoListDto>? OrdenesTrabajo { get; set; }
        public string? ErrorMessage { get; set; }

        public PerfilModel(EmpleadosAdapter empleadosAdapter, OrdenTrabajoAdapter ordenTrabajoAdapter)
        {
            _empleadosAdapter = empleadosAdapter;
            _ordenTrabajoAdapter = ordenTrabajoAdapter;
        }

        public async Task OnGetAsync(int id)
        {
            // Cargar datos del empleado
            var (ok, empleado, error) = await _empleadosAdapter.GetEmpleadoByIdAsync(id);
            if (!ok || empleado == null)
            {
                ErrorMessage = error ?? "Empleado no encontrado.";
                return;
            }

            Empleado = empleado;

            // Cargar órdenes de trabajo del mecánico
            if (empleado.TipoEmpleado == "Mecanico")
            {
                OrdenesTrabajo = await _ordenTrabajoAdapter.GetOrdenesByMecanicoAsync(empleado.EmpleadoId);
            }
        }
    }
}