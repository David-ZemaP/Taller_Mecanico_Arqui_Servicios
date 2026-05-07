using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using WebService.DTOs;

namespace WebService.Adapters
{
    /// <summary>
    /// HTTP client adapter for OrdenTrabajoService (S1).
    /// Reads the JWT token from session key "JwtToken" and sends it as Bearer on every request.
    /// </summary>
    public class OrdenTrabajoAdapter
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public OrdenTrabajoAdapter(HttpClient http, IHttpContextAccessor ctx)
        {
            _http = http;
            _ctx = ctx;
        }

        // ─── Órdenes de Trabajo ───────────────────────────────────────────────

        public Task<List<OrdenTrabajoListDto>> GetAllOrdenesAsync()
            => GetAsync<List<OrdenTrabajoListDto>>("api/ordenestrabajo") ?? Task.FromResult(new List<OrdenTrabajoListDto>());

        public Task<OrdenTrabajoDetalleDto?> GetOrdenDetalleAsync(int id)
            => GetAsync<OrdenTrabajoDetalleDto>($"api/ordenestrabajo/{id}");

        public async Task<(bool ok, string? error, int id)> RegistrarOrdenAsync(OrdenTrabajoFormDto form)
        {
            var body = new
            {
                vehiculoId = form.VehiculoId,
                fechaIngreso = form.FechaIngreso,
                estadoVehiculo = form.EstadoVehiculo,
                estadoTrabajo = form.EstadoTrabajo,
                estadoPago = form.EstadoPago,
                total = form.Total,
                productos = form.Productos.Select(p => new { p.ProductoId, p.Cantidad, p.PrecioUnitario }),
                servicios = form.Servicios.Select(s => new { s.ServicioId, s.Cantidad, s.PrecioUnitario }),
                mecanicosSeleccionados = form.MecanicosSeleccionados
            };

            var response = await SendAsync(HttpMethod.Post, "api/ordenestrabajo", body);
            if (!response.IsSuccessStatusCode)
            {
                var err = await ReadErrorAsync(response);
                return (false, err, 0);
            }

            var result = await DeserializeAsync<JsonElement>(response);
            var newId = result.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : 0;
            return (true, null, newId);
        }

        public async Task<(bool ok, string? error)> ActualizarOrdenAsync(OrdenTrabajoFormDto form)
        {
            var body = new
            {
                ordenTrabajoId = form.OrdenTrabajoId,
                vehiculoId = form.VehiculoId,
                fechaIngreso = form.FechaIngreso,
                fechaEntrega = form.FechaEntrega,
                estadoTrabajo = form.EstadoTrabajo,
                estadoPago = form.EstadoPago,
                estadoVehiculo = form.EstadoVehiculo,
                total = form.Total
            };

            var response = await SendAsync(HttpMethod.Put, $"api/ordenestrabajo/{form.OrdenTrabajoId}", body);
            if (!response.IsSuccessStatusCode)
                return (false, await ReadErrorAsync(response));
            return (true, null);
        }

        public async Task<(bool ok, string? error)> AnularOrdenAsync(int id)
        {
            var response = await SendAsync(HttpMethod.Delete, $"api/ordenestrabajo/{id}");
            if (!response.IsSuccessStatusCode)
                return (false, await ReadErrorAsync(response));
            return (true, null);
        }

        public async Task<(bool ok, string? error)> RestaurarOrdenAsync(int id)
        {
            var response = await SendAsync(HttpMethod.Put, $"api/ordenestrabajo/{id}/restaurar");
            if (!response.IsSuccessStatusCode)
                return (false, await ReadErrorAsync(response));
            return (true, null);
        }

        public Task<List<VehiculoLookupDto>> BuscarVehiculosAsync(string term, int? clienteId = null)
        {
            var query = $"api/ordenestrabajo/vehiculos/buscar?term={Uri.EscapeDataString(term)}";
            if (clienteId.HasValue) query += $"&clienteId={clienteId}";
            return GetAsync<List<VehiculoLookupDto>>(query) ?? Task.FromResult(new List<VehiculoLookupDto>());
        }

        // ─── Productos ────────────────────────────────────────────────────────

        public Task<List<ProductoDto>> GetAllProductosAsync()
            => GetAsync<List<ProductoDto>>("api/productos") ?? Task.FromResult(new List<ProductoDto>());

        public Task<ProductoDto?> GetProductoAsync(int id)
            => GetAsync<ProductoDto>($"api/productos/{id}");

        public async Task<(bool ok, string? error)> SaveProductoAsync(ProductoFormDto form)
        {
            HttpResponseMessage response;
            if (form.ProductoId == 0)
            {
                var body = new { form.Nombre, form.Precio, form.Stock };
                response = await SendAsync(HttpMethod.Post, "api/productos", body);
            }
            else
            {
                var body = new { form.ProductoId, form.Nombre, form.Precio, form.Stock };
                response = await SendAsync(HttpMethod.Put, $"api/productos/{form.ProductoId}", body);
            }

            if (!response.IsSuccessStatusCode)
                return (false, await ReadErrorAsync(response));
            return (true, null);
        }

        public Task DeleteProductoAsync(int id) => SendAsync(HttpMethod.Delete, $"api/productos/{id}");

        // ─── Servicios ────────────────────────────────────────────────────────

        public Task<List<ServicioDto>> GetAllServiciosAsync()
            => GetAsync<List<ServicioDto>>("api/servicios") ?? Task.FromResult(new List<ServicioDto>());

        public Task<ServicioDto?> GetServicioAsync(int id)
            => GetAsync<ServicioDto>($"api/servicios/{id}");

        public async Task<(bool ok, string? error)> SaveServicioAsync(ServicioFormDto form)
        {
            HttpResponseMessage response;
            if (form.ServicioId == 0)
            {
                var body = new { form.Nombre, form.Precio };
                response = await SendAsync(HttpMethod.Post, "api/servicios", body);
            }
            else
            {
                var body = new { form.ServicioId, form.Nombre, form.Precio };
                response = await SendAsync(HttpMethod.Put, $"api/servicios/{form.ServicioId}", body);
            }

            if (!response.IsSuccessStatusCode)
                return (false, await ReadErrorAsync(response));
            return (true, null);
        }

        public Task DeleteServicioAsync(int id) => SendAsync(HttpMethod.Delete, $"api/servicios/{id}");

        // ─── Vehículos ────────────────────────────────────────────────────────

        public Task<List<VehiculoListDto>> GetAllVehiculosAsync()
            => GetAsync<List<VehiculoListDto>>("api/vehiculos") ?? Task.FromResult(new List<VehiculoListDto>());

        public Task<List<VehiculoLookupDto>> BuscarVehiculosPorPlacaAsync(string term, int? clienteId = null)
        {
            var query = $"api/vehiculos/buscar?term={Uri.EscapeDataString(term)}";
            if (clienteId.HasValue) query += $"&clienteId={clienteId}";
            return GetAsync<List<VehiculoLookupDto>>(query) ?? Task.FromResult(new List<VehiculoLookupDto>());
        }

        public async Task<(bool ok, string? error, int vehiculoId)> SaveVehiculoAsync(VehiculoFormDto form)
        {
            var body = new
            {
                form.ClienteId,
                Placa = form.Placa.Trim().ToUpper(CultureInfo.InvariantCulture),
                form.MarcaId,
                form.ModeloId,
                form.ColorVehiculoId,
                form.Anio
            };

            HttpResponseMessage response;
            if (form.VehiculoId == 0)
                response = await SendAsync(HttpMethod.Post, "api/vehiculos", body);
            else
                response = await SendAsync(HttpMethod.Put, $"api/vehiculos/{form.VehiculoId}", body);

            if (!response.IsSuccessStatusCode)
                return (false, await ReadErrorAsync(response), 0);

            if (form.VehiculoId == 0)
            {
                var result = await DeserializeAsync<JsonElement>(response);
                var newId = result.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : 0;
                return (true, null, newId);
            }
            return (true, null, form.VehiculoId);
        }

        public Task DeleteVehiculoAsync(int id) => SendAsync(HttpMethod.Delete, $"api/vehiculos/{id}");

        // ─── Catálogos ────────────────────────────────────────────────────────

        public Task<List<MarcaDto>> GetAllMarcasAsync()
            => GetAsync<List<MarcaDto>>("api/marcas") ?? Task.FromResult(new List<MarcaDto>());

        public Task<List<ModeloDto>> GetAllModelosAsync()
            => GetAsync<List<ModeloDto>>("api/modelos") ?? Task.FromResult(new List<ModeloDto>());

        public Task<List<ColorVehiculoDto>> GetAllColoresAsync()
            => GetAsync<List<ColorVehiculoDto>>("api/coloresvehiculo") ?? Task.FromResult(new List<ColorVehiculoDto>());

        // ─── Helpers privados ─────────────────────────────────────────────────

        private async Task<T?> GetAsync<T>(string url)
        {
            var request = BuildRequest(HttpMethod.Get, url);
            var response = await _http.SendAsync(request);
            if (!response.IsSuccessStatusCode) return default;
            return await DeserializeAsync<T>(response);
        }

        private Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, object? body = null)
        {
            var request = BuildRequest(method, url, body);
            return _http.SendAsync(request);
        }

        private HttpRequestMessage BuildRequest(HttpMethod method, string url, object? body = null)
        {
            var request = new HttpRequestMessage(method, url);

            var token = _ctx.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (body is not null)
                request.Content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json");

            return request;
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
                var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("error", out var errProp))
                    return errProp.GetString() ?? "Error desconocido.";
            }
            catch { }
            return $"Error HTTP {(int)response.StatusCode}.";
        }
    }
}
