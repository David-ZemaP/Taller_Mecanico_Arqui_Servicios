using System.Net.Http.Json;
using Taller_Mecanico_Arqui.Frontend.DTOs.OrdenTrabajo;

namespace Taller_Mecanico_Arqui.Frontend.Adapters;

/// <summary>
/// Interface para el adapter de Orden de Trabajo.
/// Abstrae las llamadas HTTP al OrdenTrabajoService.
/// </summary>
public interface IOrdenTrabajoAdapter
{
    Task<List<OrdenTrabajoListDto>> GetAllAsync();
    Task<OrdenTrabajoDetalleDto?> GetByIdAsync(int id);
    Task<List<VehiculoLookupDto>> BuscarVehiculosAsync(string? term, int? clienteId);
    Task<List<string>> GetEstadoTrabajoOptionsAsync();
    Task<List<string>> GetEstadoPagoOptionsAsync();
    Task<bool> SaveAsync(OrdenTrabajoFormDto dto);
    Task<bool> AnularAsync(int id);
    Task<bool> ReactivarAsync(int id);
    Task<bool> ActualizarStockAsync(List<CreateOrdenTrabajoProductoDto> productos);
}