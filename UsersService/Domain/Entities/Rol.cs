using Taller_Mecanico_Users.Domain.Common;

namespace Taller_Mecanico_Users.Domain.Entities
{
    public class Rol
    {
        public int RolId { get; private set; }
        public string Nombre { get; private set; } = string.Empty;
        public string? Descripcion { get; private set; }

        private Rol() { }

        public static Result<Rol> Crear(string nombre, string? descripcion = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return Result<Rol>.Failure(ErrorCodes.ValidationRequired, "El nombre del rol es obligatorio.");
            }

            var nombreNormalizado = nombre.Trim();

            // Validar que sea uno de los roles válidos
            var rolesValidos = new[] { "Gerente", "Administrador", "Mecanico", "Cliente" };
            if (!rolesValidos.Contains(nombreNormalizado, StringComparer.OrdinalIgnoreCase))
            {
                return Result<Rol>.Failure(ErrorCodes.ValidationInvalidValue, 
                    $"El nombre del rol debe ser uno de: {string.Join(", ", rolesValidos)}");
            }

            return Result<Rol>.Success(new Rol
            {
                RolId = 0,
                Nombre = nombreNormalizado,
                Descripcion = descripcion?.Trim()
            });
        }

        public static Result<Rol> Reconstituir(int rolId, string nombre, string? descripcion)
        {
            if (rolId <= 0)
            {
                return Result<Rol>.Failure(ErrorCodes.ValidationInvalidValue, "El identificador del rol no es válido.");
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                return Result<Rol>.Failure(ErrorCodes.ValidationRequired, "El nombre del rol es obligatorio.");
            }

            return Result<Rol>.Success(new Rol
            {
                RolId = rolId,
                Nombre = nombre.Trim(),
                Descripcion = descripcion?.Trim()
            });
        }

        public void AsignarIdentificador(int rolId)
        {
            if (rolId <= 0)
            {
                throw new InvalidOperationException("El identificador del rol no es válido.");
            }

            if (RolId > 0 && RolId != rolId)
            {
                throw new InvalidOperationException("El identificador del rol ya fue asignado.");
            }

            RolId = rolId;
        }
    }
}