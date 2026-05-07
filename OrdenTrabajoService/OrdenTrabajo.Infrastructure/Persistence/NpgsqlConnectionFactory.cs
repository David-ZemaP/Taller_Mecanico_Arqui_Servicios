using Npgsql;

namespace Taller_Mecanico_Arqui.Infrastructure.Persistence;

public class NpgsqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public NpgsqlConnection CreateConnection() => new(_connectionString);
}
