using Npgsql;

namespace OrdenTrabajoService.Infrastructure.Persistence
{
    public interface ISqlConnectionFactory
    {
        NpgsqlConnection CreateConnection();
    }
}
