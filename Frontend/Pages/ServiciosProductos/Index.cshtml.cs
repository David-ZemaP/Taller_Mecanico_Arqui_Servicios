using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Taller_Mecanico_Arqui.Frontend.Adapters;
using Taller_Mecanico_Arqui.Frontend.Authorization;

namespace Taller_Mecanico_Arqui.Pages.ServiciosProductos
{
    [RequireAccessLevel(NivelAcceso.Completo)]
    public class IndexModel : PageModel
    {
        private readonly IProductoAdapter _productoAdapter;
        private readonly IServicioAdapter _servicioAdapter;

        public IndexModel(IProductoAdapter productoAdapter, IServicioAdapter servicioAdapter)
        {
            _productoAdapter = productoAdapter;
            _servicioAdapter = servicioAdapter;
        }

        public List<ProductoListDto> Productos { get; set; } = new();
        public List<ServicioListDto> Servicios { get; set; } = new();

        [BindProperty]
        public ProductoFormDto ProductoForm { get; set; } = new();

        [BindProperty]
        public ServicioFormDto ServicioForm { get; set; } = new();

        public async Task OnGetAsync()
        {
            Productos = await _productoAdapter.GetAllAsync();
            Servicios = await _servicioAdapter.GetAllAsync();
        }

        public async Task<JsonResult> OnGetProductoAsync(int id)
        {
            var producto = await _productoAdapter.GetByIdAsync(id);
            if (producto == null)
            {
                return new JsonResult(new { error = "Producto no encontrado." }) { StatusCode = 404 };
            }

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
            var servicio = await _servicioAdapter.GetByIdAsync(id);
            if (servicio == null)
            {
                return new JsonResult(new { error = "Servicio no encontrado." }) { StatusCode = 404 };
            }

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
                await OnGetAsync();
                return Page();
            }

            if (ProductoForm.ProductoId == 0)
            {
                var createResult = await _productoAdapter.CreateAsync(new CreateProductoDto
                {
                    Nombre = ProductoForm.Nombre,
                    Precio = ProductoForm.Precio,
                    Stock = ProductoForm.Stock,
                    Descripcion = ProductoForm.Descripcion,
                    StockMinimo = ProductoForm.StockMinimo
                });

                if (!createResult.Success)
                {
                    ModelState.AddModelError(string.Empty, createResult.Error ?? "No se pudo crear el producto.");
                    await OnGetAsync();
                    return Page();
                }

                TempData["SuccessMessage"] = "Producto creado correctamente.";
            }
            else
            {
                var updateResult = await _productoAdapter.UpdateAsync(new UpdateProductoDto
                {
                    ProductoId = ProductoForm.ProductoId,
                    Nombre = ProductoForm.Nombre,
                    Precio = ProductoForm.Precio,
                    Stock = ProductoForm.Stock,
                    Descripcion = ProductoForm.Descripcion,
                    StockMinimo = ProductoForm.StockMinimo
                });

                if (!updateResult.Success)
                {
                    ModelState.AddModelError(string.Empty, updateResult.Error ?? "No se pudo actualizar el producto.");
                    await OnGetAsync();
                    return Page();
                }

                TempData["SuccessMessage"] = "Producto actualizado correctamente.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSaveServicioAsync()
        {
            ModelState.Clear();
            if (!TryValidateModel(ServicioForm, nameof(ServicioForm)))
            {
                await OnGetAsync();
                return Page();
            }

            if (ServicioForm.ServicioId == 0)
            {
                var createResult = await _servicioAdapter.CreateAsync(new CreateServicioDto
                {
                    Nombre = ServicioForm.Nombre,
                    Precio = ServicioForm.Precio,
                    Descripcion = ServicioForm.Descripcion
                });

                if (!createResult.Success)
                {
                    ModelState.AddModelError(string.Empty, createResult.Error ?? "No se pudo crear el servicio.");
                    await OnGetAsync();
                    return Page();
                }

                TempData["SuccessMessage"] = "Servicio creado correctamente.";
            }
            else
            {
                var updateResult = await _servicioAdapter.UpdateAsync(new UpdateServicioDto
                {
                    ServicioId = ServicioForm.ServicioId,
                    Nombre = ServicioForm.Nombre,
                    Precio = ServicioForm.Precio,
                    Descripcion = ServicioForm.Descripcion
                });

                if (!updateResult.Success)
                {
                    ModelState.AddModelError(string.Empty, updateResult.Error ?? "No se pudo actualizar el servicio.");
                    await OnGetAsync();
                    return Page();
                }

                TempData["SuccessMessage"] = "Servicio actualizado correctamente.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteProductoAsync(int id)
        {
            await _productoAdapter.DeleteAsync(id);
            TempData["SuccessMessage"] = "Producto eliminado correctamente.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteServicioAsync(int id)
        {
            await _servicioAdapter.DeleteAsync(id);
            TempData["SuccessMessage"] = "Servicio eliminado correctamente.";
            return RedirectToPage();
        }
    }

    public class ProductoFormDto
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? Descripcion { get; set; }
        public int StockMinimo { get; set; }
    }

    public class ServicioFormDto
    {
        public int ServicioId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public string? Descripcion { get; set; }
    }
}
