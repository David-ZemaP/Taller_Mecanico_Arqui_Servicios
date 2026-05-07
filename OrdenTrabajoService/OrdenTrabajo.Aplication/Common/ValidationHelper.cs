using Taller_Mecanico_Users.Domain.Common;

namespace OrdenTrabajoService.Application.Common
{
    public static class ValidationHelper
    {
        public static Result<T> ParseEnum<T>(string? value, string errorMessage, bool removeSpaces = false) where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
                return Result<T>.Failure(ErrorCodes.ValidationInvalidValue, errorMessage);

            var normalized = removeSpaces ? value.Replace(" ", "") : value;

            if (!Enum.TryParse<T>(normalized, ignoreCase: true, out var result))
                return Result<T>.Failure(ErrorCodes.ValidationInvalidValue, errorMessage);

            return Result<T>.Success(result);
        }
    }
}
