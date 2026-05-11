using System.Security.Cryptography;
using Taller_Mecanico_Users.Domain.Common;

namespace Taller_Mecanico_Users.Application.Services
{
    /// <summary>
    /// Implementación concreta del servicio de seguridad de contraseñas.
    /// Encapsula la lógica de validación y generación de passwords.
    /// Puede ser inyectada en UseCases y testeable.
    /// </summary>
    public class PasswordSecurityService : IPasswordSecurity, Taller_Mecanico_Users.Domain.Ports.IPasswordSecurity
    {
        private const int MinimumLength = 8;

        public Result ValidatePassword(string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return Result.Failure(ErrorCodes.ValidationRequired, "La contraseña es obligatoria.");
            }

            if (password.Length < MinimumLength)
            {
                return Result.Failure(ErrorCodes.ValidationInvalidValue, "La contraseña debe tener al menos 8 caracteres.");
            }

            if (!password.Any(char.IsUpper))
            {
                return Result.Failure(ErrorCodes.ValidationInvalidValue, "La contraseña debe incluir al menos una mayúscula.");
            }

            if (!password.Any(char.IsLower))
            {
                return Result.Failure(ErrorCodes.ValidationInvalidValue, "La contraseña debe incluir al menos una minúscula.");
            }

            if (!password.Any(char.IsDigit))
            {
                return Result.Failure(ErrorCodes.ValidationInvalidValue, "La contraseña debe incluir al menos un número.");
            }

            const string specialChars = "!@#$%^&*()-_=+[]{};:,.?/";
            if (!password.Any(character => specialChars.Contains(character)))
            {
                return Result.Failure(ErrorCodes.ValidationInvalidValue, "La contraseña debe incluir al menos un carácter especial.");
            }

            return Result.Success();
        }

        public string GenerateSecurePassword(int length = 12)
        {
            if (length < MinimumLength)
            {
                length = MinimumLength;
            }

            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string numbers = "0123456789";
            const string specialChars = "!@#$%^&*()-_=+[]{};:,.?/";
            const string allChars = uppercase + lowercase + numbers + specialChars;

            var password = new char[length];
            password[0] = GetRandomChar(uppercase);
            password[1] = GetRandomChar(lowercase);
            password[2] = GetRandomChar(numbers);
            password[3] = GetRandomChar(specialChars);

            for (int index = 4; index < password.Length; index++)
            {
                password[index] = GetRandomChar(allChars);
            }

            Shuffle(password);
            return new string(password);
        }

        private static char GetRandomChar(string characters)
        {
            var index = RandomNumberGenerator.GetInt32(characters.Length);
            return characters[index];
        }

        private static void Shuffle(char[] values)
        {
            for (int index = values.Length - 1; index > 0; index--)
            {
                var swapIndex = RandomNumberGenerator.GetInt32(index + 1);
                (values[index], values[swapIndex]) = (values[swapIndex], values[index]);
            }
        }
    }
}

