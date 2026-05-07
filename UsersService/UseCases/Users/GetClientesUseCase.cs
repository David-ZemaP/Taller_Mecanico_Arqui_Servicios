using Taller_Mecanico_Users.Domain.Entities;
using Taller_Mecanico_Users.Domain.Ports;

namespace Taller_Mecanico_Users.UseCases.Users
{
    public class GetClientesUseCase
    {
        private readonly IUsuarioLoginRepository _repository;

        public GetClientesUseCase(IUsuarioLoginRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<UsuarioLogin>> ExecuteAsync()
        {
            var usuarios = await _repository.GetAllAsync();
            return usuarios.Where(u => u.EsCliente);
        }
    }
}
