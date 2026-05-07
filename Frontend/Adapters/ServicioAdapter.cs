using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Taller_Mecanico_Arqui.Frontend.Adapters;

public class ServicioAdapter : IServicioAdapter
{
    private readonly HttpClient _http;
    private readonly ILogger<ServicioAdapter> _logger;

    public ServicioAdapter(HttpClient http, ILogger<ServicioAdapter> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<ServicioListDto>> GetAllAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/Servicios");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetAllAsync falló: {StatusCode}", response.StatusCode);
                return new List<ServicioListDto>();
            }

            var result = await response.Content.ReadFromJsonAsync<List<ServicioListDto>>();
            return result ?? new List<ServicioListDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetAllAsync");
            return new List<ServicioListDto>();
        }
    }

    public async Task<ServicioDetalleDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _http.GetAsync($"api/Servicios/{id}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetByIdAsync({Id}) falló: {StatusCode}", id, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ServicioDetalleDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetByIdAsync({Id})", id);
            return null;
        }
    }

    public async Task<(bool Success, int? ServicioId, string? Error)> CreateAsync(CreateServicioDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/Servicios", dto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("CreateAsync falló: {StatusCode} - {Error}", response.StatusCode, error);
                return (false, null, error);
            }

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return (true, result?.GetValueOrDefault("servicioId"), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en CreateAsync");
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(UpdateServicioDto dto)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/Servicios/{dto.ServicioId}", dto);
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
            var response = await _http.DeleteAsync($"api/Servicios/{id}");
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