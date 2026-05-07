using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data.Common;
using Taller_Mecanico_Users.Framework.Persistence;

namespace OrdenTrabajoService.Infrastructure.Persistence
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' no configurada.");
        }

        public DbConnection CreateConnection()
            => new NpgsqlConnection(_connectionString);
    }
}
