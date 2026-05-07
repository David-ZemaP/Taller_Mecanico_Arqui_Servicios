using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Ports;
using Taller_Mecanico_Users.Framework.DTOs.Reports;

namespace Taller_Mecanico_Users.UseCases.Reports;

public class GetUsuariosActividadUseCase
{
    private readonly IReportRepository _reportRepository;

    public GetUsuariosActividadUseCase(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result<ClientesVehiculosReportDto>> ExecuteAsync(string? nombre = null)
    {
        return await _reportRepository.GetClientesVehiculosAsync(nombre);
    }
}
