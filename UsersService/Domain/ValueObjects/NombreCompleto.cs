using Taller_Mecanico_Users.Domain.Common;

namespace Taller_Mecanico_Users.Domain.ValueObjects
{
    public record NombreCompleto
    {
        public string Nombres { get; }
        public string PrimerApellido { get; }
        public string? SegundoApellido { get; }

        private NombreCompleto(string nombres, string primerApellido, string? segundoApellido)
        {
            Nombres = nombres;
            PrimerApellido = primerApellido;
            SegundoApellido = segundoApellido;
        }

        public static Result<NombreCompleto> Crear(string nombres, string primerApellido, string? segundoApellido = null)
        {
            if (string.IsNullOrWhiteSpace(nombres) || nombres.Length > 20)
                return Result<NombreCompleto>.Failure(ErrorCodes.ValidationInvalidValue, "Los nombres son obligatorios y no pueden tener más de 20 caracteres.");
            if (string.IsNullOrWhiteSpace(primerApellido) || primerApellido.Length > 20)
                return Result<NombreCompleto>.Failure(ErrorCodes.ValidationInvalidValue, "El primer apellido es obligatorio y no puede tener más de 20 caracteres.");
            if (segundoApellido != null && segundoApellido.Length > 20)
                return Result<NombreCompleto>.Failure(ErrorCodes.ValidationInvalidValue, "El segundo apellido no puede tener más de 20 caracteres.");

            return Result<NombreCompleto>.Success(new NombreCompleto(nombres.Trim(), primerApellido.Trim(), segundoApellido?.Trim()));
        }

        public override string ToString()
        {
            return $"{PrimerApellido} {SegundoApellido} {Nombres}".Trim();
        }
    }
}
