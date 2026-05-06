using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Taller_Mecanico_Users.Domain.Ports;

namespace Taller_Mecanico_Users.App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioLoginRepository _loginRepository;
        private readonly IConfiguration _configuration;

        public AuthController(IUsuarioLoginRepository loginRepository, IConfiguration configuration)
        {
            _loginRepository = loginRepository;
            _configuration = configuration; // Inyectamos configuración para leer el Secret
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var usuario = await _loginRepository.GetByEmailAsync(request.Email);
            
            // Validamos que exista y esté activo
            if (usuario == null || !usuario.Activo)
                return Unauthorized(new { message = "Credenciales inválidas o usuario inactivo." });

            if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
                return Unauthorized(new { message = "Credenciales inválidas." });

            usuario.RegistrarAcceso();
            await _loginRepository.UpdateAsync(usuario);

            // Generamos el Token JWT
            var token = GenerateJwtToken(usuario);

            return Ok(new 
            { 
                Token = token,
                RequiereCambioPassword = usuario.RequiereCambioPassword,
                EsCliente = usuario.EsCliente
            });
        }

        [HttpPost("test-token")]
        public IActionResult GetTestToken()
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "999"),
                new Claim(ClaimTypes.Email, "test@empleado.com"),
                new Claim("RequiereCambio", "false"),
                new Claim(ClaimTypes.Role, "Empleado")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(120),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { token = tokenHandler.WriteToken(token) });
        }

        private string GenerateJwtToken(Domain.Entities.UsuarioLogin usuario)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

            // Aquí metemos información del usuario dentro del token (Claims)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioLoginId.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim("RequiereCambio", usuario.RequiereCambioPassword.ToString()),
                new Claim(ClaimTypes.Role, usuario.EsCliente ? "Cliente" : "Empleado")
            };

            if (usuario.EmpleadoId.HasValue)
            {
                claims.Add(new Claim("EmpleadoId", usuario.EmpleadoId.Value.ToString()));
            }

            if (usuario.ClienteId.HasValue)
            {
                claims.Add(new Claim("ClienteId", usuario.ClienteId.Value.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpirationInMinutes"]!)),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}