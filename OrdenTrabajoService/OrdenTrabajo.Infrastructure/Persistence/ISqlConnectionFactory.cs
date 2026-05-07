using Npgsql;

namespace Taller_Mecanico_Arqui.Infrastructure.Persistence;

public interface ISqlConnectionFactory
{
    NpgsqlConnection CreateConnection();
}
