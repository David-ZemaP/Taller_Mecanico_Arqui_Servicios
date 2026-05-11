namespace Taller_Mecanico_Users.Application.Services
{
    /// <summary>
    /// Implementación de IPasswordHasher usando BCrypt.
    /// Encapsula los detalles de la librería BCrypt.Net.
    /// Si en el futuro se requiere cambiar a Argon2, solo se modifica esta clase.
    /// </summary>
    public class BcryptPasswordHasher : IPasswordHasher, Taller_Mecanico_Users.Domain.Ports.IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}

