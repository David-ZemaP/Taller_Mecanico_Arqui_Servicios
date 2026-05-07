using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Taller_Mecanico_Arqui.Frontend.Adapters;

public class ProductoAdapter : IProductoAdapter
{
    private readonly HttpClient _http;
    private readonly ILogger<ProductoAdapter> _logger;

    public ProductoAdapter(HttpClient http, ILogger<ProductoAdapter> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<ProductoListDto>> GetAllAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/Productos");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetAllAsync falló: {StatusCode}", response.StatusCode);
                return new List<ProductoListDto>();
            }

            var result = await response.Content.ReadFromJsonAsync<List<ProductoListDto>>();
            return result ?? new List<ProductoListDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetAllAsync");
            return new List<ProductoListDto>();
        }
    }

    public async Task<ProductoDetalleDto?> GetByIdAsync(int id)
    {
        try
        {
            var response = await _http.GetAsync($"api/Productos/{id}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetByIdAsync({Id}) falló: {StatusCode}", id, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ProductoDetalleDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetByIdAsync({Id})", id);
            return null;
        }
    }

    public async Task<(bool Success, int? ProductoId, string? Error)> CreateAsync(CreateProductoDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/Productos", dto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("CreateAsync falló: {StatusCode} - {Error}", response.StatusCode, error);
                return (false, null, error);
            }

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
            return (true, result?.GetValueOrDefault("productoId"), null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en CreateAsync");
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(UpdateProductoDto dto)
    {
        try
        {
            var response = await _http.PutAsJsonAsync($"api/Productos/{dto.ProductoId}", dto);
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
            var response = await _http.DeleteAsync($"api/Productos/{id}");
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