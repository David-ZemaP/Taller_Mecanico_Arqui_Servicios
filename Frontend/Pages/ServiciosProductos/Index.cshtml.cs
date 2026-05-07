using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Taller_Mecanico_Arqui.Infrastructure.Authorization;
using Taller_Mecanico_Arqui.Domain.Enums;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Application.DTOs.Productos;
using Taller_Mecanico_Arqui.Application.DTOs.Servicios;

namespace Taller_Mecanico_Arqui.Pages.ServiciosProductos
{
    [RequireAccessLevel(NivelAcceso.Completo)]
    public class IndexModel : PageModel
    {
        private readonly IRepository<Producto> _productoRepository;
        private readonly IRepository<Servicio> _servicioRepository;

        public IndexModel(IRepository<Producto> productoRepository, IRepository<Servicio> servicioRepository)
        {
            _productoRepository = productoRepository;
            _servicioRepository = servicioRepository;
        }

        public IList<Producto> Productos { get; set; } = new List<Producto>();
        public IList<Servicio> Servicios { get; set; } = new List<Servicio>();

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
            var result = await _productoRepository.GetByIdAsync(id);
            if (result.IsFailure)
            {
                return new JsonResult(new { error = result.ErrorMessage ?? "Error al cargar producto." }) { StatusCode = 500 };
            }

            if (result.Value == null)
            {
                return new JsonResult(new { error = "Producto no encontrado." }) { StatusCode = 404 };
            }

            var producto = result.Value;
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
            var result = await _servicioRepository.GetByIdAsync(id);
            if (result.IsFailure)
            {
                return new JsonResult(new { error = result.ErrorMessage ?? "Error al cargar servicio." }) { StatusCode = 500 };
            }

            if (result.Value == null)
            {
                return new JsonResult(new { error = "Servicio no encontrado." }) { StatusCode = 404 };
            }

            var servicio = result.Value;
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

            try
            {
                if (ProductoForm.ProductoId == 0)
                {
                    var nuevo = Producto.Crear(ProductoForm.Nombre, ProductoForm.Precio, ProductoForm.Stock);
                    var addResult = await _productoRepository.AddAsync(nuevo);
                    if (addResult.IsFailure)
                    {
                        TempData["ErrorMessage"] = addResult.ErrorMessage ?? "No se pudo crear el producto.";
                        await CargarCatalogosAsync();
                        return Page();
                    }

                    TempData["SuccessMessage"] = "Producto creado correctamente.";
                    return RedirectToPage();
                }

                var getResult = await _productoRepository.GetByIdAsync(ProductoForm.ProductoId);
                if (getResult.IsFailure)
                {
                    TempData["ErrorMessage"] = getResult.ErrorMessage ?? "Error al consultar producto.";
                    await CargarCatalogosAsync();
                    return Page();
                }

                if (getResult.Value == null)
                {
                    TempData["ErrorMessage"] = "Producto no encontrado.";
                    return RedirectToPage();
                }

                var existente = getResult.Value;
                existente.ActualizarDatos(ProductoForm.Nombre, ProductoForm.Precio, ProductoForm.Stock);

                var updateResult = await _productoRepository.UpdateAsync(existente);

                if (updateResult.IsFailure)
                {
                    TempData["ErrorMessage"] = updateResult.ErrorMessage ?? "No se pudo actualizar el producto.";
                    await CargarCatalogosAsync();
                    return Page();
                }

                TempData["SuccessMessage"] = "Producto actualizado correctamente.";
                return RedirectToPage();
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await CargarCatalogosAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostSaveServicioAsync()
        {
            ModelState.Clear();
            if (!TryValidateModel(ServicioForm, nameof(ServicioForm)))
            {
                await CargarCatalogosAsync();
                return Page();
            }

            try
            {
                if (ServicioForm.ServicioId == 0)
                {
                    var nuevo = Servicio.Crear(ServicioForm.Nombre, ServicioForm.Precio);
                    var addResult = await _servicioRepository.AddAsync(nuevo);
                    if (addResult.IsFailure)
                    {
                        TempData["ErrorMessage"] = addResult.ErrorMessage ?? "No se pudo crear el servicio.";
                        await CargarCatalogosAsync();
                        return Page();
                    }

                    TempData["SuccessMessage"] = "Servicio creado correctamente.";
                    return RedirectToPage();
                }

                var getResult = await _servicioRepository.GetByIdAsync(ServicioForm.ServicioId);
                if (getResult.IsFailure)
                {
                    TempData["ErrorMessage"] = getResult.ErrorMessage ?? "Error al consultar servicio.";
                    await CargarCatalogosAsync();
                    return Page();
                }

                if (getResult.Value == null)
                {
                    TempData["ErrorMessage"] = "Servicio no encontrado.";
                    return RedirectToPage();
                }

                var existente = getResult.Value;
                existente.ActualizarDatos(ServicioForm.Nombre, ServicioForm.Precio);

                var updateResult = await _servicioRepository.UpdateAsync(existente);

                if (updateResult.IsFailure)
                {
                    TempData["ErrorMessage"] = updateResult.ErrorMessage ?? "No se pudo actualizar el servicio.";
                    await CargarCatalogosAsync();
                    return Page();
                }

                TempData["SuccessMessage"] = "Servicio actualizado correctamente.";
                return RedirectToPage();
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await CargarCatalogosAsync();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteProductoAsync(int id)
        {
            await _productoRepository.DeleteAsync(id);
            TempData["SuccessMessage"] = "Producto eliminado correctamente.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteServicioAsync(int id)
        {
            await _servicioRepository.DeleteAsync(id);
            TempData["SuccessMessage"] = "Servicio eliminado correctamente.";
            return RedirectToPage();
        }

        private async Task CargarCatalogosAsync()
        {
            Productos = (await _productoRepository.GetAllAsync()).ToList();
            Servicios = (await _servicioRepository.GetAllAsync()).ToList();
        }
    }
}
