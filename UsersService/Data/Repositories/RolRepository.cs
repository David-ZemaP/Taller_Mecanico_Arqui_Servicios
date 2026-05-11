using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using Taller_Mecanico_Users.Domain.Entities;
using Taller_Mecanico_Users.Domain.Ports;
using Taller_Mecanico_Users.Application.Persistence;

namespace Taller_Mecanico_Users.Data.Repositories
{
    public class RolRepository : IRolRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public RolRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IEnumerable<Rol>> GetAllAsync()
        {
            var roles = new List<Rol>();
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT rolid, nombre, descripcion FROM rol ORDER BY rolid;";

            using var reader = await ((System.Data.Common.DbCommand)command).ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                roles.Add(MapReaderToEntity(reader));
            }
            return roles;
        }

        public async Task<Rol?> GetByIdAsync(int id)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT rolid, nombre, descripcion FROM rol WHERE rolid = @Id;";
            AddParameter(command, "@Id", id);

            using var reader = await ((System.Data.Common.DbCommand)command).ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToEntity(reader);
            }
            return null;
        }

        public async Task<Rol?> GetByNombreAsync(string nombre)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT rolid, nombre, descripcion FROM rol WHERE LOWER(nombre) = LOWER(@Nombre);";
            AddParameter(command, "@Nombre", nombre);

            using var reader = await ((System.Data.Common.DbCommand)command).ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToEntity(reader);
            }
            return null;
        }

        private void AddParameter(System.Data.IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        private Rol MapReaderToEntity(System.Data.Common.DbDataReader reader)
        {
            var rolId = reader.GetInt32(reader.GetOrdinal("rolid"));
            var nombre = reader.GetString(reader.GetOrdinal("nombre"));
            
            string? descripcion = null;
            var ordinalDesc = reader.GetOrdinal("descripcion");
            if (!reader.IsDBNull(ordinalDesc))
                descripcion = reader.GetString(ordinalDesc);

            var result = Rol.Reconstituir(rolId, nombre, descripcion);
            if (result.IsFailure)
            {
                throw new InvalidOperationException($"Datos inválidos de rol en la base de datos: {result.ErrorMessage}");
            }

            return result.Value!;
        }
    }
}