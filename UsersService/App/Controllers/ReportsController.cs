using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Taller_Mecanico_Users.UseCases.Reports;

namespace Taller_Mecanico_Users.App.Controllers;

/// <summary>
/// Reports Controller - Exposes endpoints for business reports
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly GetClienteReportUseCase _getClienteReportUseCase;
    private readonly GetServicesMetricsUseCase _getServicesMetricsUseCase;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        GetClienteReportUseCase getClienteReportUseCase,
        GetServicesMetricsUseCase getServicesMetricsUseCase,
        ILogger<ReportsController> logger)
    {
        _getClienteReportUseCase = getClienteReportUseCase;
        _getServicesMetricsUseCase = getServicesMetricsUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Get client report with vehicles and work orders history
    /// </summary>
    [HttpGet("cliente/{clienteId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClienteReport(int clienteId)
    {
        try
        {
            _logger.LogInformation($"Generando reporte de cliente {clienteId}");
            var result = await _getClienteReportUseCase.ExecuteAsync(clienteId);
            
            if (!result.IsSuccess)
                return NotFound(new { error = result.ErrorMessage });

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error en reporte de cliente {clienteId}");
            return StatusCode(500, new { error = "Error interno" });
        }
    }

    /// <summary>
    /// Get services metrics report
    /// </summary>
    [HttpGet("servicios")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetServicesMetricsReport()
    {
        try
        {
            _logger.LogInformation("Generando reporte de servicios");
            var result = await _getServicesMetricsUseCase.ExecuteAsync();
            
            if (!result.IsSuccess)
                return StatusCode(500, new { error = result.ErrorMessage });

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en reporte de servicios");
            return StatusCode(500, new { error = "Error interno" });
        }
    }
}
