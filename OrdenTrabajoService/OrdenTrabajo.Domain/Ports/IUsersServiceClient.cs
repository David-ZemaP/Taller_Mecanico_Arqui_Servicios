using Taller_Mecanico_Arqui.Domain.Common;

namespace Taller_Mecanico_Arqui.Domain.Ports
{
    /// <summary>
    /// Port: Client for communicating with UsersService to create/manage user accounts.
    /// This is an outbound adapter in hexagonal architecture.
    /// </summary>
    public interface IUsersServiceClient
    {
        /// <summary>
        /// Creates a user account for a cliente in UsersService.
        /// Returns the usuarioLoginId on success.
        /// </summary>
        Task<Result<int>> CreateUsuarioForClienteAsync(int clienteId, string email);

        /// <summary>
        /// Creates a user account for an empleado in UsersService.
        /// Returns the usuarioLoginId on success.
        /// </summary>
        Task<Result<int>> CreateUsuarioForEmpleadoAsync(int empleadoId, string email);
    }
}
