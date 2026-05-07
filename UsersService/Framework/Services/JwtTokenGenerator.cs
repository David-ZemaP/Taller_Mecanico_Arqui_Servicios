using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Taller_Mecanico_Users.Domain.Entities;

namespace Taller_Mecanico_Users.Framework.Services
{
    /// <summary>
    /// Implementación concreta de IJwtTokenGenerator.
    /// Encapsula la lógica de creación de JWT tokens.
    /// </summary>
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IJwtSettings _jwtSettings;

        public JwtTokenGenerator(IJwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
        }

        public string GenerateToken(UsuarioLogin usuario)
        {
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

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

            // Incluir nivelacceso si está disponible
            if (!string.IsNullOrEmpty(usuario.NivelAcceso))
            {
                claims.Add(new Claim("NivelAcceso", usuario.NivelAcceso));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
