using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Taller_Mecanico_Arqui.Frontend.Adapters;

public class ClienteAdapter : IClienteAdapter
{
    private readonly HttpClient _http;
    private readonly ILogger<ClienteAdapter> _logger;

    public ClienteAdapter(HttpClient http, ILogger<ClienteAdapter> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<ClienteListDto>> GetAllAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/Clientes");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetAllAsync falló: {StatusCode}", response.StatusCode);
                return new List<ClienteListDto>();
            }

            var result = await response.Content.ReadFromJsonAsync<List<ClienteListDto>>();
            return result ?? new List<ClienteListDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetAllAsync");
            return new List<ClienteListDto>();
        }
    }

    public async Task<ClienteDetalleDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _http.GetAsync($"api/Clientes/{id}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetByIdAsync({Id}) falló: {StatusCode}", id, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ClienteDetalleDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetByIdAsync({Id})", id);
            return null;
        }
    }

    public async Task<(bool Success, int? ClienteId, string? Error)> CreateAsync(CreateClienteDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/Clientes", dto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("CreateAsync falló: {StatusCode} - {Error}", response.StatusCode, error);
                return (false, null, error);
            }

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return (true, result?.GetValueOrDefault("clienteId"), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en CreateAsync");
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(UpdateClienteDto dto)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/Clientes/{dto.ClienteId}", dto);
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
            var response = await _http.DeleteAsync($"api/Clientes/{id}");
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