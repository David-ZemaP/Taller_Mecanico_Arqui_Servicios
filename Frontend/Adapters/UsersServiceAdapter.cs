using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Taller_Mecanico_Arqui.Frontend.Adapters;

/// <summary>
/// Adapter que consume la API REST del UsersService (puerto 5001).
/// Abstrae todas las llamadas HTTP, incluyendo parsing de JWT para claims.
/// </summary>
public class UsersServiceAdapter : IUsersServiceAdapter
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersServiceAdapter> _logger;

    public UsersServiceAdapter(HttpClient httpClient, ILogger<UsersServiceAdapter> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    // ==========================================
    // AUTHENTICATION
    // ==========================================

    public async Task<AuthResponse?> LoginAsync(string email, string password)
    {
        try
        {
            var request = new { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("LoginAsync falló: {StatusCode}", response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<LoginApiResponse>();
            if (result == null) return null;

            // Parsear el JWT para extraer claims
            var authResponse = new AuthResponse
            {
                Token = result.Token,
                RequiereCambioPassword = result.RequiereCambioPassword,
                EsCliente = result.EsCliente
            };

            // Extraer claims del token JWT
            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(result.Token))
                {
                    var jwt = handler.ReadJwtToken(result.Token);
                    authResponse.UserId = int.TryParse(jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : null;
                    authResponse.EmpleadoId = int.TryParse(jwt.Claims.FirstOrDefault(c => c.Type == "EmpleadoId")?.Value, out var eid) ? eid : null;
                    authResponse.ClienteId = int.TryParse(jwt.Claims.FirstOrDefault(c => c.Type == "ClienteId")?.Value, out var cid) ? cid : null;
                    authResponse.NivelAcceso = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudieron extraer claims del JWT");
            }

            return authResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en LoginAsync");
            return null;
        }
    }

    // ==========================================
    // USER MANAGEMENT
    // ==========================================

    public async Task<List<UserListDto>> GetAllUsersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/users");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetAllUsersAsync falló: {StatusCode}", response.StatusCode);
                return new List<UserListDto>();
            }

            var result = await response.Content.ReadFromJsonAsync<List<UserApiDto>>();
            if (result == null) return new List<UserListDto>();

            return result.Select(MapToListDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetAllUsersAsync");
            return new List<UserListDto>();
        }
    }

    public async Task<UserDetailDto?> GetUserByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/users/{id}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("GetUserByIdAsync({Id}) falló: {StatusCode}", id, response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<UserApiDto>();
            if (result == null) return null;

            return MapToDetailDto(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en GetUserByIdAsync({Id})", id);
            return null;
        }
    }

    public async Task<(bool Success, int? UserId, string? Error)> CreateUserAsync(CreateUserDto dto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/users", dto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("CreateUserAsync falló: {StatusCode} - {Error}", response.StatusCode, error);
                return (false, null, error);
            }

            var result = await response.Content.ReadFromJsonAsync<UserCreatedResponse>();
            return (true, result?.UsuarioLoginId, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en CreateUserAsync");
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> UpdateUserAsync(UpdateUserDto dto)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/users/{dto.UsuarioLoginId}", dto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("UpdateUserAsync falló: {StatusCode} - {Error}", response.StatusCode, error);
                return (false, error);
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en UpdateUserAsync");
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> DeleteUserAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/users/{id}");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("DeleteUserAsync({Id}) falló: {StatusCode} - {Error}", id, response.StatusCode, error);
                return (false, error);
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en DeleteUserAsync({Id})", id);
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> ToggleActivoAsync(int id, bool activo)
    {
        try
        {
            // El endpoint PUT api/users/{id} acepta { email, activo }
            // Necesitamos primero el email actual para no cambiarlo
            var user = await GetUserByIdAsync(id);
            if (user == null)
            {
                return (false, "Usuario no encontrado.");
            }

            var response = await _httpClient.PutAsJsonAsync($"api/users/{id}", new UpdateUserDto
            {
                UsuarioLoginId = id,
                Email = user.Email,
                Activo = activo
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("ToggleActivoAsync({Id}, {Activo}) falló: {StatusCode} - {Error}", id, activo, response.StatusCode, error);
                return (false, error);
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ToggleActivoAsync({Id}, {Activo})", id, activo);
            return (false, ex.Message);
        }
    }

    // ==========================================
    // PASSWORD MANAGEMENT
    // ==========================================

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(int userId, string currentPassword, string newPassword, string confirmPassword)
    {
        try
        {
            var request = new
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
                ConfirmPassword = confirmPassword
            };

            var response = await _httpClient.PostAsJsonAsync($"api/users/{userId}/change-password", request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("ChangePasswordAsync({UserId}) falló: {StatusCode} - {Error}", userId, response.StatusCode, error);
                return (false, error);
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ChangePasswordAsync({UserId})", userId);
            return (false, ex.Message);
        }
    }

    public async Task<(bool Success, string? Error)> ResetPasswordAsync(int userId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/users/{userId}/reset-password", null);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("ResetPasswordAsync({UserId}) falló: {StatusCode} - {Error}", userId, response.StatusCode, error);
                return (false, error);
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en ResetPasswordAsync({UserId})", userId);
            return (false, ex.Message);
        }
    }

    // ==========================================
    // MAPPINGS
    // ==========================================

    private static UserListDto MapToListDto(UserApiDto dto) => new()
    {
        UsuarioLoginId = dto.UsuarioLoginId,
        EmpleadoId = dto.EmpleadoId,
        ClienteId = dto.ClienteId,
        Email = dto.Email,
        UltimoAcceso = dto.UltimoAcceso,
        Activo = dto.Activo,
        RequiereCambioPassword = dto.RequiereCambioPassword,
        EsCliente = dto.EsCliente
    };

    private static UserDetailDto MapToDetailDto(UserApiDto dto) => new()
    {
        UsuarioLoginId = dto.UsuarioLoginId,
        EmpleadoId = dto.EmpleadoId,
        ClienteId = dto.ClienteId,
        Email = dto.Email,
        UltimoAcceso = dto.UltimoAcceso,
        Activo = dto.Activo,
        RequiereCambioPassword = dto.RequiereCambioPassword,
        EsCliente = dto.EsCliente
    };

    // ==========================================
    // INTERNAL DTOs (respuestas de la API)
    // ==========================================

    private class LoginApiResponse
    {
        public string Token { get; set; } = string.Empty;
        public bool RequiereCambioPassword { get; set; }
        public bool EsCliente { get; set; }
    }

    private class UserApiDto
    {
        public int UsuarioLoginId { get; set; }
        public int? EmpleadoId { get; set; }
        public int? ClienteId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime? UltimoAcceso { get; set; }
        public bool Activo { get; set; }
        public bool RequiereCambioPassword { get; set; }
        public bool EsCliente { get; set; }
    }

    private class UserCreatedResponse
    {
        public int UsuarioLoginId { get; set; }
    }
}