using Taller_Mecanico_Users.Domain.Common;

namespace Taller_Mecanico_Users.Domain.Ports
{
    public interface IPasswordSecurity
    {
        Result ValidatePassword(string? password);
        string GenerateSecurePassword(int length = 12);
    }
}
