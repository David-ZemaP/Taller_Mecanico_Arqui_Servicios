using Microsoft.AspNetCore.Mvc;
using TallerMecanico.Core.Interfaces;
using TallerMecanico.Core.Models;

namespace TallerMecanico.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiculosController : ControllerBase
{
    private readonly IVehiculoService _vehiculoService;

    public VehiculosController(IVehiculoService vehiculoService)
    {
        _vehiculoService = vehiculoService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Vehiculo>> ObtenerTodos() =>
        Ok(_vehiculoService.ObtenerTodos());

    [HttpGet("{id}")]
    public ActionResult<Vehiculo> ObtenerPorId(int id)
    {
        var vehiculo = _vehiculoService.ObtenerPorId(id);
        return vehiculo is null ? NotFound() : Ok(vehiculo);
    }

    [HttpGet("cliente/{clienteId}")]
    public ActionResult<IEnumerable<Vehiculo>> ObtenerPorCliente(int clienteId) =>
        Ok(_vehiculoService.ObtenerPorCliente(clienteId));

    [HttpPost]
    public ActionResult<Vehiculo> Crear(Vehiculo vehiculo)
    {
        var creado = _vehiculoService.Crear(vehiculo);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id}")]
    public ActionResult<Vehiculo> Actualizar(int id, Vehiculo vehiculo)
    {
        var actualizado = _vehiculoService.Actualizar(id, vehiculo);
        return actualizado is null ? NotFound() : Ok(actualizado);
    }

    [HttpDelete("{id}")]
    public IActionResult Eliminar(int id)
    {
        var eliminado = _vehiculoService.Eliminar(id);
        return eliminado ? NoContent() : NotFound();
    }
}
