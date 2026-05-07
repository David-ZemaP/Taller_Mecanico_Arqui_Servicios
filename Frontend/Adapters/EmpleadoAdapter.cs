using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Taller_Mecanico_Arqui.Frontend.Adapters;

public class EmpleadoAdapter : IEmpleadoAdapter
{
    private readonly HttpClient _http;
    private readonly ILogger<EmpleadoAdapter> _logger;

    public EmpleadoAdapter(HttpClient http, ILogger<EmpleadoAdapter> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<EmpleadoListDto>> GetAllAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/Empleados");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetAllAsync falló: {StatusCode}", response.StatusCode);
                return new List<EmpleadoListDto>();
            }

            var result = await response.Content.ReadFromJsonAsync<List<EmpleadoListDto>>();
            return result ?? new List<EmpleadoListDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetAllAsync");
            return new List<EmpleadoListDto>();
        }
    }

    public async Task<EmpleadoDetalleDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _http.GetAsync($"api/Empleados/{id}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetByIdAsync({Id}) falló: {StatusCode}", id, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<EmpleadoDetalleDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetByIdAsync({Id})", id);
            return null;
        }
    }

    public async Task<(bool Success, int? EmpleadoId, string? Error)> CreateAsync(CreateEmpleadoDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/Empleados", dto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("CreateAsync falló: {StatusCode} - {Error}", response.StatusCode, error);
                return (false, null, error);
            }

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return (true, result?.GetValueOrDefault("empleadoId"), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en CreateAsync");
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(UpdateEmpleadoDto dto)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/Empleados/{dto.EmpleadoId}", dto);
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
            var response = await _http.DeleteAsync($"api/Empleados/{id}");
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