using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Taller_Mecanico_Arqui.Frontend.Adapters;

public class VehiculoAdapter : IVehiculoAdapter
{
    private readonly HttpClient _http;
    private readonly ILogger<VehiculoAdapter> _logger;

    public VehiculoAdapter(HttpClient http, ILogger<VehiculoAdapter> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<VehiculoListDto>> GetAllAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/Vehiculos");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetAllAsync falló: {StatusCode}", response.StatusCode);
                return new List<VehiculoListDto>();
            }

            var result = await response.Content.ReadFromJsonAsync<List<VehiculoListDto>>();
            return result ?? new List<VehiculoListDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetAllAsync");
            return new List<VehiculoListDto>();
        }
    }

    public async Task<List<VehiculoListDto>> GetByClienteIdAsync(int clienteId)
    {
        try
        {
            var response = await _http.GetAsync($"api/Vehiculos/cliente/{clienteId}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetByClienteIdAsync({ClienteId}) falló: {StatusCode}", clienteId, response.StatusCode);
                return new List<VehiculoListDto>();
            }

            var result = await response.Content.ReadFromJsonAsync<List<VehiculoListDto>>();
            return result ?? new List<VehiculoListDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetByClienteIdAsync({ClienteId})", clienteId);
            return new List<VehiculoListDto>();
        }
    }

    public async Task<VehiculoDetalleDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _http.GetAsync($"api/Vehiculos/{id}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetByIdAsync({Id}) falló: {StatusCode}", id, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<VehiculoDetalleDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetByIdAsync({Id})", id);
            return null;
        }
    }

    public async Task<(bool Success, int? VehiculoId, string? Error)> CreateAsync(CreateVehiculoDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/Vehiculos", dto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("CreateAsync falló: {StatusCode} - {Error}", response.StatusCode, error);
                return (false, null, error);
            }

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return (true, result?.GetValueOrDefault("vehiculoId"), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en CreateAsync");
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(UpdateVehiculoDto dto)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/Vehiculos/{dto.VehiculoId}", dto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("UpdateAsync falló: {StatusCode} - {Error}", response.StatusCode, error);
                return (false, error);
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en UpdateAsync");
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/Vehiculos/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("DeleteAsync({Id}) falló: {StatusCode} - {Error}", id, response.StatusCode, error);
                return (false, error);
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en DeleteAsync({Id})", id);
            return (false, ex.Message);
        }
    }
}