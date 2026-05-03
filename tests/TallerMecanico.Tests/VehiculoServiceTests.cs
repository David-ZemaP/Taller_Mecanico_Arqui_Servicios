using TallerMecanico.Core.Models;
using TallerMecanico.Services;

namespace TallerMecanico.Tests;

public class VehiculoServiceTests
{
    private readonly VehiculoService _service = new();

    [Fact]
    public void Crear_DebeAsignarIdYRetornarVehiculo()
    {
        var vehiculo = new Vehiculo { Marca = "Toyota", Modelo = "Corolla", Anio = 2020, Placa = "ABC123", ClienteId = 1 };

        var resultado = _service.Crear(vehiculo);

        Assert.NotNull(resultado);
        Assert.True(resultado.Id > 0);
        Assert.Equal("Toyota", resultado.Marca);
    }

    [Fact]
    public void ObtenerPorCliente_RetornaVehiculosDelCliente()
    {
        _service.Crear(new Vehiculo { ClienteId = 10, Marca = "Honda", Modelo = "Civic", Anio = 2019, Placa = "X1" });
        _service.Crear(new Vehiculo { ClienteId = 10, Marca = "Ford", Modelo = "Focus", Anio = 2021, Placa = "X2" });
        _service.Crear(new Vehiculo { ClienteId = 20, Marca = "Nissan", Modelo = "Sentra", Anio = 2018, Placa = "Y1" });

        var resultado = _service.ObtenerPorCliente(10).ToList();

        Assert.Equal(2, resultado.Count);
        Assert.All(resultado, v => Assert.Equal(10, v.ClienteId));
    }

    [Fact]
    public void Actualizar_CuandoExiste_ActualizaVehiculo()
    {
        var vehiculo = _service.Crear(new Vehiculo { Marca = "Kia", Modelo = "Rio", Anio = 2017, Placa = "KIA001", ClienteId = 1 });
        var actualizado = new Vehiculo { Marca = "Kia", Modelo = "Sportage", Anio = 2022, Placa = "KIA001", Color = "Rojo" };

        var resultado = _service.Actualizar(vehiculo.Id, actualizado);

        Assert.NotNull(resultado);
        Assert.Equal("Sportage", resultado.Modelo);
        Assert.Equal("Rojo", resultado.Color);
    }

    [Fact]
    public void Eliminar_CuandoExiste_RetornaTrue()
    {
        var vehiculo = _service.Crear(new Vehiculo { Marca = "BMW", Modelo = "Serie 3", Anio = 2023, Placa = "BMW001" });

        var resultado = _service.Eliminar(vehiculo.Id);

        Assert.True(resultado);
        Assert.Null(_service.ObtenerPorId(vehiculo.Id));
    }
}
