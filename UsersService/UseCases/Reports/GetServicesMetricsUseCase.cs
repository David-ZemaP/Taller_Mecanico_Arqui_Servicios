using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Ports;

namespace Taller_Mecanico_Users.UseCases.Reports;

public class GetServicesMetricsUseCase
{
    private readonly IReportRepository _reportRepository;

    public GetServicesMetricsUseCase(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result<ServicesMetricsData>> ExecuteAsync()
    {
        return await _reportRepository.GetServicesMetricsAsync();
    }
}
