using Microsoft.AspNetCore.Mvc;
using TallerMecanico.Core.Interfaces;
using TallerMecanico.Core.Models;

namespace TallerMecanico.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdenesTrabajoController : ControllerBase
{
    private readonly IOrdenTrabajoService _ordenService;

    public OrdenesTrabajoController(IOrdenTrabajoService ordenService)
    {
        _ordenService = ordenService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<OrdenTrabajo>> ObtenerTodas() =>
        Ok(_ordenService.ObtenerTodas());

    [HttpGet("{id}")]
    public ActionResult<OrdenTrabajo> ObtenerPorId(int id)
    {
        var orden = _ordenService.ObtenerPorId(id);
        return orden is null ? NotFound() : Ok(orden);
    }

    [HttpGet("cliente/{clienteId}")]
    public ActionResult<IEnumerable<OrdenTrabajo>> ObtenerPorCliente(int clienteId) =>
        Ok(_ordenService.ObtenerPorCliente(clienteId));

    [HttpPost]
    public ActionResult<OrdenTrabajo> Crear(OrdenTrabajo orden)
    {
        var creada = _ordenService.Crear(orden);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }

    [HttpPut("{id}")]
    public ActionResult<OrdenTrabajo> Actualizar(int id, OrdenTrabajo orden)
    {
        var actualizada = _ordenService.Actualizar(id, orden);
        return actualizada is null ? NotFound() : Ok(actualizada);
    }

    [HttpPatch("{id}/estado")]
    public ActionResult<OrdenTrabajo> CambiarEstado(int id, [FromBody] EstadoOrden estado)
    {
        var actualizada = _ordenService.CambiarEstado(id, estado);
        return actualizada is null ? NotFound() : Ok(actualizada);
    }

    [HttpDelete("{id}")]
    public IActionResult Eliminar(int id)
    {
        var eliminada = _ordenService.Eliminar(id);
        return eliminada ? NoContent() : NotFound();
    }
}
