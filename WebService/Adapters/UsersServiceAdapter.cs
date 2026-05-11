using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebService.DTOs;

namespace WebService.Adapters
{
    public class UsersServiceAdapter
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public UsersServiceAdapter(HttpClient http, IHttpContextAccessor ctx)
        {
            _http = http;
            _ctx = ctx;
        }

        public async Task<(bool ok, UsersLoginResponseDto? response, string? error)> LoginAsync(string email, string password)
        {
            HttpResponseMessage httpResponse;
            try
            {
                httpResponse = await SendAsync(HttpMethod.Post, "api/auth/login", new
                {
                    email,
                    password
                }, includeBearerToken: false);
            }
            catch (Exception)
            {
                return (false, null, "No se pudo conectar con el servicio de usuarios.");
            }

            if (!httpResponse.IsSuccessStatusCode)
            {
                return (false, null, await ReadErrorAsync(httpResponse));
            }

            var response = await DeserializeAsync<UsersLoginResponseDto>(httpResponse);
            return (true, response, null);
        }

        public async Task<(bool ok, string? error)> ChangePasswordAsync(int usuarioLoginId, string currentPassword, string newPassword, string confirmPassword)
        {
            HttpResponseMessage response;
            try
            {
                response = await SendAsync(HttpMethod.Post, $"api/users/{usuarioLoginId}/change-password", new ChangePasswordRequestDto
                {
                    CurrentPassword = currentPassword,
                    NewPassword = newPassword,
                    ConfirmPassword = confirmPassword
                });
            }
            catch (Exception)
            {
                return (false, "No se pudo conectar con el servicio de usuarios.");
            }

            if (!response.IsSuccessStatusCode)
            {
                return (false, await ReadErrorAsync(response));
            }

            return (true, null);
        }

        private Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body = null, bool includeBearerToken = true)
        {
            var request = new HttpRequestMessage(method, url);

            if (includeBearerToken)
            {
                var token = _ctx.HttpContext?.Session.GetString("JwtToken");
                if (string.IsNullOrWhiteSpace(token))
                    token = _ctx.HttpContext?.User.FindFirst("JwtToken")?.Value;
                if (!string.IsNullOrWhiteSpace(token))
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            if (body is not null)
            {
                request.Content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json");
            }

            return _http.SendAsync(request);
        }

        private static async Task<T?> DeserializeAsync<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOpts);
        }

        private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
        {
            try
            {
                var json = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(json))
                {
                    return $"Error HTTP {(int)response.StatusCode}.";
                }

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("message", out var messageProp) && messageProp.ValueKind == JsonValueKind.String)
                {
                    return messageProp.GetString() ?? $"Error HTTP {(int)response.StatusCode}.";
                }

                if (root.TryGetProperty("error", out var errorProp) && errorProp.ValueKind == JsonValueKind.String)
                {
                    return errorProp.GetString() ?? $"Error HTTP {(int)response.StatusCode}.";
                }
            }
            catch
            {
            }

            return $"Error HTTP {(int)response.StatusCode}.";
        }
    }
}