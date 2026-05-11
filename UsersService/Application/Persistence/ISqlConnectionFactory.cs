using System.Data.Common;

namespace Taller_Mecanico_Users.Application.Persistence
{
    public interface ISqlConnectionFactory
    {
        DbConnection CreateConnection();
    }
}

