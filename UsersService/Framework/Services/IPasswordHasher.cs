namespace Taller_Mecanico_Users.Framework.Services
{
    /// <summary>
    /// Servicio de hash de contraseñas.
    /// Responsabilidad: Encapsular la implementación de hashing (BCrypt, Argon2, etc).
    /// Inyectable - Permite cambiar algoritmo de hashing sin afectar UseCases.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Genera un hash seguro de la contraseña.
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// Verifica que una contraseña coincida con su hash.
        /// </summary>
        bool VerifyPassword(string password, string hash);
    }
}
