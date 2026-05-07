using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebService.Adapters;
using WebService.DTOs;

namespace WebService.Pages.Empleados
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly EmpleadosAdapter _empleadosAdapter;

        public IEnumerable<EmpleadoDto> Empleados { get; set; } = [];
        public string? ErrorMessage { get; set; }

        public IndexModel(EmpleadosAdapter empleadosAdapter)
        {
            _empleadosAdapter = empleadosAdapter;
        }

        public async Task OnGetAsync()
        {
            var (ok, empleados, error) = await _empleadosAdapter.GetAllEmpleadosAsync();
            if (ok && empleados != null)
            {
                Empleados = empleados;
            }
            else
            {
                ErrorMessage = error ?? "No se pudieron cargar los empleados.";
                Empleados = [];
            }
        }
    }
}
