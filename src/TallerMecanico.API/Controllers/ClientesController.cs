using Microsoft.AspNetCore.Mvc;
using TallerMecanico.Core.Interfaces;
using TallerMecanico.Core.Models;

namespace TallerMecanico.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Cliente>> ObtenerTodos() =>
        Ok(_clienteService.ObtenerTodos());

    [HttpGet("{id}")]
    public ActionResult<Cliente> ObtenerPorId(int id)
    {
        var cliente = _clienteService.ObtenerPorId(id);
        return cliente is null ? NotFound() : Ok(cliente);
    }

    [HttpPost]
    public ActionResult<Cliente> Crear(Cliente cliente)
    {
        var creado = _clienteService.Crear(cliente);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id}")]
    public ActionResult<Cliente> Actualizar(int id, Cliente cliente)
    {
        var actualizado = _clienteService.Actualizar(id, cliente);
        return actualizado is null ? NotFound() : Ok(actualizado);
    }

    [HttpDelete("{id}")]
    public IActionResult Eliminar(int id)
    {
        var eliminado = _clienteService.Eliminar(id);
        return eliminado ? NoContent() : NotFound();
    }
}
