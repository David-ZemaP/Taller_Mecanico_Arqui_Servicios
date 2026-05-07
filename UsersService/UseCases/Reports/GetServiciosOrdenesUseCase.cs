using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Ports;
using Taller_Mecanico_Users.Framework.DTOs.Reports;

namespace Taller_Mecanico_Users.UseCases.Reports;

public class GetServiciosOrdenesUseCase
{
    private readonly IReportRepository _reportRepository;
    private readonly ILogger<GetServiciosOrdenesUseCase> _logger;

    public GetServiciosOrdenesUseCase(IReportRepository reportRepository, ILogger<GetServiciosOrdenesUseCase> logger)
    {
        _reportRepository = reportRepository;
        _logger = logger;
    }

    public async Task<Result<ServiciosOrdenesReportDto>> ExecuteAsync(DateTime desde, DateTime hasta)
    {
        try
        {
            _logger.LogInformation("Generando reporte Servicios-Órdenes del {Desde} al {Hasta}", desde, hasta);
            return await _reportRepository.GetServiciosOrdenesAsync(desde, hasta);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetServiciosOrdenesUseCase");
            return Result<ServiciosOrdenesReportDto>.Failure("USE_CASE_ERROR", ex.Message);
        }
    }
}
