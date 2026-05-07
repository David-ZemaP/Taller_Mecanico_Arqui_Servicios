using System.Net.Http.Headers;
using System.Text.Json;
using WebService.DTOs;

namespace WebService.Adapters
{
    public class ClientesAdapter
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

        public ClientesAdapter(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(bool ok, IEnumerable<ClienteDto>? clientes, string? error)> GetAllClientesAsync()
        {
            var response = await SendAsync(HttpMethod.Get, "api/users/clientes");
            if (!response.IsSuccessStatusCode)
                return (false, null, await ReadErrorAsync(response));

            var result = await DeserializeAsync<IEnumerable<ClienteDto>>(response);
            return (true, result, null);
        }

        public async Task<(bool ok, ClienteDto? cliente, string? error)> GetClienteByIdAsync(int id)
        {
            var response = await SendAsync(HttpMethod.Get, $"api/users/{id}");
            if (!response.IsSuccessStatusCode)
                return (false, null, await ReadErrorAsync(response));

            var result = await DeserializeAsync<ClienteDto>(response);
            return (true, result, null);
        }

        private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string endpoint, object? data = null, bool includeBearerToken = true)
        {
            var request = new HttpRequestMessage(method, endpoint);

            if (data != null)
            {
                var json = JsonSerializer.Serialize(data);
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            }

            if (includeBearerToken)
            {
                var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                    token = _httpContextAccessor.HttpContext?.User.FindFirst("JwtToken")?.Value;
                if (!string.IsNullOrEmpty(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await _httpClient.SendAsync(request);
        }

        private async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }

        private async Task<string> ReadErrorAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content)) return $"Error: {response.StatusCode}";
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("message", out var msg))
                    return msg.GetString() ?? content;
            }
            catch { }
            return content;
        }
    }
}
