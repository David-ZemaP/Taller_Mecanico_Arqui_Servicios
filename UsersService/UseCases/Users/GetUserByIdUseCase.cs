using Taller_Mecanico_Users.Domain.Common;
using Taller_Mecanico_Users.Domain.Entities;
using Taller_Mecanico_Users.Domain.Ports;

namespace Taller_Mecanico_Users.UseCases.Users
{
    public class GetUserByIdUseCase
    {
        private readonly IUsuarioLoginRepository _repository;

        public GetUserByIdUseCase(IUsuarioLoginRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<UsuarioLogin?>> ExecuteAsync(int usuarioLoginId)
        {
            return await _repository.GetByIdAsync(usuarioLoginId);
        }
    }
}