using Taller_Mecanico_Users.Domain.Entities;

namespace Taller_Mecanico_Users.Application.Services
{
    /// <summary>
    /// Servicio de generación de JWT tokens.
    /// Responsabilidad: Crear tokens JWT con los claims apropiados.
    /// Inyectable - Encapsula la lógica de token generation.
    /// </summary>
    public interface IJwtTokenGenerator
    {
        /// <summary>
        /// Genera un JWT token para un usuario autenticado.
        /// Claims incluyen: ID, Email, Rol, EmpleadoId/ClienteId, RequiereCambioPassword
        /// </summary>
        string GenerateToken(UsuarioLogin usuario);
    }
}

