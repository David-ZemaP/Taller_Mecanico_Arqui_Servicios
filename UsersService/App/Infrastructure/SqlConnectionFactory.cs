using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data.Common;
using Taller_Mecanico_Users.Framework.Persistence;

namespace Taller_Mecanico_Users.App.Infrastructure
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not configured.");
        }

        public DbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}
