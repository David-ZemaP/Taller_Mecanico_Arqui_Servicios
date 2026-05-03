using TallerMecanico.Core.Contracts;
using TallerMecanico.Core.Models;

namespace TallerMecanico.Services.Infrastructure.Repositories;

internal sealed class InMemoryVehiculoRepository : IVehiculoRepository
{
    private readonly InMemoryTallerContext _context;

    public InMemoryVehiculoRepository(InMemoryTallerContext context)
    {
        _context = context;
    }

    public Vehiculo Add(Vehiculo vehiculo)
    {
        var stored = new Vehiculo
        {
            Id = _context.NextVehiculoId++,
            ClienteId = vehiculo.ClienteId,
            Marca = vehiculo.Marca,
            Modelo = vehiculo.Modelo,
            Anio = vehiculo.Anio,
            Placa = vehiculo.Placa,
            Color = vehiculo.Color,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = vehiculo.CreatedByUserId
        };

        _context.Vehiculos.Add(stored);
        return stored;
    }

    public Vehiculo? GetById(int id) => _context.Vehiculos.FirstOrDefault(vehiculo => vehiculo.Id == id && !vehiculo.IsDeleted);

    public IEnumerable<Vehiculo> GetAll() => _context.Vehiculos.Where(vehiculo => !vehiculo.IsDeleted).ToList();

    public IEnumerable<Vehiculo> GetByCliente(int clienteId) => _context.Vehiculos.Where(vehiculo => vehiculo.ClienteId == clienteId && !vehiculo.IsDeleted).ToList();

    public Vehiculo? Update(int id, Vehiculo vehiculo)
    {
        var stored = _context.Vehiculos.FirstOrDefault(item => item.Id == id && !item.IsDeleted);
        if (stored is null)
        {
            return null;
        }

        stored.ClienteId = vehiculo.ClienteId;
        stored.Marca = vehiculo.Marca;
        stored.Modelo = vehiculo.Modelo;
        stored.Anio = vehiculo.Anio;
        stored.Placa = vehiculo.Placa;
        stored.Color = vehiculo.Color;
        return stored;
    }

    public bool Delete(int id, int? usuarioId = null)
    {
        var stored = _context.Vehiculos.FirstOrDefault(item => item.Id == id && !item.IsDeleted);
        if (stored is null)
        {
            return false;
        }

        stored.IsDeleted = true;
        stored.DeletedAt = DateTime.UtcNow;
        stored.DeletedByUserId = usuarioId;
        return true;
    }
}