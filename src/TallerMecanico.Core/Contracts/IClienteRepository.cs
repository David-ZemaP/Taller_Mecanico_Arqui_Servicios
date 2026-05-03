using TallerMecanico.Core.Models;

namespace TallerMecanico.Core.Contracts;

public interface IClienteRepository
{
    Cliente Add(Cliente cliente);

    Cliente? GetById(int id);

    IEnumerable<Cliente> GetAll();

    Cliente? Update(int id, Cliente cliente);

    bool Delete(int id, int? usuarioId = null);
}