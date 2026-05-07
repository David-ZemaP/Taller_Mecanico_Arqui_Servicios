using System.Net.Http.Json;
using System.Text.Json;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Ports;

namespace OrdenTrabajoService.Infrastructure.Clients
{
    public class UsersServiceClient : IUsersServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UsersServiceClient> _logger;

        public UsersServiceClient(HttpClient httpClient, ILogger<UsersServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<Result<int>> CreateUsuarioForClienteAsync(int clienteId, string email)
        {
            var request = new CreateUserRequest
            {
                ClienteId = clienteId,
                Email = email
            };

            return await SendCreateUserAsync(request, "cliente");
        }

        public async Task<Result<int>> CreateUsuarioForEmpleadoAsync(int empleadoId, string email)
        {
            var request = new CreateUserRequest
            {
                EmpleadoId = empleadoId,
                Email = email
            };

            return await SendCreateUserAsync(request, "empleado");
        }

        private async Task<Result<int>> SendCreateUserAsync(CreateUserRequest request, string entityType)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/users", request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("UsersService returned {StatusCode} for {entityType}: {ErrorBody}",
                        response.StatusCode, entityType, errorBody);

                    return Result<int>.Failure(
                        ErrorCodes.ExternalServiceError,
                        $"Error al crear usuario para {entityType} en UsersService: {response.StatusCode}");
                }

                var created = await response.Content.ReadFromJsonAsync<CreateUserResponse>();
                if (created == null || created.UsuarioLoginId <= 0)
                {
                    return Result<int>.Failure(
                        ErrorCodes.ExternalServiceError,
                        $"UsersService returned invalid response for {entityType}.");
                }

                return Result<int>.Success(created.UsuarioLoginId);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to UsersService for {entityType}", entityType);
                return Result<int>.Failure(
                    ErrorCodes.ExternalServiceUnavailable,
                    $"No se pudo conectar al servicio de usuarios: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling UsersService for {entityType}", entityType);
                return Result<int>.Failure(
                    ErrorCodes.ExternalServiceError,
                    $"Error inesperado al comunicar con UsersService: {ex.Message}");
            }
        }

        private class CreateUserRequest
        {
            [System.Text.Json.Serialization.JsonPropertyName("empleadoId")]
            public int? EmpleadoId { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("clienteId")]
            public int? ClienteId { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("email")]
            public string Email { get; set; } = string.Empty;
        }

        private class CreateUserResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("usuarioLoginId")]
            public int UsuarioLoginId { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("email")]
            public string? Email { get; set; }
        }
    }
}
