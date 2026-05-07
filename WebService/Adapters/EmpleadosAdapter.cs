using System.Net.Http.Headers;
using System.Text;
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
            try
            {
                var response = await SendAsync(HttpMethod.Get, "api/empleado");
                if (!response.IsSuccessStatusCode)
                    return (false, null, await ReadErrorAsync(response));

                var result = await DeserializeAsync<IEnumerable<EmpleadoDto>>(response);
                return (true, result, null);
            }
            catch (Exception)
            {
                return (false, null, "No se pudo conectar con el servicio de usuarios.");
            }
        }

        public async Task<(bool ok, EmpleadoDto? empleado, string? error)> GetEmpleadoByIdAsync(int id)
        {
            try
            {
                var response = await SendAsync(HttpMethod.Get, $"api/empleado/{id}");
                if (!response.IsSuccessStatusCode)
                    return (false, null, await ReadErrorAsync(response));

                var result = await DeserializeAsync<EmpleadoDto>(response);
                return (true, result, null);
            }
            catch (Exception)
            {
                return (false, null, "No se pudo conectar con el servicio de usuarios.");
            }
        }

        public async Task<(bool ok, string? error)> CrearEmpleadoAsync(EmpleadoFormDto form)
        {
            try
            {
                var body = BuildEmpleadoBody(form);
                var response = await SendAsync(HttpMethod.Post, "api/empleado", body);
                if (!response.IsSuccessStatusCode)
                    return (false, await ReadErrorAsync(response));

                return (true, null);
            }
            catch (Exception)
            {
                return (false, "No se pudo conectar con el servicio de usuarios.");
            }
        }

        public async Task<(bool ok, string? error)> ActualizarEmpleadoAsync(int id, EmpleadoFormDto form)
        {
            try
            {
                var body = BuildEmpleadoBody(form);
                var response = await SendAsync(HttpMethod.Put, $"api/empleado/{id}", body);
                if (!response.IsSuccessStatusCode)
                    return (false, await ReadErrorAsync(response));

                return (true, null);
            }
            catch (Exception)
            {
                return (false, "No se pudo conectar con el servicio de usuarios.");
            }
        }

        public async Task<(bool ok, string? error)> EliminarEmpleadoAsync(int id)
        {
            try
            {
                var response = await SendAsync(HttpMethod.Delete, $"api/empleado/{id}");
                if (!response.IsSuccessStatusCode)
                    return (false, await ReadErrorAsync(response));

                return (true, null);
            }
            catch (Exception)
            {
                return (false, "No se pudo conectar con el servicio de usuarios.");
            }
        }

        public async Task<(bool ok, IEnumerable<UsuarioDto>? usuarios, string? error)> GetAllUsuariosAsync()
        {
            try
            {
                var response = await SendAsync(HttpMethod.Get, "api/users");
                if (!response.IsSuccessStatusCode)
                    return (false, null, await ReadErrorAsync(response));

                var result = await DeserializeAsync<IEnumerable<UsuarioDto>>(response);
                return (true, result, null);
            }
            catch (Exception)
            {
                return (false, null, "No se pudo conectar con el servicio de usuarios.");
            }
        }

        public async Task<(bool ok, UsuarioDto? usuario, string? error)> GetUsuarioByIdAsync(int id)
        {
            try
            {
                var response = await SendAsync(HttpMethod.Get, $"api/users/{id}");
                if (!response.IsSuccessStatusCode)
                    return (false, null, await ReadErrorAsync(response));

                var result = await DeserializeAsync<UsuarioDto>(response);
                return (true, result, null);
            }
            catch (Exception)
            {
                return (false, null, "No se pudo conectar con el servicio de usuarios.");
            }
        }

        public async Task<(bool ok, string? plainPassword, string? error)> CreateUsuarioAsync(int empleadoId, string email, string? password)
        {
            try
            {
                var body = new { empleadoId, email, password };
                var response = await SendAsync(HttpMethod.Post, "api/users", body);
                if (!response.IsSuccessStatusCode)
                    return (false, null, await ReadErrorAsync(response));

                var json = await response.Content.ReadAsStringAsync();
                string? plain = null;
                if (!string.IsNullOrWhiteSpace(json))
                {
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("plainPassword", out var pw))
                        plain = pw.GetString();
                }
                return (true, plain, null);
            }
            catch (Exception)
            {
                return (false, null, "No se pudo conectar con el servicio de usuarios.");
            }
        }

        public async Task<(bool ok, string? error)> UpdateUsuarioAsync(int id, string email, bool activo)
        {
            try
            {
                var body = new { email, activo };
                var response = await SendAsync(HttpMethod.Put, $"api/users/{id}", body);
                if (!response.IsSuccessStatusCode)
                    return (false, await ReadErrorAsync(response));

                return (true, null);
            }
            catch (Exception)
            {
                return (false, "No se pudo conectar con el servicio de usuarios.");
            }
        }

        public async Task<(bool ok, string? plainPassword, string? error)> ResetPasswordAsync(int id)
        {
            try
            {
                var response = await SendAsync(HttpMethod.Post, $"api/users/{id}/reset-password");
                if (!response.IsSuccessStatusCode)
                    return (false, null, await ReadErrorAsync(response));

                var json = await response.Content.ReadAsStringAsync();
                string? plain = null;
                if (!string.IsNullOrWhiteSpace(json))
                {
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("plainPassword", out var pw))
                        plain = pw.GetString();
                }
                return (true, plain, null);
            }
            catch (Exception)
            {
                return (false, null, "No se pudo conectar con el servicio de usuarios.");
            }
        }

        private static object BuildEmpleadoBody(EmpleadoFormDto form) => new
        {
            nombre = form.Nombres,
            primerApellido = form.PrimerApellido,
            segundoApellido = form.SegundoApellido,
            ci = form.CiNumero,
            ciComplemento = form.CiComplemento,
            telefono = form.Telefono,
            email = form.Email,
            fechaContratacion = form.FechaContratacion,
            tipoEmpleado = form.TipoEmpleado,
            estadoLaboral = form.EstadoLaboral,
            especialidad = form.Especialidad,
            salarioPorHora = form.SalarioPorHora,
            salarioMensual = form.SalarioMensual,
            nivelAcceso = form.NivelAcceso
        };

        private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string endpoint, object? body = null)
        {
            var request = new HttpRequestMessage(method, endpoint);

            var token = _ctx.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (body is not null)
                request.Content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
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
