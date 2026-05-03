using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TallerMecanico.Core.Models;
using TallerMecanico.Services;

namespace Front.Pages;

public class IndexModel : PageModel
{
    private readonly ClienteService _clienteService;
    private readonly VehiculoService _vehiculoService;
    private readonly ProductoService _productoService;
    private readonly ServicioCatalogoService _servicioCatalogoService;
    private readonly OrdenTrabajoService _ordenTrabajoService;

    public IndexModel(
        ClienteService clienteService,
        VehiculoService vehiculoService,
        ProductoService productoService,
        ServicioCatalogoService servicioCatalogoService,
        OrdenTrabajoService ordenTrabajoService)
    {
        _clienteService = clienteService;
        _vehiculoService = vehiculoService;
        _productoService = productoService;
        _servicioCatalogoService = servicioCatalogoService;
        _ordenTrabajoService = ordenTrabajoService;
    }

    public List<Cliente> Clientes { get; private set; } = new();

    public List<Vehiculo> Vehiculos { get; private set; } = new();

    public List<Producto> Productos { get; private set; } = new();

    public List<Servicio> Servicios { get; private set; } = new();

    public List<OrdenTrabajo> Ordenes { get; private set; } = new();

    public IEnumerable<SelectListItem> ClienteOptions => Clientes.Select(cliente => new SelectListItem($"{cliente.Nombre} {cliente.Apellido}".Trim(), cliente.Id.ToString()));

    public IEnumerable<SelectListItem> VehiculoOptions => Vehiculos.Select(vehiculo => new SelectListItem($"{vehiculo.Placa} - {vehiculo.Marca} {vehiculo.Modelo}".Trim(), vehiculo.Id.ToString()));

    public IEnumerable<SelectListItem> ProductoOptions => Productos.Select(producto => new SelectListItem($"{producto.Nombre} - {producto.Precio:C}", producto.Id.ToString()));

    public IEnumerable<SelectListItem> ServicioOptions => Servicios.Select(servicio => new SelectListItem($"{servicio.Nombre} - {servicio.Precio:C}", servicio.Id.ToString()));

    [BindProperty]
    public int? ClienteId { get; set; }

    [BindProperty]
    public int? VehiculoId { get; set; }

    [BindProperty]
    public string? Descripcion { get; set; }

    [BindProperty]
    public int? EditingOrderId { get; set; }

    [BindProperty]
    public string? ProductosJson { get; set; }

    [BindProperty]
    public string? ServiciosJson { get; set; }

    public string? SuccessMessage { get; private set; }

    public void OnGet(int? clienteId = null, int? vehiculoId = null)
    {
        ClienteId = clienteId;
        VehiculoId = vehiculoId;
        LoadData();
    }

    public IActionResult OnPostGuardar()
    {
        LoadData();

        var orden = new OrdenTrabajo
        {
            ClienteId = ClienteId ?? 0,
            VehiculoId = VehiculoId ?? 0,
            Descripcion = Descripcion,
            Productos = ParseProductos(),
            Servicios = ParseServicios()
        };

        if (EditingOrderId.HasValue && EditingOrderId.Value > 0)
        {
            var updated = _ordenTrabajoService.Actualizar(EditingOrderId.Value, orden);
            if (updated is null)
            {
                ModelState.AddModelError(string.Empty, "No se encontró la orden para editar.");
                return Page();
            }

            SuccessMessage = $"Orden #{updated.Id} actualizada correctamente.";
        }
        else
        {
            var created = _ordenTrabajoService.Crear(orden, usuarioId: 1);
            SuccessMessage = $"Orden #{created.Id} creada correctamente.";
        }

        ResetInputs();
        LoadData();
        return Page();
    }

    public IActionResult OnPostAnular(int id)
    {
        _ordenTrabajoService.Anular(id, usuarioId: 1);
        LoadData();
        SuccessMessage = $"Orden #{id} anulada correctamente.";
        return Page();
    }

    private void LoadData()
    {
        Clientes = _clienteService.ObtenerTodos().ToList();
        Vehiculos = _vehiculoService.ObtenerTodos().ToList();
        Productos = _productoService.ObtenerTodos().ToList();
        Servicios = _servicioCatalogoService.ObtenerTodos().ToList();
        Ordenes = _ordenTrabajoService.ObtenerTodos().OrderByDescending(orden => orden.Id).ToList();
    }

    private List<DetalleProducto> ParseProductos()
    {
        if (string.IsNullOrWhiteSpace(ProductosJson))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<DetalleProducto>>(ProductosJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
    }

    private List<Servicio> ParseServicios()
    {
        if (string.IsNullOrWhiteSpace(ServiciosJson))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<Servicio>>(ServiciosJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
    }

    private void ResetInputs()
    {
        ClienteId = null;
        VehiculoId = null;
        Descripcion = string.Empty;
        EditingOrderId = null;
        ProductosJson = null;
        ServiciosJson = null;
    }
}