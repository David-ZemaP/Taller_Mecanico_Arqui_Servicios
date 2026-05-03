using Microsoft.AspNetCore.Mvc;
using TallerMecanico.Core.Interfaces;
using TallerMecanico.Core.Models;

namespace TallerMecanico.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiciosController : ControllerBase
{
    private readonly IServicioService _servicioService;

    public ServiciosController(IServicioService servicioService)
    {
        _servicioService = servicioService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Servicio>> ObtenerTodos() =>
        Ok(_servicioService.ObtenerTodos());

    [HttpGet("{id}")]
    public ActionResult<Servicio> ObtenerPorId(int id)
    {
        var servicio = _servicioService.ObtenerPorId(id);
        return servicio is null ? NotFound() : Ok(servicio);
    }

    [HttpPost]
    public ActionResult<Servicio> Crear(Servicio servicio)
    {
        var creado = _servicioService.Crear(servicio);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id}")]
    public ActionResult<Servicio> Actualizar(int id, Servicio servicio)
    {
        var actualizado = _servicioService.Actualizar(id, servicio);
        return actualizado is null ? NotFound() : Ok(actualizado);
    }

    [HttpDelete("{id}")]
    public IActionResult Eliminar(int id)
    {
        var eliminado = _servicioService.Eliminar(id);
        return eliminado ? NoContent() : NotFound();
    }
}
