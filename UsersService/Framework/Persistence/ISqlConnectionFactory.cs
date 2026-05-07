using System.Data.Common;

namespace Taller_Mecanico_Users.Framework.Persistence
{
    public interface ISqlConnectionFactory
    {
        DbConnection CreateConnection();
    }
}
