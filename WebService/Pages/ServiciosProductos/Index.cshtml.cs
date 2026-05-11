using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebService.Adapters;
using WebService.DTOs;

namespace WebService.Pages.ServiciosProductos
{
    public class IndexModel : PageModel
    {
        private readonly OrdenTrabajoAdapter _adapter;

        public IndexModel(OrdenTrabajoAdapter adapter)
        {
            _adapter = adapter;
        }

        public IList<ProductoDto> Productos { get; set; } = new List<ProductoDto>();
        public IList<ServicioDto> Servicios { get; set; } = new List<ServicioDto>();

        [BindProperty]
        public ProductoFormDto ProductoForm { get; set; } = new();

        [BindProperty]
        public ServicioFormDto ServicioForm { get; set; } = new();

        public async Task OnGetAsync()
        {
            await CargarCatalogosAsync();
        }

        public async Task<JsonResult> OnGetProductoAsync(int id)
        {
            var producto = await _adapter.GetProductoAsync(id);
            if (producto is null)
                return new JsonResult(new { error = "Producto no encontrado." }) { StatusCode = 404 };
            return new JsonResult(new
            {
                productoId = producto.ProductoId,
                nombre = producto.Nombre,
                precio = producto.Precio,
                stock = producto.Stock
            });
        }

        public async Task<JsonResult> OnGetServicioAsync(int id)
        {
            var servicio = await _adapter.GetServicioAsync(id);
            if (servicio is null)
                return new JsonResult(new { error = "Servicio no encontrado." }) { StatusCode = 404 };
            return new JsonResult(new
            {
                servicioId = servicio.ServicioId,
                nombre = servicio.Nombre,
                precio = servicio.Precio
            });
        }

        public async Task<IActionResult> OnPostSaveProductoAsync()
        {
            ModelState.Clear();
            if (!TryValidateModel(ProductoForm, nameof(ProductoForm)))
            {
                await CargarCatalogosAsync();
                return Page();
            }

            var (ok, error) = await _adapter.SaveProductoAsync(ProductoForm);
            if (!ok)
            {
                TempData["ErrorMessage"] = error ?? "No se pudo guardar el producto.";
                await CargarCatalogosAsync();
                return Page();
            }

            TempData["SuccessMessage"] = ProductoForm.ProductoId == 0
                ? "Producto creado correctamente."
                : "Producto actualizado correctamente.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSaveServicioAsync()
        {
            ModelState.Clear();
            if (!TryValidateModel(ServicioForm, nameof(ServicioForm)))
            {
                await CargarCatalogosAsync();
                return Page();
            }

            var (ok, error) = await _adapter.SaveServicioAsync(ServicioForm);
            if (!ok)
            {
                TempData["ErrorMessage"] = error ?? "No se pudo guardar el servicio.";
                await CargarCatalogosAsync();
                return Page();
            }

            TempData["SuccessMessage"] = ServicioForm.ServicioId == 0
                ? "Servicio creado correctamente."
                : "Servicio actualizado correctamente.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteProductoAsync(int id)
        {
            await _adapter.DeleteProductoAsync(id);
            TempData["SuccessMessage"] = "Producto eliminado correctamente.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteServicioAsync(int id)
        {
            await _adapter.DeleteServicioAsync(id);
            TempData["SuccessMessage"] = "Servicio eliminado correctamente.";
            return RedirectToPage();
        }

        private async Task CargarCatalogosAsync()
        {
            Productos = await _adapter.GetAllProductosAsync();
            Servicios = await _adapter.GetAllServiciosAsync();
        }
    }
}
