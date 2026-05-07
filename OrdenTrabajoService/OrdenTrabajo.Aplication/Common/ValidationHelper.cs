using System.Text.RegularExpressions;
using Taller_Mecanico_Arqui.Domain.Common;
using Taller_Mecanico_Arqui.Domain.Enums;

namespace Taller_Mecanico_Arqui.Application.Common
{
    /// <summary>
    /// Provides common validation methods for domain operations.
    /// </summary>
    public static class ValidationHelper
    {
        #region Basic Validation

        /// <summary>
        /// Validates a condition and returns success or failure.
        /// </summary>
        public static Result Require(bool condition, string errorCode, string errorMessage)
            => condition ? Result.Success() : Result.Failure(errorCode, errorMessage);

        /// <summary>
        /// Validates that a reference type is not null.
        /// </summary>
        public static Result RequireNotNull<T>(T? value, string errorCode, string errorMessage)
            where T : class
            => value is null ? Result.Failure(errorCode, errorMessage) : Result.Success();

        /// <summary>
        /// Validates that a value type has a value.
        /// </summary>
        public static Result RequireNotNull<T>(T? value, string errorCode, string errorMessage)
            where T : struct
            => value is null ? Result.Failure(errorCode, errorMessage) : Result.Success();

        /// <summary>
        /// Validates that a string is not null or empty.
        /// </summary>
        public static Result RequireNotEmpty(string? value, string errorCode, string errorMessage)
            => string.IsNullOrWhiteSpace(value) 
                ? Result.Failure(errorCode, errorMessage) 
                : Result.Success();

        /// <summary>
        /// Validates that a string meets minimum length.
        /// </summary>
        public static Result RequireMinLength(string? value, int minLength, string errorCode, string errorMessage)
            => string.IsNullOrWhiteSpace(value) || value.Length < minLength
                ? Result.Failure(errorCode, errorMessage)
                : Result.Success();

        /// <summary>
        /// Validates that a string meets maximum length.
        /// </summary>
        public static Result RequireMaxLength(string? value, int maxLength, string errorCode, string errorMessage)
            => value?.Length > maxLength
                ? Result.Failure(errorCode, errorMessage)
                : Result.Success();

        /// <summary>
        /// Validates that a value is within range.
        /// </summary>
        public static Result RequireInRange(int value, int min, int max, string errorCode, string errorMessage)
            => value < min || value > max
                ? Result.Failure(errorCode, errorMessage)
                : Result.Success();

        /// <summary>
        /// Validates that a value is positive.
        /// </summary>
        public static Result RequirePositive(decimal value, string errorCode, string errorMessage)
            => value <= 0
                ? Result.Failure(errorCode, errorMessage)
                : Result.Success();

        /// <summary>
        /// Validates that a value is non-negative.
        /// </summary>
        public static Result RequireNonNegative(decimal value, string errorCode, string errorMessage)
            => value < 0
                ? Result.Failure(errorCode, errorMessage)
                : Result.Success();

        #endregion

        #region Email Validation

        private static readonly Regex EmailRegex = new(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Validates an email address format.
        /// </summary>
        public static Result<string> ValidateEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result<string>.Failure(ErrorCodes.ValidationRequired, "El correo electrónico es obligatorio.");
            
            if (!EmailRegex.IsMatch(email))
                return Result<string>.Failure(ErrorCodes.ValidationInvalidValue, "El formato del correo electrónico es inválido.");
            
            return Result<string>.Success(email.Trim().ToLowerInvariant());
        }

        /// <summary>
        /// Validates an optional email address (returns success if empty).
        /// </summary>
        public static Result<string> ValidateEmailOptional(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result<string>.Success(string.Empty);
            
            if (!EmailRegex.IsMatch(email))
                return Result<string>.Failure(ErrorCodes.ValidationInvalidValue, "El formato del correo electrónico es inválido.");
            
            return Result<string>.Success(email.Trim().ToLowerInvariant());
        }

        #endregion

        #region Phone Validation

        private static readonly Regex PhoneRegex = new(
            @"^9\d{7}$",
            RegexOptions.Compiled);

        /// <summary>
        /// Validates a phone number (Uruguay format: 9 followed by 7 digits).
        /// </summary>
        public static Result<int> ValidatePhone(int phone)
        {
            if (phone < 900000000 || phone > 999999999)
                return Result<int>.Failure(ErrorCodes.ValidationInvalidValue, "El teléfono debe tener 9 dígitos y comenzar con 9.");
            
            return Result<int>.Success(phone);
        }

        /// <summary>
        /// Validates an optional phone number.
        /// </summary>
        public static Result<int?> ValidatePhoneOptional(int? phone)
        {
            if (!phone.HasValue)
                return Result<int?>.Success(null);
            
            return ValidatePhone(phone.Value).Map(x => (int?)x);
        }

        #endregion

        #region Plate Validation

        private static readonly Regex PlateRegex = new(
            @"^[A-Z]{1,3}\d{3,4}$",
            RegexOptions.Compiled);

        /// <summary>
        /// Validates a vehicle plate format.
        /// </summary>
        public static Result<string> ValidatePlate(string? plate)
        {
            if (string.IsNullOrWhiteSpace(plate))
                return Result<string>.Failure(ErrorCodes.ValidationRequired, "La placa es obligatoria.");
            
            var normalized = NormalizePlate(plate);
            if (!PlateRegex.IsMatch(normalized))
                return Result<string>.Failure(ErrorCodes.ValidationInvalidValue, "El formato de placa es inválido (ej: ABC123).");
            
            return Result<string>.Success(normalized);
        }

        /// <summary>
        /// Normalizes a plate to uppercase without spaces.
        /// </summary>
        public static string NormalizePlate(string? placa)
            => (placa ?? string.Empty).Trim().ToUpperInvariant();

        /// <summary>
        /// Validates plate availability (not duplicate).
        /// </summary>
        public static Result ValidatePlateAvailable(bool isDuplicate)
            => Require(!isDuplicate, ErrorCodes.ValidationInvalidValue, "Esta placa ya está registrada en el sistema.");

        #endregion

        #region CI (Identity Document) Validation

        /// <summary>
        /// Validates a CI number (Uruguay: 6-8 digits).
        /// </summary>
        public static Result<int> ValidateCiNumber(int ciNumber)
        {
            if (ciNumber < 100000 || ciNumber > 99999999)
                return Result<int>.Failure(ErrorCodes.ValidationInvalidValue, "CI debe tener entre 6 y 8 dígitos.");
            
            return Result<int>.Success(ciNumber);
        }

        /// <summary>
        /// Validates a CI complement (optional, format: digit + uppercase letter).
        /// </summary>
        public static Result<string?> ValidateCiComplement(string? complement)
        {
            if (string.IsNullOrWhiteSpace(complement))
                return Result<string?>.Success(null);
            
            if (!Regex.IsMatch(complement, @"^\d[a-zA-Z]$"))
                return Result<string?>.Failure(ErrorCodes.ValidationInvalidValue, "Formato de complemento inválido (Ej: 1G).");
            
            return Result<string?>.Success(complement.ToUpperInvariant());
        }

        #endregion

        #region Enum Parsing

        /// <summary>
        /// Parses a string to an enum value.
        /// </summary>
        public static Result<TEnum> ParseEnum<TEnum>(string? rawValue, string errorMessage, bool removeSpaces = false)
            where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return Result<TEnum>.Failure(ErrorCodes.ValidationInvalidValue, errorMessage);

            var candidate = removeSpaces ? rawValue.Replace(" ", string.Empty) : rawValue.Trim();
            if (Enum.TryParse<TEnum>(candidate, ignoreCase: true, out var parsed))
                return Result<TEnum>.Success(parsed);

            return Result<TEnum>.Failure(ErrorCodes.ValidationInvalidValue, errorMessage);
        }

        /// <summary>
        /// Parses a string to NivelAcceso enum.
        /// </summary>
        public static Result<NivelAcceso> ParseNivelAcceso(string? rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return Result<NivelAcceso>.Failure(ErrorCodes.ValidationInvalidAccessLevel, "Nivel de acceso no válido.");

            var candidate = rawValue.Trim();
            if (candidate.Equals("Total", StringComparison.OrdinalIgnoreCase))
                candidate = NivelAcceso.Completo.ToString();

            if (Enum.TryParse<NivelAcceso>(candidate, ignoreCase: true, out var parsed))
                return Result<NivelAcceso>.Success(parsed);

            return Result<NivelAcceso>.Failure(ErrorCodes.ValidationInvalidAccessLevel, "Nivel de acceso no válido.");
        }

        /// <summary>
        /// Parses a string to TipoCliente enum.
        /// </summary>
        public static Result<TipoCliente> ParseTipoCliente(string? rawValue)
            => ParseEnum<TipoCliente>(rawValue, "Tipo de cliente no válido.");

        /// <summary>
        /// Parses a string to EstadoLaboral enum.
        /// </summary>
        public static Result<EstadoLaboral> ParseEstadoLaboral(string? rawValue)
            => ParseEnum<EstadoLaboral>(rawValue, "Estado laboral no válido.");

        /// <summary>
        /// Parses a string to EstadoTrabajo enum.
        /// </summary>
        public static Result<EstadoTrabajo> ParseEstadoTrabajo(string? rawValue)
            => ParseEnum<EstadoTrabajo>(rawValue, "Estado de trabajo no válido.", removeSpaces: true);

        /// <summary>
        /// Parses a string to EstadoPago enum.
        /// </summary>
        public static Result<EstadoPago> ParseEstadoPago(string? rawValue)
            => ParseEnum<EstadoPago>(rawValue, "Estado de pago no válido.");

        #endregion

        #region Admin Validation

        /// <summary>
        /// Validates if an admin can be created based on access level.
        /// </summary>
        public static Result RequireCanCreateAdmin(bool allowed, NivelAcceso nivelAcceso)
            => allowed
                ? Result.Success()
                : Result.Failure(ErrorCodes.ValidationInvalidAccessLevel, $"No tienes permisos para crear administradores con nivel {nivelAcceso}.");

        /// <summary>
        /// Validates if an admin can be modified based on access level.
        /// </summary>
        public static Result RequireCanModifyAdmin(bool allowed, NivelAcceso nivelAcceso)
            => allowed
                ? Result.Success()
                : Result.Failure(ErrorCodes.ValidationInvalidAccessLevel, $"No tienes permisos para modificar administradores con nivel {nivelAcceso}.");

        /// <summary>
        /// Validates that admins have an email address.
        /// </summary>
        public static Result ValidateAdminEmail(string tipoEmpleado, string? email)
        {
            if (tipoEmpleado.Equals("Administrador", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(email))
                return Result.Failure(ErrorCodes.ValidationAdminEmailRequired, "El correo electrónico es obligatorio para administradores.");

            return Result.Success();
        }

        #endregion

        #region Access Level Validation

        /// <summary>
        /// Validates that the current user has a configured access level.
        /// </summary>
        public static Result ValidateAccessLevelConfigured(NivelAcceso? currentUserLevel)
            => currentUserLevel.HasValue
                ? Result.Success()
                : Result.Failure(ErrorCodes.ValidationInvalidValue, "No se pudo determinar el nivel de acceso del usuario actual.");

        /// <summary>
        /// Validates if the user has sufficient access level for an operation.
        /// </summary>
        public static Result ValidateAccessLevel(NivelAcceso? currentLevel, NivelAcceso requiredLevel, string errorMessage)
        {
            if (!currentLevel.HasValue)
                return Result.Failure(ErrorCodes.ValidationInvalidAccessLevel, "No se pudo determinar el nivel de acceso.");
            
            if ((int)currentLevel.Value < (int)requiredLevel)
                return Result.Failure(ErrorCodes.ValidationInvalidAccessLevel, errorMessage);
            
            return Result.Success();
        }

        #endregion

        #region Date Validation

        /// <summary>
        /// Validates that a date is not in the future.
        /// </summary>
        public static Result ValidateDateNotFuture(DateTime date, string errorMessage)
            => date > DateTime.UtcNow
                ? Result.Failure(ErrorCodes.ValidationInvalidValue, errorMessage)
                : Result.Success();

        /// <summary>
        /// Validates that a date is not in the past.
        /// </summary>
        public static Result ValidateDateNotPast(DateTime date, string errorMessage)
            => date < DateTime.UtcNow
                ? Result.Failure(ErrorCodes.ValidationInvalidValue, errorMessage)
                : Result.Success();

        /// <summary>
        /// Validates a date range is valid (start before end).
        /// </summary>
        public static Result ValidateDateRange(DateTime startDate, DateTime endDate, string errorMessage)
            => startDate > endDate
                ? Result.Failure(ErrorCodes.ValidationInvalidValue, errorMessage)
                : Result.Success();

        /// <summary>
        /// Validates that a year is within a valid range.
        /// </summary>
        public static Result ValidateYear(int year, int minYear, int maxYear, string errorMessage)
            => year < minYear || year > maxYear
                ? Result.Failure(ErrorCodes.ValidationInvalidValue, errorMessage)
                : Result.Success();

        #endregion

        #region Collection Validation

        /// <summary>
        /// Validates that a collection is not empty.
        /// </summary>
        public static Result ValidateNotEmpty<T>(IEnumerable<T>? collection, string errorCode, string errorMessage)
            => collection is null || !collection.Any()
                ? Result.Failure(errorCode, errorMessage)
                : Result.Success();

        /// <summary>
        /// Validates the count of a collection is within range.
        /// </summary>
        public static Result ValidateCount<T>(IEnumerable<T>? collection, int min, int max, string errorCode, string errorMessage)
        {
            var count = collection?.Count() ?? 0;
            if (count < min || count > max)
                return Result.Failure(errorCode, errorMessage);
            return Result.Success();
        }

        #endregion

        #region Business Rules

        /// <summary>
        /// Validates that a value is unique (not duplicated).
        /// </summary>
        public static Result ValidateUnique(bool isDuplicate, string errorMessage)
            => isDuplicate
                ? Result.Failure(ErrorCodes.ValidationDuplicateValue, errorMessage)
                : Result.Success();

        /// <summary>
        /// Validates business rules through a predicate.
        /// </summary>
        public static Result ValidateBusinessRule(bool isValid, string errorCode, string errorMessage)
            => isValid ? Result.Success() : Result.Failure(errorCode, errorMessage);

        #endregion
    }
}
