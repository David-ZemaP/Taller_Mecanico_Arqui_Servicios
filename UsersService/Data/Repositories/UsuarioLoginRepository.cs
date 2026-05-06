using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Entities;
using Taller_Mecanico_Users.Domain.Ports;
using Taller_Mecanico_Users.Framework.Persistence;
using Taller_Mecanico_Users.Framework.Services;

namespace Taller_Mecanico_Users.Data.Repositories
{
    public class UsuarioLoginRepository : IUsuarioLoginRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly IAuthenticationHelper _authHelper;

        public UsuarioLoginRepository(ISqlConnectionFactory connectionFactory, IAuthenticationHelper authHelper)
        {
            _connectionFactory = connectionFactory;
            _authHelper = authHelper;
        }

        public async Task<Result> AddAsync(UsuarioLogin entity)
        {
            string actor = _authHelper.GetCurrentAuditActor();
            
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // --- A. INSERTAR EL USUARIO ---
                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO usuariologin (empleadoid, clienteid, email, passwordhash, activo, requierecambiopassword, escliente) 
                    VALUES (@EmpleadoId, @ClienteId, @Email, @PasswordHash, @Activo, @RequiereCambioPassword, @EsCliente)
                    RETURNING usuariologinid;";

                AddParameter(command, "@EmpleadoId", entity.EmpleadoId ?? (object)DBNull.Value);
                AddParameter(command, "@ClienteId", entity.ClienteId ?? (object)DBNull.Value);
                AddParameter(command, "@Email", entity.Email);
                AddParameter(command, "@PasswordHash", entity.PasswordHash);
                AddParameter(command, "@Activo", entity.Activo);
                AddParameter(command, "@RequiereCambioPassword", entity.RequiereCambioPassword);
                AddParameter(command, "@EsCliente", entity.EsCliente);

                var result = await command.ExecuteScalarAsync();
                if (result != null)
                {
                    entity.UsuarioLoginId = Convert.ToInt32(result);
                }

                // --- B. INSERTAR EN BITÁCORA (AUDITORÍA) ---
                var auditCommand = connection.CreateCommand();
                auditCommand.Transaction = transaction;
                auditCommand.CommandText = @"
                    INSERT INTO audit_logs (tabla_afectada, registro_id, accion, realizado_por, fecha_hora) 
                    VALUES ('usuariologin', @RegistroId, 'INSERT', @Actor, @FechaHora);";

                AddParameter(auditCommand, "@RegistroId", entity.UsuarioLoginId);
                AddParameter(auditCommand, "@Actor", actor);
                AddParameter(auditCommand, "@FechaHora", DateTime.UtcNow);

                await auditCommand.ExecuteNonQueryAsync();

                // Confirmamos la transacción
                transaction.Commit();

                return Result.Success();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Result.Failure(ErrorCodes.DbError, ex.Message);
            }
        }

        public async Task<Result> UpdateAsync(UsuarioLogin entity)
        {
            string actor = _authHelper.GetCurrentAuditActor();
            
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // --- A. ACTUALIZAR EL USUARIO ---
                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    UPDATE usuariologin 
                    SET email = @Email, 
                        activo = @Activo, 
                        requierecambiopassword = @RequiereCambioPassword,
                        passwordhash = @PasswordHash,
                        ultimoacceso = @UltimoAcceso
                    WHERE usuariologinid = @UsuarioLoginId;";

                AddParameter(command, "@UsuarioLoginId", entity.UsuarioLoginId);
                AddParameter(command, "@Email", entity.Email);
                AddParameter(command, "@Activo", entity.Activo);
                AddParameter(command, "@RequiereCambioPassword", entity.RequiereCambioPassword);
                AddParameter(command, "@PasswordHash", entity.PasswordHash);
                AddParameter(command, "@UltimoAcceso", entity.UltimoAcceso ?? (object)DBNull.Value);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    return Result.Failure(ErrorCodes.UsuarioLoginNotFound, "El usuario no existe.");
                }

                // --- B. INSERTAR EN BITÁCORA (AUDITORÍA) ---
                var auditCommand = connection.CreateCommand();
                auditCommand.Transaction = transaction;
                auditCommand.CommandText = @"
                    INSERT INTO audit_logs (tabla_afectada, registro_id, accion, realizado_por, fecha_hora) 
                    VALUES ('usuariologin', @RegistroId, 'UPDATE', @Actor, @FechaHora);";

                AddParameter(auditCommand, "@RegistroId", entity.UsuarioLoginId);
                AddParameter(auditCommand, "@Actor", actor);
                AddParameter(auditCommand, "@FechaHora", DateTime.UtcNow);

                await auditCommand.ExecuteNonQueryAsync();

                // Confirmamos la transacción
                transaction.Commit();

                return Result.Success();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Result.Failure(ErrorCodes.DbError, ex.Message);
            }
        }

        public async Task<Result<UsuarioLogin?>> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM usuariologin WHERE usuariologinid = @Id;";
            AddParameter(command, "@Id", id);

            using var reader = await (command as System.Data.Common.DbCommand)!.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return Result<UsuarioLogin?>.Success(MapReaderToEntity(reader));
            }

            return Result<UsuarioLogin?>.Failure(ErrorCodes.UsuarioLoginNotFound, "Usuario no encontrado.");
        }

        public async Task<IEnumerable<UsuarioLogin>> GetAllAsync()
        {
            var usuarios = new List<UsuarioLogin>();
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM usuariologin;";

            using var reader = await (command as System.Data.Common.DbCommand)!.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                usuarios.Add(MapReaderToEntity(reader));
            }
            return usuarios;
        }

        public async Task<UsuarioLogin?> GetByEmailAsync(string email)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM usuariologin WHERE email = @Email LIMIT 1;";
            AddParameter(command, "@Email", email);

            using var reader = await (command as System.Data.Common.DbCommand)!.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToEntity(reader);
            }
            return null;
        }

        public async Task<UsuarioLogin?> GetByEmpleadoIdAsync(int empleadoId)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM usuariologin WHERE empleadoid = @EmpleadoId LIMIT 1;";
            AddParameter(command, "@EmpleadoId", empleadoId);

            using var reader = await (command as System.Data.Common.DbCommand)!.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToEntity(reader);
            }
            return null;
        }

        public async Task<UsuarioLogin?> GetByClienteIdAsync(int clienteId)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM usuariologin WHERE clienteid = @ClienteId LIMIT 1;";
            AddParameter(command, "@ClienteId", clienteId);

            using var reader = await (command as System.Data.Common.DbCommand)!.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToEntity(reader);
            }
            return null;
        }

        // ==========================================
        // MÉTODOS AUXILIARES
        // ==========================================
        public async Task<Result> DeleteAsync(int id)
        {
            string actor = _authHelper.GetCurrentAuditActor();

            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                // --- A. ELIMINAR EL USUARIO ---
                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"DELETE FROM usuariologin WHERE usuariologinid = @UsuarioLoginId;";
                AddParameter(command, "@UsuarioLoginId", id);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    return Result.Failure(ErrorCodes.UsuarioLoginNotFound, "Usuario no encontrado.");
                }

                // --- B. INSERTAR EN BITÁCORA (AUDITORÍA) ---
                var auditCommand = connection.CreateCommand();
                auditCommand.Transaction = transaction;
                auditCommand.CommandText = @"
                    INSERT INTO audit_logs (tabla_afectada, registro_id, accion, realizado_por, fecha_hora) 
                    VALUES ('usuariologin', @RegistroId, 'DELETE', @Actor, @FechaHora);";

                AddParameter(auditCommand, "@RegistroId", id);
                AddParameter(auditCommand, "@Actor", actor);
                AddParameter(auditCommand, "@FechaHora", DateTime.UtcNow);

                await auditCommand.ExecuteNonQueryAsync();

                // Confirmamos la transacción
                transaction.Commit();

                return Result.Success();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Result.Failure(ErrorCodes.DbError, ex.Message);
            }
        }

        private void AddParameter(IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        private UsuarioLogin MapReaderToEntity(System.Data.Common.DbDataReader reader)
        {
            return UsuarioLogin.Reconstituir(
                reader.GetInt32(reader.GetOrdinal("usuariologinid")),
                reader.IsDBNull(reader.GetOrdinal("empleadoid")) ? null : reader.GetInt32(reader.GetOrdinal("empleadoid")),
                reader.IsDBNull(reader.GetOrdinal("clienteid")) ? null : reader.GetInt32(reader.GetOrdinal("clienteid")),
                reader.GetString(reader.GetOrdinal("email")),
                reader.GetString(reader.GetOrdinal("passwordhash")),
                reader.IsDBNull(reader.GetOrdinal("ultimoacceso")) ? null : reader.GetDateTime(reader.GetOrdinal("ultimoacceso")),
                reader.GetBoolean(reader.GetOrdinal("activo")),
                reader.GetBoolean(reader.GetOrdinal("requierecambiopassword")),
                reader.GetBoolean(reader.GetOrdinal("escliente"))
            );
        }
    }
}