using System;
using System.Threading.Tasks;
using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Ports;

namespace Taller_Mecanico_Users.UseCases.Users
{
    public class UpdateUserUseCase
    {
        private readonly IUsuarioLoginRepository _repository;

        public UpdateUserUseCase(IUsuarioLoginRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result> ExecuteAsync(int usuarioLoginId, string nuevoEmail, bool activo)
        {
            // 1. Verificar que el usuario exista
            var resultUsuario = await _repository.GetByIdAsync(usuarioLoginId);
            if (resultUsuario.IsFailure || resultUsuario.Value == null)
            {
                return Result.Failure(ErrorCodes.UsuarioLoginNotFound, "El usuario no existe.");
            }

            var usuario = resultUsuario.Value;

            // 2. Aplicar los cambios
            usuario.CambiarEmail(nuevoEmail);
            
            if (activo)
                usuario.Activar();
            else
                usuario.Desactivar();

            // 3. Guardar cambios en el repositorio (esto disparará la auditoría automáticamente)
            return await _repository.UpdateAsync(usuario);
        }
    }
}