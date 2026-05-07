using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taller_Mecanico_Users.UseCases.Reports;

namespace Taller_Mecanico_Users.App.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly GetClientesVehiculosUseCase _clientesVehiculosUseCase;
    private readonly GetServiciosOrdenesUseCase _serviciosOrdenesUseCase;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        GetClientesVehiculosUseCase clientesVehiculosUseCase,
        GetServiciosOrdenesUseCase serviciosOrdenesUseCase,
        ILogger<ReportsController> logger)
    {
        _clientesVehiculosUseCase = clientesVehiculosUseCase;
        _serviciosOrdenesUseCase = serviciosOrdenesUseCase;
        _logger = logger;
    }

    [HttpGet("cliente/vehiculos")]
    public async Task<IActionResult> GetClientesVehiculos(
        [FromQuery] string? nombreCliente = null,
        [FromQuery] string? placa = null,
        [FromQuery] string? marca = null)
    {
        var result = await _clientesVehiculosUseCase.ExecuteAsync(nombreCliente, placa, marca);
        if (result.IsFailure)
            return StatusCode(500, new { error = result.ErrorMessage });

        return Ok(new { data = result.Value?.Clientes, generadoEn = result.Value?.GeneradoEn });
    }

    [HttpGet("servicios/ordenes")]
    public async Task<IActionResult> GetServiciosOrdenes(
        [FromQuery] DateTime? desde = null,
        [FromQuery] DateTime? hasta = null)
    {
        var fechaDesde = desde ?? DateTime.Today.AddMonths(-1);
        var fechaHasta = hasta ?? DateTime.Today;

        var result = await _serviciosOrdenesUseCase.ExecuteAsync(fechaDesde, fechaHasta);
        if (result.IsFailure)
            return StatusCode(500, new { error = result.ErrorMessage });

        return Ok(result.Value);
    }
}
