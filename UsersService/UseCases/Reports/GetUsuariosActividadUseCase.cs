using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Ports;
using Taller_Mecanico_Users.Framework.DTOs.Reports;

namespace Taller_Mecanico_Users.UseCases.Reports;

/// <summary>
/// Use Case: Obtener reporte de actividad de usuarios
/// Validaciones: Autorización (usuario debe ser Administrador)
/// Orquestación: Consulta repo y retorna Result<T>
/// </summary>
public class GetUsuariosActividadUseCase
{
    private readonly IReportRepository _reportRepository;
    private readonly ILogger<GetUsuariosActividadUseCase> _logger;

    public GetUsuariosActividadUseCase(IReportRepository reportRepository, ILogger<GetUsuariosActividadUseCase> logger)
    {
        _reportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<UsuariosActividadReportDto>> ExecuteAsync(
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null,
        string? nombreUsuario = null,
        string? tipoActividad = null)
    {
        try
        {
            _logger.LogInformation($"📊 Generando Reporte Actividad de Usuarios | Filtros: Desde={fechaDesde}, Hasta={fechaHasta}, Usuario={nombreUsuario}, Actividad={tipoActividad}");
            
            var result = await _reportRepository.GetUsuariosActividadAsync(fechaDesde, fechaHasta, nombreUsuario, tipoActividad);
            
            if (result.IsSuccess)
                _logger.LogInformation($"✅ Reporte de actividad generado exitosamente");
            else
                _logger.LogWarning($"⚠️ No se generó reporte: {result.ErrorMessage}");

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error en GetUsuariosActividadUseCase");
            return Result<UsuariosActividadReportDto>.Failure("USE_CASE_ERROR", ex.Message);
        }
    }
}
