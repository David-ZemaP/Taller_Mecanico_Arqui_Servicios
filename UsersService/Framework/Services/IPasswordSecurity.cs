using Taller_Mecanico_Users.Domain.Common;

namespace Taller_Mecanico_Users.Framework.Services
{
    /// <summary>
    /// Servicio de seguridad de contraseñas.
    /// Responsabilidad: Validar y generar contraseñas seguras.
    /// Inyectable y testeable - Encapsula la lógica de seguridad de passwords.
    /// </summary>
    public interface IPasswordSecurity
    {
        /// <summary>
        /// Valida que una contraseña cumpla con los requisitos de seguridad:
        /// - Mínimo 8 caracteres
        /// - Al menos 1 MAYÚSCULA
        /// - Al menos 1 minúscula
        /// - Al menos 1 número
        /// - Al menos 1 carácter especial
        /// </summary>
        Result ValidatePassword(string? password);

        /// <summary>
        /// Genera una contraseña temporal segura.
        /// Garantiza que incluye todas las categorías requeridas.
        /// </summary>
        string GenerateSecurePassword(int length = 12);
    }
}
