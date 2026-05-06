using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Framework.DTOs.Reports;

namespace Taller_Mecanico_Users.Domain.Ports;

/// <summary>
/// Puerto (Interfaz) para acceso a datos de reportes.
/// Define contratos para:
/// - Reporte 1: Clientes + Vehículos (con filtros)
/// - Reporte 2: Servicios + Órdenes (con agregaciones)
/// </summary>
public interface IReportRepository
{
    /// <summary>
    /// Obtiene listado de clientes con vehículos (opcionalmente filtrado).
    /// Implementación: SQL LEFT JOIN clientes-vehiculos
    /// </summary>
    Task<Result<ClientesVehiculosReportDto>> GetClientesVehiculosAsync(
        string? nombreCliente = null,
        string? placaVehiculo = null,
        string? marcaVehiculo = null);

    /// <summary>
    /// Obtiene métricas de servicios dentro de un rango de fechas.
    /// Implementación: SQL aggregation (SUM, COUNT, GROUP BY)
    /// </summary>
    Task<Result<ServiciosOrdenesReportDto>> GetServiciosOrdenesAsync(
        DateTime desde,
        DateTime hasta);
}
