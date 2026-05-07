using System.Net.Http.Headers;
using System.Text.Json;
using WebService.DTOs;

namespace WebService.Adapters
{
    public class EmpleadosAdapter
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;
        private readonly JsonSerializerOptions _jsonOpts = new() { PropertyNameCaseInsensitive = true };

        public EmpleadosAdapter(HttpClient http, IHttpContextAccessor ctx)
        {
            _http = http;
            _ctx = ctx;
        }

        public async Task<(bool ok, IEnumerable<EmpleadoDto>? empleados, string? error)> GetAllEmpleadosAsync()
        {
            var response = await SendAsync(HttpMethod.Get, "api/users");
            if (!response.IsSuccessStatusCode)
                return (false, null, await ReadErrorAsync(response));

            var todos = await DeserializeAsync<IEnumerable<EmpleadoDto>>(response);
            var empleados = todos?.Where(u => !u.EsCliente);
            return (true, empleados, null);
        }

        public async Task<(bool ok, EmpleadoDto? empleado, string? error)> GetEmpleadoByIdAsync(int id)
        {
            var response = await SendAsync(HttpMethod.Get, $"api/users/{id}");
            if (!response.IsSuccessStatusCode)
                return (false, null, await ReadErrorAsync(response));

            var result = await DeserializeAsync<EmpleadoDto>(response);
            return (true, result, null);
        }

        public async Task<(bool ok, string? error)> UpdateEmpleadoAsync(int id, string email, bool activo)
        {
            var response = await SendAsync(HttpMethod.Put, $"api/users/{id}", new { email, activo });
            if (!response.IsSuccessStatusCode)
                return (false, await ReadErrorAsync(response));

            return (true, null);
        }

        public async Task<(bool ok, string? error)> ResetPasswordAsync(int id)
        {
            var response = await SendAsync(HttpMethod.Post, $"api/users/{id}/reset-password");
            if (!response.IsSuccessStatusCode)
                return (false, await ReadErrorAsync(response));

            return (true, null);
        }

        private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string endpoint, object? body = null)
        {
            var request = new HttpRequestMessage(method, endpoint);

            var token = _ctx.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (body is not null)
                request.Content = new StringContent(
                    JsonSerializer.Serialize(body),
                    System.Text.Encoding.UTF8,
                    "application/json");

            return await _http.SendAsync(request);
        }

        private async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, _jsonOpts);
        }

        private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
        {
            try
            {
                var json = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
                        return msg.GetString() ?? $"Error HTTP {(int)response.StatusCode}.";
                    if (root.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.String)
                        return err.GetString() ?? $"Error HTTP {(int)response.StatusCode}.";
                }
            }
            catch { }
            return $"Error HTTP {(int)response.StatusCode}.";
        }
    }
}
