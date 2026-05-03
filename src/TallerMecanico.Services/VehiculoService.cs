using TallerMecanico.Core.Interfaces;
using TallerMecanico.Core.Models;

namespace TallerMecanico.Services;

public class VehiculoService : IVehiculoService
{
    private readonly List<Vehiculo> _vehiculos = new();
    private int _nextId = 1;

    public IEnumerable<Vehiculo> ObtenerTodos() => _vehiculos.AsReadOnly();

    public IEnumerable<Vehiculo> ObtenerPorCliente(int clienteId) =>
        _vehiculos.Where(v => v.ClienteId == clienteId);

    public Vehiculo? ObtenerPorId(int id) =>
        _vehiculos.FirstOrDefault(v => v.Id == id);

    public Vehiculo Crear(Vehiculo vehiculo)
    {
        vehiculo.Id = _nextId++;
        _vehiculos.Add(vehiculo);
        return vehiculo;
    }

    public Vehiculo? Actualizar(int id, Vehiculo vehiculo)
    {
        var existente = _vehiculos.FirstOrDefault(v => v.Id == id);
        if (existente is null) return null;

        existente.ClienteId = vehiculo.ClienteId;
        existente.Marca = vehiculo.Marca;
        existente.Modelo = vehiculo.Modelo;
        existente.Anio = vehiculo.Anio;
        existente.Placa = vehiculo.Placa;
        existente.Color = vehiculo.Color;
        existente.NumeroVin = vehiculo.NumeroVin;
        return existente;
    }

    public bool Eliminar(int id)
    {
        var vehiculo = _vehiculos.FirstOrDefault(v => v.Id == id);
        if (vehiculo is null) return false;
        _vehiculos.Remove(vehiculo);
        return true;
    }
}
