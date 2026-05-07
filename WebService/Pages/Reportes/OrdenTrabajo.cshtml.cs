using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebService.Adapters;
using WebService.DTOs;

namespace WebService.Pages.Reportes
{
    [Authorize]
    public class OrdenTrabajoModel : PageModel
    {
        private readonly OrdenTrabajoAdapter _adapter;

        public OrdenTrabajoModel(OrdenTrabajoAdapter adapter)
        {
            _adapter = adapter;
        }

        public OrdenTrabajoDetalleDto? Orden { get; private set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Orden = await _adapter.GetOrdenDetalleAsync(id);
            if (Orden is null)
                return NotFound();
            return Page();
        }
    }
}
