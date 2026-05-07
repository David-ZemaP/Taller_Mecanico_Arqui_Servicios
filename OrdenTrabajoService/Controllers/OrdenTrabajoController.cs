using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taller_Mecanico_Arqui.Application.DTOs.OrdenTrabajo;
using Taller_Mecanico_Arqui.Application.Facades;

namespace Taller_Mecanico_Arqui.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdenTrabajoController : ControllerBase
{
    private readonly OrdenTrabajoCreate _facade;
    private readonly OrdenTrabajoAnular _anularFacade;
    private readonly ILogger<OrdenTrabajoController> _logger;

    public OrdenTrabajoController(OrdenTrabajoCreate facade, OrdenTrabajoAnular anularFacade, ILogger<OrdenTrabajoController> logger)
    {
        _facade = facade;
        _anularFacade = anularFacade;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var ordenes = await _facade.GetAllAsync();
        return Ok(ordenes);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _facade.GetDetalleAsync(id);
        if (result.IsFailure)
            return NotFound(new { message = result.ErrorMessage });
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OrdenTrabajoFormDto dto)
    {
        var result = await _facade.RegistrarProcesoPrincipalAsync(dto);
        if (result.IsFailure)
            return BadRequest(new { message = result.ErrorMessage });
        return Ok(new { message = "Orden de trabajo registrada correctamente." });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] OrdenTrabajoFormDto dto)
    {
        dto.OrdenTrabajoId = id;
        var result = await _facade.SaveAsync(dto);
        if (result.IsFailure)
            return BadRequest(new { message = result.ErrorMessage });
        return Ok(new { message = "Orden de trabajo actualizada correctamente." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _anularFacade.DeleteAsync(id);
        if (result.IsFailure)
            return BadRequest(new { message = result.ErrorMessage });
        return Ok(new { message = "Orden de trabajo anulada correctamente." });
    }

    [HttpPost("{id:int}/restaurar")]
    public async Task<IActionResult> Restaurar(int id)
    {
        var result = await _anularFacade.AnularProcesoPrincipalAsync(id, anular: false);
        if (result.IsFailure)
            return BadRequest(new { message = result.ErrorMessage });
        return Ok(new { message = "Orden de trabajo restaurada correctamente." });
    }

    [HttpGet("vehiculos/buscar")]
    public async Task<IActionResult> BuscarVehiculos([FromQuery] string? term, [FromQuery] int? clienteId)
    {
        var vehiculos = await _facade.BuscarVehiculosAsync(term, clienteId);
        return Ok(vehiculos);
    }

    [HttpGet("estados/trabajo")]
    [AllowAnonymous]
    public IActionResult GetEstadosTrabajo()
    {
        return Ok(_facade.GetEstadoTrabajoOptions());
    }

    [HttpGet("estados/pago")]
    [AllowAnonymous]
    public IActionResult GetEstadosPago()
    {
        return Ok(_facade.GetEstadoPagoOptions());
    }
}
