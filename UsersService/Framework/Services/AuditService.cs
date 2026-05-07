using System.Data.Common;
using Taller_Mecanico_Users.Framework.Persistence;
using Taller_Mecanico_Users.Framework.Services;

namespace Taller_Mecanico_Users.Framework.Services
{
    /// <summary>
    /// Implementación concreta de IAuditService.
    /// Inserta logs de auditoría en la tabla audit_logs y obtiene el actor internamente.
    /// </summary>
    public class AuditService : IAuditService
    {
        private readonly IAuthenticationHelper _authHelper;

        public AuditService(IAuthenticationHelper authHelper)
        {
            _authHelper = authHelper;
        }

        public async Task LogAsync(DbConnection connection, DbTransaction transaction, string tabla, int registroId, string accion)
        {
            var actor = _authHelper.GetCurrentAuditActor();

            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = @"
                INSERT INTO audit_logs (tabla_afectada, registro_id, accion, realizado_por, fecha_hora) 
                VALUES (@Tabla, @RegistroId, @Accion, @Actor, @FechaHora);";

            AddParameter(command, "@Tabla", tabla);
            AddParameter(command, "@RegistroId", registroId);
            AddParameter(command, "@Accion", accion);
            AddParameter(command, "@Actor", actor);
            AddParameter(command, "@FechaHora", DateTime.UtcNow);

            await command.ExecuteNonQueryAsync();
        }

        private static void AddParameter(DbCommand command, string paramName, object? value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = paramName;
            parameter.Value = value ?? System.DBNull.Value;
            command.Parameters.Add(parameter);
        }
    }
}
