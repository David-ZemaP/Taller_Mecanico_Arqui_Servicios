using System;
using System.Threading.Tasks;
using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Entities;
using Taller_Mecanico_Users.Domain.Ports;
using Taller_Mecanico_Users.Application.Services;

namespace Taller_Mecanico_Users.UseCases.Users
{
    public class UpdateUserUseCase
    {
        private readonly IUsuarioLoginRepository _repository;
        private readonly IAuthenticationHelper _authHelper;

        public UpdateUserUseCase(IUsuarioLoginRepository repository, IAuthenticationHelper authHelper)
        {
            _repository = repository;
            _authHelper = authHelper;
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

            // Obtener el actor para auditoría antes de modificar
            var actor = _authHelper.GetCurrentAuditActor();

            // 1.1 Evitar duplicados de email en actualización
            var existingByEmail = await _repository.GetByEmailAsync(nuevoEmail);
            if (existingByEmail != null && existingByEmail.UsuarioLoginId != usuarioLoginId)
            {
                return Result.Failure(ErrorCodes.UsuarioEmailDuplicado, "El email ya está registrado.");
            }

            // 2. Aplicar los cambios
            var emailResult = usuario.CambiarEmail(nuevoEmail);
            if (emailResult.IsFailure)
            {
                return emailResult;
            }
            
            if (activo)
            {
                var activateResult = usuario.Activar();
                if (activateResult.IsFailure)
                {
                    return activateResult;
                }
            }
            else
            {
                var deactivateResult = usuario.Desactivar();
                if (deactivateResult.IsFailure)
                {
                    return deactivateResult;
                }
                usuario.RegistrarEliminacion(actor);
            }

// 3. Registrar auditoría de actualización (solo si cambió email)
            if (nuevoEmail != usuario.Email)
            {
                usuario.RegistrarActualizacion(actor);
            }

            // 4. Guardar cambios en el repositorio
            return await _repository.UpdateAsync(usuario);
        }
    }
}