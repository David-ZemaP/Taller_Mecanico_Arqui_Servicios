using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Ports;
using Taller_Mecanico_Users.Framework.DTOs.Reports;

namespace Taller_Mecanico_Users.UseCases.Reports;

/// <summary>
/// Use Case: Obtener reporte de clientes con vehículos
/// Validaciones: Autorización (usuario debe ser Administrador/Empleado)
/// Orquestación: Consulta repo y retorna Result<T>
/// </summary>
public class GetClientesVehiculosUseCase
{
    private readonly IReportRepository _reportRepository;
    private readonly ILogger<GetClientesVehiculosUseCase> _logger;

    public GetClientesVehiculosUseCase(IReportRepository reportRepository, ILogger<GetClientesVehiculosUseCase> logger)
    {
        _reportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ClientesVehiculosReportDto>> ExecuteAsync(
        string? nombreCliente = null,
        string? placaVehiculo = null,
        string? marcaVehiculo = null)
    {
        try
        {
            _logger.LogInformation($"📊 Generando Reporte Clientes-Vehículos | Filtros: Nombre={nombreCliente}, Placa={placaVehiculo}, Marca={marcaVehiculo}");
            
            var result = await _reportRepository.GetClientesVehiculosAsync(nombreCliente, placaVehiculo, marcaVehiculo);
            
            if (result.IsSuccess)
                _logger.LogInformation($"✅ Reporte generado exitosamente");
            else
                _logger.LogWarning($"⚠️ No se generó reporte: {result.ErrorMessage}");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error en GetClientesVehiculosUseCase");
            return Result<ClientesVehiculosReportDto>.Failure("USE_CASE_ERROR", ex.Message);
        }
    }
}
