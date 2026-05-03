using TallerMecanico.Core.Contracts;
using TallerMecanico.Core.Models;

namespace TallerMecanico.Services.Infrastructure.Repositories;

internal sealed class InMemoryClienteRepository : IClienteRepository
{
    private readonly InMemoryTallerContext _context;

    public InMemoryClienteRepository(InMemoryTallerContext context)
    {
        _context = context;
    }

    public Cliente Add(Cliente cliente)
    {
        var stored = new Cliente
        {
            Id = _context.NextClienteId++,
            Nombre = cliente.Nombre,
            Apellido = cliente.Apellido,
            Telefono = cliente.Telefono,
            Email = cliente.Email,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = cliente.CreatedByUserId
        };

        _context.Clientes.Add(stored);
        return stored;
    }

    public Cliente? GetById(int id) => _context.Clientes.FirstOrDefault(cliente => cliente.Id == id && !cliente.IsDeleted);

    public IEnumerable<Cliente> GetAll() => _context.Clientes.Where(cliente => !cliente.IsDeleted).ToList();

    public Cliente? Update(int id, Cliente cliente)
    {
        var stored = _context.Clientes.FirstOrDefault(item => item.Id == id && !item.IsDeleted);
        if (stored is null)
        {
            return null;
        }

        stored.Nombre = cliente.Nombre;
        stored.Apellido = cliente.Apellido;
        stored.Telefono = cliente.Telefono;
        stored.Email = cliente.Email;
        return stored;
    }

    public bool Delete(int id, int? usuarioId = null)
    {
        var stored = _context.Clientes.FirstOrDefault(item => item.Id == id && !item.IsDeleted);
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