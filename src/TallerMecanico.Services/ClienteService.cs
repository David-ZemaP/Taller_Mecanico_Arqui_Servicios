using TallerMecanico.Core.Interfaces;
using TallerMecanico.Core.Models;

namespace TallerMecanico.Services;

public class ClienteService : IClienteService
{
    private readonly List<Cliente> _clientes = new();
    private int _nextId = 1;

    public IEnumerable<Cliente> ObtenerTodos() => _clientes.AsReadOnly();

    public Cliente? ObtenerPorId(int id) =>
        _clientes.FirstOrDefault(c => c.Id == id);

    public Cliente Crear(Cliente cliente)
    {
        cliente.Id = _nextId++;
        cliente.FechaRegistro = DateTime.UtcNow;
        _clientes.Add(cliente);
        return cliente;
    }

    public Cliente? Actualizar(int id, Cliente cliente)
    {
        var existente = _clientes.FirstOrDefault(c => c.Id == id);
        if (existente is null) return null;

        existente.Nombre = cliente.Nombre;
        existente.Apellido = cliente.Apellido;
        existente.Telefono = cliente.Telefono;
        existente.Email = cliente.Email;
        existente.Direccion = cliente.Direccion;
        return existente;
    }

    public bool Eliminar(int id)
    {
        var cliente = _clientes.FirstOrDefault(c => c.Id == id);
        if (cliente is null) return false;
        _clientes.Remove(cliente);
        return true;
    }
}
