using System.Data.Common;

namespace Taller_Mecanico_Users.Framework.Services
{
    /// <summary>
    /// Servicio de auditoría.
    /// Responsabilidad: Registrar cambios en entidades para trazabilidad.
    /// 
    /// Ventajas de separar auditoría:
    /// 1. Repositorio se enfoca en persistencia
    /// 2. Auditoría se puede testear independientemente
    /// 3. Se puede cambiar estrategia de auditoría sin afectar repositorio
    /// 4. Permite encriptar/enviar logs de auditoría a otro sistema
    /// </summary>
    public interface IAuditService
    {
        /// <summary>
        /// Registra una acción de auditoría en la base de datos.
        /// El servicio obtiene internamente el actor para mantener el repositorio libre
        /// de dependencias relacionadas a contexto HTTP / autenticación.
        /// </summary>
        Task LogAsync(DbConnection connection, DbTransaction transaction, string tabla, int registroId, string accion);
    }
}
