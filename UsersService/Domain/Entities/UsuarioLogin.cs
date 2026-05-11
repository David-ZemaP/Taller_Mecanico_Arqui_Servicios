using System.Net.Mail;
using Taller_Mecanico_Users.Domain.ValueObjects;
using Taller_Mecanico_Users.Domain.Common;

namespace Taller_Mecanico_Users.Domain.Entities
{
    public class UsuarioLogin
    {
        public int UsuarioLoginId { get; private set; }
        public int? EmpleadoId { get; private set; }
        public int? ClienteId { get; private set; }
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public DateTime? UltimoAcceso { get; private set; }
        public bool Activo { get; private set; }
        public bool RequiereCambioPassword { get; private set; }
        public bool EsCliente { get; private set; }
        public string? NivelAcceso { get; private set; }
        public string? CreadoPor { get; private set; }
        public string? ActualizadoPor { get; private set; }
        public DateTime? FechaActualizacion { get; private set; }
        public string? InactivadoPor { get; private set; }

        private UsuarioLogin() { }

        public static Result<UsuarioLogin> Crear(int empleadoId, string email, string passwordHash, bool requiereCambioPassword = true)
        {
            return CreateInternal(
                empleadoId: empleadoId,
                clienteId: null,
                email: email,
                passwordHash: passwordHash,
                activo: true,
                requiereCambioPassword: requiereCambioPassword,
                esCliente: false,
                ultimoAcceso: null,
                usuarioLoginId: 0);
        }

        public static Result<UsuarioLogin> CrearParaCliente(int clienteId, string email, string passwordHash)
        {
            return CreateInternal(
                empleadoId: null,
                clienteId: clienteId,
                email: email,
                passwordHash: passwordHash,
                activo: true,
                requiereCambioPassword: true,
                esCliente: true,
                ultimoAcceso: null,
                usuarioLoginId: 0);
        }

        public static Result<UsuarioLogin> Reconstituir(int usuarioLoginId, int? empleadoId, int? clienteId, string email, string passwordHash, DateTime? ultimoAcceso, bool activo, bool requiereCambioPassword = false, bool esCliente = false, string? nivelAcceso = null, string? creadoPor = null, string? actualizadoPor = null, DateTime? fechaActualizacion = null, string? inactivadoPor = null)
        {
            return CreateInternal(
                empleadoId: empleadoId,
                clienteId: clienteId,
                email: email,
                passwordHash: passwordHash,
                activo: activo,
                requiereCambioPassword: requiereCambioPassword,
                esCliente: esCliente,
                ultimoAcceso: ultimoAcceso,
                usuarioLoginId: usuarioLoginId,
                nivelAcceso: nivelAcceso,
                creadoPor: null,
                actualizadoPor: null,
                fechaActualizacion: null,
                inactivadoPor: null);
        }

        public Result RegistrarAcceso()
        {
            if (!Activo)
            {
                return Result.Failure(ErrorCodes.ValidationInvalidValue, "No se puede registrar acceso en un usuario inactivo.");
            }

            UltimoAcceso = DateTime.UtcNow;
            return Result.Success();
        }

        public void RegistrarActualizacion(string? actor)
        {
            ActualizadoPor = actor;
            FechaActualizacion = DateTime.UtcNow;
        }

        public void RegistrarCreacion(string? actor)
        {
            CreadoPor = actor;
        }

        public void RegistrarEliminacion(string? actor)
        {
            InactivadoPor = actor;
            FechaActualizacion = DateTime.UtcNow;
        }

        public Result AsignarIdentificador(int usuarioLoginId)
        {
            if (usuarioLoginId <= 0)
            {
                return Result.Failure(ErrorCodes.ValidationInvalidValue, "El identificador del usuario no es válido.");
            }

            if (UsuarioLoginId > 0 && UsuarioLoginId != usuarioLoginId)
            {
                return Result.Failure(ErrorCodes.ValidationInvalidValue, "El identificador del usuario ya fue asignado.");
            }

            UsuarioLoginId = usuarioLoginId;
            return Result.Success();
        }

        public Result Desactivar()
        {
            if (!Activo)
            {
                return Result.Success();
            }

            Activo = false;
            return Result.Success();
        }

        public Result Activar()
        {
            if (Activo)
            {
                return Result.Success();
            }

            Activo = true;
            return Result.Success();
        }

        public Result CambiarPassword(string nuevoPasswordHash)
        {
            if (!Activo)
            {
                return Result.Failure(ErrorCodes.ValidationInvalidValue, "No se puede cambiar la contraseña de un usuario inactivo.");
            }

            if (string.IsNullOrWhiteSpace(nuevoPasswordHash))
            {
                return Result.Failure(ErrorCodes.ValidationRequired, "El hash de contraseña es obligatorio.");
            }

            PasswordHash = nuevoPasswordHash.Trim();
            RequiereCambioPassword = false;
            return Result.Success();
        }

        public Result CambiarEmail(string nuevoEmail)
        {
            var normalizedEmailResult = ValidateEmail(nuevoEmail);
            if (normalizedEmailResult.IsFailure)
            {
                return normalizedEmailResult;
            }

            Email = normalizedEmailResult.Value!;
            return Result.Success();
        }

        public Result ResetearPassword(string nuevoPasswordHash)
        {
            if (!Activo)
            {
                return Result.Failure(ErrorCodes.ValidationInvalidValue, "No se puede restablecer la contraseña de un usuario inactivo.");
            }

            if (string.IsNullOrWhiteSpace(nuevoPasswordHash))
            {
                return Result.Failure(ErrorCodes.ValidationRequired, "El hash de contraseña es obligatorio.");
            }

            PasswordHash = nuevoPasswordHash.Trim();
            RequiereCambioPassword = true;
            return Result.Success();
        }

        private static Result<UsuarioLogin> CreateInternal(
            int? empleadoId,
            int? clienteId,
            string email,
            string passwordHash,
            bool activo,
            bool requiereCambioPassword,
            bool esCliente,
            DateTime? ultimoAcceso,
            int usuarioLoginId,
            string? nivelAcceso = null,
            string? creadoPor = null,
            string? actualizadoPor = null,
            DateTime? fechaActualizacion = null,
            string? inactivadoPor = null)
        {
            var normalizedEmailResult = ValidateEmail(email);
            if (normalizedEmailResult.IsFailure)
            {
                return Result<UsuarioLogin>.Failure(normalizedEmailResult.ErrorCode!, normalizedEmailResult.ErrorMessage!);
            }

            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                return Result<UsuarioLogin>.Failure(ErrorCodes.ValidationRequired, "El hash de contraseña es obligatorio.");
            }

            if (esCliente)
            {
                if (!clienteId.HasValue || clienteId.Value <= 0)
                {
                    return Result<UsuarioLogin>.Failure(ErrorCodes.ValidationInvalidValue, "El ClienteId es obligatorio para usuarios cliente.");
                }

                if (empleadoId.HasValue)
                {
                    return Result<UsuarioLogin>.Failure(ErrorCodes.ValidationInvalidValue, "Un usuario cliente no puede tener EmpleadoId.");
                }
            }
            else
            {
                if (!empleadoId.HasValue || empleadoId.Value <= 0)
                {
                    return Result<UsuarioLogin>.Failure(ErrorCodes.ValidationInvalidValue, "El EmpleadoId es obligatorio para usuarios empleado.");
                }

                if (clienteId.HasValue)
                {
                    return Result<UsuarioLogin>.Failure(ErrorCodes.ValidationInvalidValue, "Un usuario empleado no puede tener ClienteId.");
                }
            }

            if (usuarioLoginId < 0)
            {
                return Result<UsuarioLogin>.Failure(ErrorCodes.ValidationInvalidValue, "El identificador del usuario no es válido.");
            }

            return Result<UsuarioLogin>.Success(new UsuarioLogin
            {
                UsuarioLoginId = usuarioLoginId,
                EmpleadoId = empleadoId,
                ClienteId = clienteId,
                Email = normalizedEmailResult.Value!,
                PasswordHash = passwordHash.Trim(),
                UltimoAcceso = ultimoAcceso,
                Activo = activo,
                RequiereCambioPassword = requiereCambioPassword,
                EsCliente = esCliente,
                NivelAcceso = nivelAcceso,
                CreadoPor = creadoPor,
                ActualizadoPor = actualizadoPor,
                FechaActualizacion = fechaActualizacion,
                InactivadoPor = inactivadoPor
            });
        }

        private static Result<string> ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Result<string>.Failure(ErrorCodes.ValidationRequired, "El email es obligatorio.");
            }

            var trimmedEmail = email.Trim();

            try
            {
                var mailAddress = new MailAddress(trimmedEmail);
                if (!string.Equals(mailAddress.Address, trimmedEmail, StringComparison.OrdinalIgnoreCase))
                {
                    return Result<string>.Failure(ErrorCodes.ValidationInvalidValue, "El email no es válido.");
                }

                return Result<string>.Success(trimmedEmail);
            }
            catch
            {
                return Result<string>.Failure(ErrorCodes.ValidationInvalidValue, "El email no es válido.");
            }
        }
    }
}
