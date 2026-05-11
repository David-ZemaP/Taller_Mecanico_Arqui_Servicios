using System.Data;
using System.Data.Common;
using Taller_Mecanico_Users.Domain.Ports;
using Taller_Mecanico_Users.Application.Persistence;
using Taller_Mecanico_Users.Application.Services;

namespace Taller_Mecanico_Users.Data.Repositories
{
    public class EmpleadoRepository : IEmpleadoRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;
        private readonly IAuditService _auditService;

        public EmpleadoRepository(ISqlConnectionFactory connectionFactory, IAuditService auditService)
        {
            _connectionFactory = connectionFactory;
            _auditService = auditService;
        }

        public async Task<IEnumerable<EmpleadoRecord>> GetAllAsync()
        {
            var lista = new List<EmpleadoRecord>();
            await using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM empleado WHERE isdeleted = FALSE ORDER BY empleadoid;";

            using var reader = await ((DbCommand)cmd).ExecuteReaderAsync();
            while (await reader.ReadAsync())
                lista.Add(MapReader(reader));

            return lista;
        }

        public async Task<EmpleadoRecord?> GetByIdAsync(int id)
        {
            await using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM empleado WHERE empleadoid = @Id AND isdeleted = FALSE;";
            AddParam(cmd, "@Id", id);

            using var reader = await ((DbCommand)cmd).ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapReader(reader) : null;
        }

        public async Task<int> CreateAsync(NuevoEmpleadoRecord data)
        {
            await using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync();
            await using var tx = await ((DbConnection)conn).BeginTransactionAsync();

            try
            {
                var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    INSERT INTO empleado
                        (nombre, primerapellido, segundoapellido, ci, cicomplemento, telefono, email,
                         fechacontratacion, tipoempleado, estadolaboral, especialidad,
                         salarioporhora, salariomensual, nivelacceso, creadopor, isdeleted)
                    VALUES
                        (@Nombre, @PrimerApellido, @SegundoApellido, @Ci, @CiComplemento, @Telefono, @Email,
                         @FechaContratacion, @TipoEmpleado, @EstadoLaboral, @Especialidad,
                         @SalarioPorHora, @SalarioMensual, @NivelAcceso, @CreadoPor, FALSE)
                    RETURNING empleadoid;";

                AddParam(cmd, "@Nombre", data.Nombre);
                AddParam(cmd, "@PrimerApellido", data.PrimerApellido);
                AddParam(cmd, "@SegundoApellido", (object?)data.SegundoApellido ?? DBNull.Value);
                AddParam(cmd, "@Ci", data.Ci);
                AddParam(cmd, "@CiComplemento", (object?)data.CiComplemento ?? DBNull.Value);
                AddParam(cmd, "@Telefono", data.Telefono);
                AddParam(cmd, "@Email", (object?)data.Email ?? DBNull.Value);
                AddParam(cmd, "@FechaContratacion", data.FechaContratacion);
                AddParam(cmd, "@TipoEmpleado", data.TipoEmpleado);
                AddParam(cmd, "@EstadoLaboral", data.EstadoLaboral);
                AddParam(cmd, "@Especialidad", (object?)data.Especialidad ?? DBNull.Value);
                AddParam(cmd, "@SalarioPorHora", (object?)data.SalarioPorHora ?? DBNull.Value);
                AddParam(cmd, "@SalarioMensual", (object?)data.SalarioMensual ?? DBNull.Value);
                AddParam(cmd, "@NivelAcceso", (object?)data.NivelAcceso ?? DBNull.Value);
                AddParam(cmd, "@CreadoPor", (object?)data.CreadoPor ?? DBNull.Value);

                var newId = Convert.ToInt32(await ((DbCommand)cmd).ExecuteScalarAsync());
                await _auditService.LogAsync((DbConnection)conn, (DbTransaction)tx, "empleado", newId, "INSERT");
                await tx.CommitAsync();
                return newId;
            }
            catch
            {
                try { await tx.RollbackAsync(); } catch { }
                throw;
            }
        }

        public async Task UpdateAsync(int id, NuevoEmpleadoRecord data)
        {
            await using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync();
            await using var tx = await ((DbConnection)conn).BeginTransactionAsync();

            try
            {
                var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
                    UPDATE empleado SET
                        nombre = @Nombre, primerapellido = @PrimerApellido, segundoapellido = @SegundoApellido,
                        ci = @Ci, cicomplemento = @CiComplemento, telefono = @Telefono, email = @Email,
                        fechacontratacion = @FechaContratacion, tipoempleado = @TipoEmpleado,
                        estadolaboral = @EstadoLaboral, especialidad = @Especialidad,
                        salarioporhora = @SalarioPorHora, salariomensual = @SalarioMensual,
                        nivelacceso = @NivelAcceso
                    WHERE empleadoid = @Id AND isdeleted = FALSE;";

                AddParam(cmd, "@Id", id);
                AddParam(cmd, "@Nombre", data.Nombre);
                AddParam(cmd, "@PrimerApellido", data.PrimerApellido);
                AddParam(cmd, "@SegundoApellido", (object?)data.SegundoApellido ?? DBNull.Value);
                AddParam(cmd, "@Ci", data.Ci);
                AddParam(cmd, "@CiComplemento", (object?)data.CiComplemento ?? DBNull.Value);
                AddParam(cmd, "@Telefono", data.Telefono);
                AddParam(cmd, "@Email", (object?)data.Email ?? DBNull.Value);
                AddParam(cmd, "@FechaContratacion", data.FechaContratacion);
                AddParam(cmd, "@TipoEmpleado", data.TipoEmpleado);
                AddParam(cmd, "@EstadoLaboral", data.EstadoLaboral);
                AddParam(cmd, "@Especialidad", (object?)data.Especialidad ?? DBNull.Value);
                AddParam(cmd, "@SalarioPorHora", (object?)data.SalarioPorHora ?? DBNull.Value);
                AddParam(cmd, "@SalarioMensual", (object?)data.SalarioMensual ?? DBNull.Value);
                AddParam(cmd, "@NivelAcceso", (object?)data.NivelAcceso ?? DBNull.Value);

                await ((DbCommand)cmd).ExecuteNonQueryAsync();
                await _auditService.LogAsync((DbConnection)conn, (DbTransaction)tx, "empleado", id, "UPDATE");
                await tx.CommitAsync();
            }
            catch
            {
                try { await tx.RollbackAsync(); } catch { }
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            await using var conn = _connectionFactory.CreateConnection();
            await conn.OpenAsync();
            await using var tx = await ((DbConnection)conn).BeginTransactionAsync();

            try
            {
                var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = "UPDATE empleado SET isdeleted = TRUE WHERE empleadoid = @Id;";
                AddParam(cmd, "@Id", id);
                await ((DbCommand)cmd).ExecuteNonQueryAsync();
                await _auditService.LogAsync((DbConnection)conn, (DbTransaction)tx, "empleado", id, "DELETE");
                await tx.CommitAsync();
            }
            catch
            {
                try { await tx.RollbackAsync(); } catch { }
                throw;
            }
        }

        private static EmpleadoRecord MapReader(DbDataReader r) => new(
            r.GetInt32(r.GetOrdinal("empleadoid")),
            r.GetString(r.GetOrdinal("nombre")),
            r.GetString(r.GetOrdinal("primerapellido")),
            r.IsDBNull(r.GetOrdinal("segundoapellido")) ? null : r.GetString(r.GetOrdinal("segundoapellido")),
            r.GetInt32(r.GetOrdinal("ci")),
            r.IsDBNull(r.GetOrdinal("cicomplemento")) ? null : r.GetString(r.GetOrdinal("cicomplemento")),
            r.GetInt32(r.GetOrdinal("telefono")),
            r.IsDBNull(r.GetOrdinal("email")) ? null : r.GetString(r.GetOrdinal("email")),
            r.GetDateTime(r.GetOrdinal("fechacontratacion")),
            r.GetString(r.GetOrdinal("tipoempleado")),
            r.GetString(r.GetOrdinal("estadolaboral")),
            r.IsDBNull(r.GetOrdinal("especialidad")) ? null : r.GetString(r.GetOrdinal("especialidad")),
            r.IsDBNull(r.GetOrdinal("salarioporhora")) ? null : r.GetDecimal(r.GetOrdinal("salarioporhora")),
            r.IsDBNull(r.GetOrdinal("salariomensual")) ? null : r.GetDecimal(r.GetOrdinal("salariomensual")),
            r.IsDBNull(r.GetOrdinal("nivelacceso")) ? null : r.GetString(r.GetOrdinal("nivelacceso"))
        );

        private static void AddParam(IDbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value;
            cmd.Parameters.Add(p);
        }
    }
}
