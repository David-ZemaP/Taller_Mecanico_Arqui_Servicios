using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Taller_Mecanico_Arqui.Frontend.DTOs.OrdenTrabajo;

namespace Taller_Mecanico_Arqui.Frontend.Adapters;

/// <summary>
/// Implementación del adapter que consume la API REST del OrdenTrabajoService via HttpClient.
/// </summary>
public class OrdenTrabajoAdapter : IOrdenTrabajoAdapter
{
    private readonly HttpClient _http;
    private readonly ILogger<OrdenTrabajoAdapter> _logger;

    public OrdenTrabajoAdapter(HttpClient http, ILogger<OrdenTrabajoAdapter> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<OrdenTrabajoListDto>> GetAllAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/OrdenTrabajos");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetAllAsync falló: {StatusCode}", response.StatusCode);
                return new List<OrdenTrabajoListDto>();
            }

            var result = await response.Content.ReadFromJsonAsync<List<OrdenTrabajoListDto>>();
            return result ?? new List<OrdenTrabajoListDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetAllAsync");
            return new List<OrdenTrabajoListDto>();
        }
    }

    public async Task<OrdenTrabajoDetalleDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _http.GetAsync($"api/OrdenTrabajos/{id}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetByIdAsync({Id}) falló: {StatusCode}", id, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<OrdenTrabajoDetalleDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetByIdAsync({Id})", id);
            return null;
        }
    }

    public async Task<List<VehiculoLookupDto>> BuscarVehiculosAsync(string? term, int? clienteId)
    {
        try
        {
            var url = "api/OrdenTrabajos/buscar-vehiculos";
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(term))
                queryParams.Add($"term={Uri.EscapeDataString(term)}");
            if (clienteId.HasValue)
                queryParams.Add($"clienteId={clienteId.Value}");

            if (queryParams.Count > 0)
                url += "?" + string.Join("&", queryParams);

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("BuscarVehiculosAsync falló: {StatusCode}", response.StatusCode);
                return new List<VehiculoLookupDto>();
            }

            var result = await response.Content.ReadFromJsonAsync<List<VehiculoLookupDto>>();
            return result ?? new List<VehiculoLookupDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en BuscarVehiculosAsync");
            return new List<VehiculoLookupDto>();
        }
    }

    public async Task<List<string>> GetEstadoTrabajoOptionsAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/OrdenTrabajos/opciones-estado-trabajo");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetEstadoTrabajoOptionsAsync falló: {StatusCode}", response.StatusCode);
                return new List<string>();
            }

            var result = await response.Content.ReadFromJsonAsync<List<string>>();
            return result ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetEstadoTrabajoOptionsAsync");
            return new List<string>();
        }
    }

    public async Task<List<string>> GetEstadoPagoOptionsAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/OrdenTrabajos/opciones-estado-pago");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetEstadoPagoOptionsAsync falló: {StatusCode}", response.StatusCode);
                return new List<string>();
            }

            var result = await response.Content.ReadFromJsonAsync<List<string>>();
            return result ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetEstadoPagoOptionsAsync");
            return new List<string>();
        }
    }

    public async Task<int?> SaveAsync(OrdenTrabajoFormDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/OrdenTrabajos", dto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("SaveAsync falló: {StatusCode} - {Error}", response.StatusCode, error);
                return null;
            }

            var body = await response.Content.ReadFromJsonAsync<SaveOrdenResponse>();
            return body?.OrdenTrabajoId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en SaveAsync");
            return null;
        }
    }

    private record SaveOrdenResponse(int OrdenTrabajoId, string Message);

    public async Task<bool> AnularAsync(int id)
    {
        try
        {
            var response = await _http.PostAsync($"api/OrdenTrabajos/{id}/anular", null);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("AnularAsync({Id}) falló: {StatusCode}", id, response.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en AnularAsync({Id})", id);
            return false;
        }
    }

    public async Task<bool> ReactivarAsync(int id)
    {
        try
        {
            var response = await _http.PostAsync($"api/OrdenTrabajos/{id}/reactivar", null);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ReactivarAsync({Id}) falló: {StatusCode}", id, response.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ReactivarAsync({Id})", id);
            return false;
        }
    }

    public async Task<bool> ActualizarStockAsync(List<CreateOrdenTrabajoProductoDto> productos)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/OrdenTrabajos/actualizar-stock", productos);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ActualizarStockAsync falló: {StatusCode}", response.StatusCode);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ActualizarStockAsync");
            return false;
        }
    }
}