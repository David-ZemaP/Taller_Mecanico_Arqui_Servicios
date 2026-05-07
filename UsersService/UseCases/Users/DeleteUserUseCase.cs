using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Ports;

namespace Taller_Mecanico_Users.UseCases.Users
{
    public class DeleteUserUseCase
    {
        private readonly IUsuarioLoginRepository _repository;

        public DeleteUserUseCase(IUsuarioLoginRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result> ExecuteAsync(int usuarioLoginId)
        {
            // Verificar que el usuario exista antes de intentar eliminarlo
            var userResult = await _repository.GetByIdAsync(usuarioLoginId);
            if (userResult.IsFailure || userResult.Value == null)
            {
                return Result.Failure(ErrorCodes.UsuarioLoginNotFound, "Usuario no encontrado.");
            }

            // Eliminar el usuario (auditoría se registra en el repositorio)
            return await _repository.DeleteAsync(usuarioLoginId);
        }
    }
}
