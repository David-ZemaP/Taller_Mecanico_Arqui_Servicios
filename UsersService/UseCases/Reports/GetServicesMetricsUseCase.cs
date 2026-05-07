using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Ports;
using Taller_Mecanico_Users.Framework.DTOs.Reports;

namespace Taller_Mecanico_Users.UseCases.Reports;

public class GetServicesMetricsUseCase
{
    private readonly IReportRepository _reportRepository;

    public GetServicesMetricsUseCase(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result<ServiciosOrdenesReportDto>> ExecuteAsync()
    {
        return await _reportRepository.GetServiciosOrdenesAsync(
            DateTime.Today.AddMonths(-1),
            DateTime.Today);
    }
}
