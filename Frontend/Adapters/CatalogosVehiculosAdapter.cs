using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Taller_Mecanico_Arqui.Frontend.Adapters;

public interface ICatalogosVehiculosAdapter
{
    Task<List<CatalogoMarcaDto>> GetMarcasAsync();
    Task<List<CatalogoModeloDto>> GetModelosAsync();
    Task<List<CatalogoColorDto>> GetColoresAsync();
}

public class CatalogoMarcaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class CatalogoModeloDto
{
    public int Id { get; set; }
    public int MarcaId { get; set; }
    public string MarcaNombre { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
}

public class CatalogoColorDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class CatalogosVehiculosAdapter : ICatalogosVehiculosAdapter
{
    private readonly HttpClient _http;
    private readonly ILogger<CatalogosVehiculosAdapter> _logger;

    public CatalogosVehiculosAdapter(HttpClient http, ILogger<CatalogosVehiculosAdapter> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<List<CatalogoMarcaDto>> GetMarcasAsync()
    {
        return await GetAsync<CatalogoMarcaDto>("api/CatalogosVehiculos/marcas", "GetMarcasAsync");
    }

    public async Task<List<CatalogoModeloDto>> GetModelosAsync()
    {
        return await GetAsync<CatalogoModeloDto>("api/CatalogosVehiculos/modelos", "GetModelosAsync");
    }

    public async Task<List<CatalogoColorDto>> GetColoresAsync()
    {
        return await GetAsync<CatalogoColorDto>("api/CatalogosVehiculos/colores", "GetColoresAsync");
    }

    private async Task<List<T>> GetAsync<T>(string url, string operationName)
    {
        try
        {
            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("{Operation} falló: {StatusCode}", operationName, response.StatusCode);
                return new List<T>();
            }

            var result = await response.Content.ReadFromJsonAsync<List<T>>();
            return result ?? new List<T>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en {Operation}", operationName);
            return new List<T>();
        }
    }
}
