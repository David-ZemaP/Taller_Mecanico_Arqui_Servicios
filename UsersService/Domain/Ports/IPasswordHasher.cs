namespace Taller_Mecanico_Users.Domain.Ports
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}
