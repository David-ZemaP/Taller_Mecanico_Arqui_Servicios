using Microsoft.AspNetCore.Mvc;
using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.UseCases.Users;

namespace Taller_Mecanico_Users.Controllers
{
    /// <summary>
    /// Controller: Autenticación
    /// 
    /// Responsabilidad ÚNICA: Mapear requests HTTP → UseCases → Responses HTTP
    /// NO contiene lógica de negocio (está en LoginUseCase)
    /// 
    /// ANTES: Generaba JWT, accedía a IConfiguration, validaba credenciales
    /// AHORA: Solo orquesta el UseCase y retorna respuestas
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly LoginUseCase _loginUseCase;

        public AuthController(LoginUseCase loginUseCase)
        {
            _loginUseCase = loginUseCase;
        }

        /// <summary>
        /// POST /api/auth/login
        /// Autentica usuario y genera JWT token.
        /// 
        /// Request:
        /// {
        ///   "email": "usuario@example.com",
        ///   "password": "Password123!"
        /// }
        /// 
        /// Response (200 OK):
        /// {
        ///   "token": "eyJhbGc...",
        ///   "requiereCambioPassword": true,
        ///   "esCliente": false
        /// }
        /// 
        /// Response (401 Unauthorized):
        /// {
        ///   "message": "Credenciales inválidas."
        /// }
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _loginUseCase.ExecuteAsync(request.Email, request.Password);
            
            if (result.IsFailure)
            {
                if (result.ErrorCode == ErrorCodes.ValidationRequired || result.ErrorCode == ErrorCodes.ValidationInvalidValue)
                {
                    return BadRequest(new
                    {
                        code = result.ErrorCode,
                        message = result.ErrorMessage ?? "Solicitud inválida."
                    });
                }

                return Unauthorized(new
                {
                    code = result.ErrorCode ?? ErrorCodes.ValidationInvalidValue,
                    message = result.ErrorMessage ?? "Credenciales inválidas."
                });
            }

            return Ok(result.Value);
        }
    }

    /// <summary>
    /// DTO: Request de login
    /// </summary>
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
