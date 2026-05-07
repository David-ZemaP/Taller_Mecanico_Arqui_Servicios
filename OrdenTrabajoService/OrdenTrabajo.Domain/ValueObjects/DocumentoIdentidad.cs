using Taller_Mecanico_Arqui.Domain.Common;

namespace Taller_Mecanico_Arqui.Domain.ValueObjects
{
    public record DocumentoIdentidad
    {
        public int Numero { get; }
        public string? Complemento { get; }

        private DocumentoIdentidad(int numero, string? complemento)
        {
            Numero = numero;
            Complemento = complemento;
        }

        public static Result<DocumentoIdentidad> Crear(int numero, string? complemento = null)
        {
            if (numero < 100000 || numero > 99999999)
                return Result<DocumentoIdentidad>.Failure(ErrorCodes.ValidationInvalidValue, "CI debe tener entre 6 y 8 dígitos.");
            if (complemento != null && (complemento.Length > 2 || !System.Text.RegularExpressions.Regex.IsMatch(complemento, @"^\d[A-Z]$")))
                return Result<DocumentoIdentidad>.Failure(ErrorCodes.ValidationInvalidValue, "Formato de complemento inválido (Ej: 1G).");

            return Result<DocumentoIdentidad>.Success(new DocumentoIdentidad(numero, complemento));
        }

        public override string ToString()
        {
            return $"{Numero} {Complemento}".Trim();
        }
    }
}
