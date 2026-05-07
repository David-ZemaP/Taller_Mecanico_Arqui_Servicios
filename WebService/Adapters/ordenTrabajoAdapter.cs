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

        public async Task<List<OrdenTrabajoListDto>> GetAllOrdenesAsync()
            => await GetAsync<List<OrdenTrabajoListDto>>("api/ordenestrabajo") ?? new List<OrdenTrabajoListDto>();

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

        // ─── Clientes ─────────────────────────────────────────────────────────

        public async Task<List<ClienteLookupDto>> BuscarClientesAsync(string term)
        {
            var query = $"api/clientes?term={Uri.EscapeDataString(term)}";
            var result = await GetAsync<List<ClienteLookupDto>>(query);
            return result ?? new List<ClienteLookupDto>();
        }

        public async Task<ClienteLookupDto?> GetClienteAsync(int id)
            => await GetAsync<ClienteLookupDto>($"api/clientes/{id}");

        public async Task<(bool ok, string? error, ClienteLookupDto? cliente)> SaveClienteAsync(ClienteFormDto form)
        {
            HttpResponseMessage response;
            if (form.ClienteId == 0)
            {
                response = await SendAsync(HttpMethod.Post, "api/clientes", new
                {
                    nombres = form.Nombres,
                    primerApellido = form.PrimerApellido,
                    segundoApellido = form.SegundoApellido,
                    ciNumero = form.CiNumero,
                    ciComplemento = form.CiComplemento,
                    telefono = form.Telefono,
                    email = form.Email
                });
            }
            else
            {
                response = await SendAsync(HttpMethod.Put, $"api/clientes/{form.ClienteId}", new
                {
                    nombres = form.Nombres,
                    primerApellido = form.PrimerApellido,
                    segundoApellido = form.SegundoApellido,
                    ciNumero = form.CiNumero,
                    ciComplemento = form.CiComplemento,
                    telefono = form.Telefono,
                    email = form.Email
                });
            }

            if (!response.IsSuccessStatusCode)
                return (false, await ReadErrorAsync(response), null);

            ClienteLookupDto cliente;
            if (form.ClienteId == 0)
            {
                var created = await DeserializeAsync<ClienteLookupDto>(response);
                cliente = created ?? new ClienteLookupDto { ClienteId = 0, Nombres = form.Nombres, PrimerApellido = form.PrimerApellido, CiNumero = form.CiNumero };
            }
            else
            {
                cliente = new ClienteLookupDto
                {
                    ClienteId = form.ClienteId,
                    Nombres = form.Nombres,
                    PrimerApellido = form.PrimerApellido,
                    SegundoApellido = form.SegundoApellido,
                    CiNumero = form.CiNumero,
                    CiComplemento = form.CiComplemento,
                    Telefono = form.Telefono,
                    Email = form.Email
                };
            }

            return (true, null, cliente);
        }

        // ─── Productos ────────────────────────────────────────────────────────

        public async Task<List<ProductoDto>> GetAllProductosAsync()
            => await GetAsync<List<ProductoDto>>("api/productos") ?? new List<ProductoDto>();

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

        public async Task<List<ServicioDto>> GetAllServiciosAsync()
            => await GetAsync<List<ServicioDto>>("api/servicios") ?? new List<ServicioDto>();

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

        public async Task<List<VehiculoListDto>> GetAllVehiculosAsync()
            => await GetAsync<List<VehiculoListDto>>("api/vehiculos") ?? new List<VehiculoListDto>();

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

        public async Task<List<MarcaDto>> GetAllMarcasAsync()
            => await GetAsync<List<MarcaDto>>("api/marcas") ?? new List<MarcaDto>();

        public async Task<List<ModeloDto>> GetAllModelosAsync()
            => await GetAsync<List<ModeloDto>>("api/modelos") ?? new List<ModeloDto>();

        public async Task<List<ColorVehiculoDto>> GetAllColoresAsync()
            => await GetAsync<List<ColorVehiculoDto>>("api/coloresvehiculo") ?? new List<ColorVehiculoDto>();

        // ─── Helpers privados ─────────────────────────────────────────────────

        private async Task<T?> GetAsync<T>(string url)
        {
            try
            {
                var request = BuildRequest(HttpMethod.Get, url);
                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode) return default;
                return await DeserializeAsync<T>(response);
            }
            catch
            {
                return default;
            }
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
            if (string.IsNullOrWhiteSpace(token))
                token = _ctx.HttpContext?.User.FindFirst("JwtToken")?.Value;
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
