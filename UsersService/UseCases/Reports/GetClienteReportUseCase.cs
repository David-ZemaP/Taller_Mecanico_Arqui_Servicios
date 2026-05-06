using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Ports;

namespace Taller_Mecanico_Users.UseCases.Reports;

public class GetClienteReportUseCase
{
    private readonly IReportRepository _reportRepository;

    public GetClienteReportUseCase(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result<ClienteReportData>> ExecuteAsync(int clienteId)
    {
        if (clienteId <= 0)
            return Result<ClienteReportData>.Failure("INVALID_ID", "Cliente ID debe ser mayor a 0");

        return await _reportRepository.GetClienteWithVehiculosAndOrdenesAsync(clienteId);
    }
}
