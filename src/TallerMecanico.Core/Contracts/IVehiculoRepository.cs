using TallerMecanico.Core.Models;

namespace TallerMecanico.Core.Contracts;

public interface IVehiculoRepository
{
    Vehiculo Add(Vehiculo vehiculo);

    Vehiculo? GetById(int id);

    IEnumerable<Vehiculo> GetAll();

    IEnumerable<Vehiculo> GetByCliente(int clienteId);

    Vehiculo? Update(int id, Vehiculo vehiculo);

    bool Delete(int id, int? usuarioId = null);
}