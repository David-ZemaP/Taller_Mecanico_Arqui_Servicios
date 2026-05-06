namespace Taller_Mecanico_Users.Domain.Common
{
    public static class ErrorCodes
    {
        public const string DbError = "DB_ERROR";
        public const string DbInsertFailed = "DB_INSERT_FAILED";

        public const string ClienteNotFound = "CLIENTE_NOT_FOUND";
        public const string ClienteCiDuplicado = "CLIENTE_CI_DUPLICADO";

        public const string EmpleadoNotFound = "EMPLEADO_NOT_FOUND";
        public const string EmpleadoCiDuplicado = "EMPLEADO_CI_DUPLICADO";

        public const string VehiculoNotFound = "VEHICULO_NOT_FOUND";
        public const string VehiculoPlacaDuplicada = "VEHICULO_PLACA_DUPLICADA";

        public const string OrdenTrabajoNotFound = "ORDEN_TRABAJO_NOT_FOUND";

        public const string ProductoNotFound = "PRODUCTO_NOT_FOUND";
        public const string ValidationInvalidValue = "VALIDATION_INVALID_VALUE";
        public const string ValidationInvalidAccessLevel = "VALIDATION_INVALID_ACCESS_LEVEL";
        public const string ValidationAdminEmailRequired = "VALIDATION_ADMIN_EMAIL_REQUIRED";

        public const string UsuarioLoginNotFound = "USUARIO_LOGIN_NOT_FOUND";
        public const string UsuarioEmailDuplicado = "USUARIO_EMAIL_DUPLICADO";

        public const string EmailNotConfigured = "EMAIL_NOT_CONFIGURED";
        public const string EmailSendFailed = "EMAIL_SEND_FAILED";

        public const string ValidationRequired = "VALIDATION_REQUIRED";
        public const string ValidationDuplicateValue = "VALIDATION_DUPLICATE_VALUE";
        public const string NotFound = "NOT_FOUND";
    }
}
