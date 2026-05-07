using Microsoft.Extensions.Configuration;
using Npgsql;

namespace OrdenTrabajoService.Infrastructure.Persistence
{
    public class NpgsqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;

        public NpgsqlConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection no está configurada.");
        }

        public NpgsqlConnection CreateConnection()
            => new(_connectionString);
    }
}
