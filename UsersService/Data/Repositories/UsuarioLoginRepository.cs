using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Entities;
using Taller_Mecanico_Users.Domain.Ports;
using Taller_Mecanico_Users.Application.Persistence;
using Taller_Mecanico_Users.Application.Services;

namespace Taller_Mecanico_Users.Data.Repositories
{
    public class UsuarioLoginRepository : IUsuarioLoginRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly IAuditService _auditService;

        public UsuarioLoginRepository(ISqlConnectionFactory connectionFactory, IAuditService auditService)
        {
            _connectionFactory = connectionFactory;
            _auditService = auditService;
        }

        public async Task<Result> AddAsync(UsuarioLogin entity)
        {
            
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await (connection as System.Data.Common.DbConnection)!.BeginTransactionAsync();

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    INSERT INTO usuariologin (empleadoid, clienteid, email, passwordhash, activo, requierecambiopassword, escliente, creadopor, rolid) 
                    VALUES (@EmpleadoId, @ClienteId, @Email, @PasswordHash, @Activo, @RequiereCambioPassword, @EsCliente, @CreadoPor, @RolId)
                    RETURNING usuariologinid;";

                AddParameter(command, "@EmpleadoId", entity.EmpleadoId ?? (object)DBNull.Value);
                AddParameter(command, "@ClienteId", entity.ClienteId ?? (object)DBNull.Value);
                AddParameter(command, "@Email", entity.Email);
                AddParameter(command, "@PasswordHash", entity.PasswordHash);
                AddParameter(command, "@Activo", entity.Activo);
                AddParameter(command, "@RequiereCambioPassword", entity.RequiereCambioPassword);
                AddParameter(command, "@EsCliente", entity.EsCliente);
                AddParameter(command, "@CreadoPor", entity.CreadoPor ?? (object)DBNull.Value);
                AddParameter(command, "@RolId", entity.RolId ?? (object)DBNull.Value);

                var result = await command.ExecuteScalarAsync();
                if (result != null)
                {
                    var assignIdResult = entity.AsignarIdentificador(Convert.ToInt32(result));
                    if (assignIdResult.IsFailure)
                    {
                        return assignIdResult;
                    }
                }
                await _auditService.LogAsync(connection, transaction, "usuariologin", entity.UsuarioLoginId, "INSERT");

                await transaction.CommitAsync();

                return Result.Success();
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                try { await transaction.RollbackAsync(); } catch { }
                return Result.Failure(ErrorCodes.ValidationDuplicateValue, "Ya existe un registro con valores duplicados.");
            }
            catch (Exception ex)
            {
                try { await transaction.RollbackAsync(); } catch { }
                return Result.Failure(ErrorCodes.DbError, ex.Message);
            }
        }

        public async Task<Result> UpdateAsync(UsuarioLogin entity)
        {
            
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await (connection as System.Data.Common.DbConnection)!.BeginTransactionAsync();

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"
                    UPDATE usuariologin 
                    SET email = @Email, 
                        activo = @Activo, 
                        requierecambiopassword = @RequiereCambioPassword,
                        passwordhash = @PasswordHash,
                        ultimoacceso = @UltimoAcceso,
                        inactivadopor = @InactivadoPor,
                        actualizadopor = @ActualizadoPor,
                        fechaactualizacion = @FechaActualizacion,
                        rolid = @RolId
                    WHERE usuariologinid = @UsuarioLoginId;";

                AddParameter(command, "@UsuarioLoginId", entity.UsuarioLoginId);
                AddParameter(command, "@Email", entity.Email);
                AddParameter(command, "@Activo", entity.Activo);
                AddParameter(command, "@RequiereCambioPassword", entity.RequiereCambioPassword);
                AddParameter(command, "@PasswordHash", entity.PasswordHash);
                AddParameter(command, "@UltimoAcceso", entity.UltimoAcceso ?? (object)DBNull.Value);
                AddParameter(command, "@InactivadoPor", entity.InactivadoPor ?? (object)DBNull.Value);
                AddParameter(command, "@ActualizadoPor", entity.ActualizadoPor ?? (object)DBNull.Value);
                AddParameter(command, "@FechaActualizacion", entity.FechaActualizacion ?? (object)DBNull.Value);
                AddParameter(command, "@RolId", entity.RolId ?? (object)DBNull.Value);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    return Result.Failure(ErrorCodes.UsuarioLoginNotFound, "El usuario no existe.");
                }

                await _auditService.LogAsync(connection, transaction, "usuariologin", entity.UsuarioLoginId, "UPDATE");

                await transaction.CommitAsync();

                return Result.Success();
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                try { await transaction.RollbackAsync(); } catch { }
                return Result.Failure(ErrorCodes.ValidationDuplicateValue, "Ya existe un registro con valores duplicados.");
            }
            catch (Exception ex)
            {
                try { await transaction.RollbackAsync(); } catch { }
                return Result.Failure(ErrorCodes.DbError, ex.Message);
            }
        }

        public async Task<Result<UsuarioLogin?>> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT ul.*, r.rolid AS rol_rolid, r.nombre AS rol_nombre, r.descripcion AS rol_descripcion
                FROM usuariologin ul
                LEFT JOIN rol r ON r.rolid = ul.rolid
                WHERE ul.usuariologinid = @Id;";
            AddParameter(command, "@Id", id);

            using var reader = await (command as System.Data.Common.DbCommand)!.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return Result<UsuarioLogin?>.Success(MapReaderToEntityWithRol(reader));
            }

            return Result<UsuarioLogin?>.Failure(ErrorCodes.UsuarioLoginNotFound, "Usuario no encontrado.");
        }

        public async Task<IEnumerable<UsuarioLogin>> GetAllAsync()
        {
            var usuarios = new List<UsuarioLogin>();
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT ul.*, r.rolid AS rol_rolid, r.nombre AS rol_nombre, r.descripcion AS rol_descripcion
                FROM usuariologin ul
                LEFT JOIN rol r ON r.rolid = ul.rolid;";

            using var reader = await (command as System.Data.Common.DbCommand)!.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                usuarios.Add(MapReaderToEntityWithRol(reader));
            }
            return usuarios;
        }

        public async Task<UsuarioLogin?> GetByEmailAsync(string email)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT ul.*, r.rolid AS rol_rolid, r.nombre AS rol_nombre, r.descripcion AS rol_descripcion
                FROM usuariologin ul
                LEFT JOIN rol r ON r.rolid = ul.rolid
                WHERE ul.email = @Email LIMIT 1;";
            AddParameter(command, "@Email", email);

            using var reader = await (command as System.Data.Common.DbCommand)!.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToEntityWithRol(reader);
            }
            return null;
        }

        public async Task<UsuarioLogin?> GetByEmpleadoIdAsync(int empleadoId)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT ul.*, r.rolid AS rol_rolid, r.nombre AS rol_nombre, r.descripcion AS rol_descripcion
                FROM usuariologin ul
                LEFT JOIN rol r ON r.rolid = ul.rolid
                WHERE ul.empleadoid = @EmpleadoId LIMIT 1;";
            AddParameter(command, "@EmpleadoId", empleadoId);

            using var reader = await (command as System.Data.Common.DbCommand)!.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToEntityWithRol(reader);
            }
            return null;
        }

        public async Task<UsuarioLogin?> GetByClienteIdAsync(int clienteId)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT ul.*, r.rolid AS rol_rolid, r.nombre AS rol_nombre, r.descripcion AS rol_descripcion
                FROM usuariologin ul
                LEFT JOIN rol r ON r.rolid = ul.rolid
                WHERE ul.clienteid = @ClienteId LIMIT 1;";
            AddParameter(command, "@ClienteId", clienteId);

            using var reader = await (command as System.Data.Common.DbCommand)!.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToEntityWithRol(reader);
            }
            return null;
        }

        public async Task<Result> DeleteAsync(int id)
        {
            
            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            await using var transaction = await (connection as System.Data.Common.DbConnection)!.BeginTransactionAsync();

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"DELETE FROM usuariologin WHERE usuariologinid = @UsuarioLoginId;";
                AddParameter(command, "@UsuarioLoginId", id);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    return Result.Failure(ErrorCodes.UsuarioLoginNotFound, "Usuario no encontrado.");
                }

                await _auditService.LogAsync(connection, transaction, "usuariologin", id, "DELETE");

                await transaction.CommitAsync();

                return Result.Success();
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                try { await transaction.RollbackAsync(); } catch { }
                return Result.Failure(ErrorCodes.ValidationDuplicateValue, "Ya existe un registro con valores duplicados.");
            }
            catch (Exception ex)
            {
                try { await transaction.RollbackAsync(); } catch { }
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

        private UsuarioLogin MapReaderToEntity(System.Data.Common.DbDataReader reader, string? nivelAcceso = null)
        {
            string? creadoPor = null;
            string? actualizadoPor = null;
            string? inactivadoPor = null;
            DateTime? fechaActualizacion = null;

            var ordinalCreado = reader.GetOrdinal("creadopor");
            if (!reader.IsDBNull(ordinalCreado))
                creadoPor = reader.GetString(ordinalCreado);

            var ordinalActualizado = reader.GetOrdinal("actualizadopor");
            if (!reader.IsDBNull(ordinalActualizado))
                actualizadoPor = reader.GetString(ordinalActualizado);

            var ordinalInactivado = reader.GetOrdinal("inactivadopor");
            if (!reader.IsDBNull(ordinalInactivado))
                inactivadoPor = reader.GetString(ordinalInactivado);

            var ordinalFecha = reader.GetOrdinal("fechaactualizacion");
            if (!reader.IsDBNull(ordinalFecha))
                fechaActualizacion = reader.GetDateTime(ordinalFecha);

            var result = UsuarioLogin.Reconstituir(
                reader.GetInt32(reader.GetOrdinal("usuariologinid")),
                reader.IsDBNull(reader.GetOrdinal("empleadoid")) ? null : reader.GetInt32(reader.GetOrdinal("empleadoid")),
                reader.IsDBNull(reader.GetOrdinal("clienteid")) ? null : reader.GetInt32(reader.GetOrdinal("clienteid")),
                reader.GetString(reader.GetOrdinal("email")),
                reader.GetString(reader.GetOrdinal("passwordhash")),
                reader.IsDBNull(reader.GetOrdinal("ultimoacceso")) ? null : reader.GetDateTime(reader.GetOrdinal("ultimoacceso")),
                reader.GetBoolean(reader.GetOrdinal("activo")),
                reader.GetBoolean(reader.GetOrdinal("requierecambiopassword")),
                reader.GetBoolean(reader.GetOrdinal("escliente")),
                nivelAcceso,
                creadoPor,
                actualizadoPor,
                fechaActualizacion,
                inactivadoPor
            );

            if (result.IsFailure)
            {
                throw new InvalidOperationException($"Datos inválidos de usuario login en la base de datos: {result.ErrorMessage}");
            }

            return result.Value!;
        }

        private UsuarioLogin MapReaderToEntityWithRol(System.Data.Common.DbDataReader reader)
        {
            string? creadoPor = null;
            string? actualizadoPor = null;
            string? inactivadoPor = null;
            DateTime? fechaActualizacion = null;
            int? rolId = null;
            Rol? rol = null;

            var ordinalCreado = reader.GetOrdinal("creadopor");
            if (!reader.IsDBNull(ordinalCreado))
                creadoPor = reader.GetString(ordinalCreado);

            var ordinalActualizado = reader.GetOrdinal("actualizadopor");
            if (!reader.IsDBNull(ordinalActualizado))
                actualizadoPor = reader.GetString(ordinalActualizado);

            var ordinalInactivado = reader.GetOrdinal("inactivadopor");
            if (!reader.IsDBNull(ordinalInactivado))
                inactivadoPor = reader.GetString(ordinalInactivado);

            var ordinalFecha = reader.GetOrdinal("fechaactualizacion");
            if (!reader.IsDBNull(ordinalFecha))
                fechaActualizacion = reader.GetDateTime(ordinalFecha);

            // Leer datos del rol si existen
            var ordinalRolId = reader.GetOrdinal("rol_rolid");
            if (!reader.IsDBNull(ordinalRolId))
            {
                rolId = reader.GetInt32(ordinalRolId);
                var ordinalRolNombre = reader.GetOrdinal("rol_nombre");
                string? rolNombre = null;
                string? rolDescripcion = null;
                
                if (!reader.IsDBNull(ordinalRolNombre))
                    rolNombre = reader.GetString(ordinalRolNombre);
                
                var ordinalRolDesc = reader.GetOrdinal("rol_descripcion");
                if (!reader.IsDBNull(ordinalRolDesc))
                    rolDescripcion = reader.GetString(ordinalRolDesc);

                if (!string.IsNullOrEmpty(rolNombre))
                {
                    var rolResult = Taller_Mecanico_Users.Domain.Entities.Rol.Reconstituir(rolId.Value, rolNombre, rolDescripcion);
                    if (rolResult.IsSuccess)
                        rol = rolResult.Value;
                }
            }

            var result = UsuarioLogin.Reconstituir(
                reader.GetInt32(reader.GetOrdinal("usuariologinid")),
                reader.IsDBNull(reader.GetOrdinal("empleadoid")) ? null : reader.GetInt32(reader.GetOrdinal("empleadoid")),
                reader.IsDBNull(reader.GetOrdinal("clienteid")) ? null : reader.GetInt32(reader.GetOrdinal("clienteid")),
                reader.GetString(reader.GetOrdinal("email")),
                reader.GetString(reader.GetOrdinal("passwordhash")),
                reader.IsDBNull(reader.GetOrdinal("ultimoacceso")) ? null : reader.GetDateTime(reader.GetOrdinal("ultimoacceso")),
                reader.GetBoolean(reader.GetOrdinal("activo")),
                reader.GetBoolean(reader.GetOrdinal("requierecambiopassword")),
                reader.GetBoolean(reader.GetOrdinal("escliente")),
                null, // nivelAcceso ya no se usa, se usa rol
                creadoPor,
                actualizadoPor,
                fechaActualizacion,
                inactivadoPor,
                rolId,
                rol
            );

            if (result.IsFailure)
            {
                throw new InvalidOperationException($"Datos inválidos de usuario login en la base de datos: {result.ErrorMessage}");
            }

            return result.Value!;
        }
    }
}