using TallerMecanico.Core.Models;
using TallerMecanico.Services;

namespace TallerMecanico.Tests;

public class ClienteServiceTests
{
    private readonly ClienteService _service = new();

    [Fact]
    public void Crear_DebeAsignarIdYRetornarCliente()
    {
        var cliente = new Cliente { Nombre = "Juan", Apellido = "Pérez", Telefono = "5551234", Email = "juan@ejemplo.com" };

        var resultado = _service.Crear(cliente);

        Assert.NotNull(resultado);
        Assert.True(resultado.Id > 0);
        Assert.Equal("Juan", resultado.Nombre);
    }

    [Fact]
    public void ObtenerPorId_CuandoExiste_RetornaCliente()
    {
        var cliente = _service.Crear(new Cliente { Nombre = "Ana", Apellido = "García" });

        var resultado = _service.ObtenerPorId(cliente.Id);

        Assert.NotNull(resultado);
        Assert.Equal("Ana", resultado.Nombre);
    }

    [Fact]
    public void ObtenerPorId_CuandoNoExiste_RetornaNull()
    {
        var resultado = _service.ObtenerPorId(9999);

        Assert.Null(resultado);
    }

    [Fact]
    public void ObtenerTodos_RetornaListaDeClientes()
    {
        _service.Crear(new Cliente { Nombre = "Pedro", Apellido = "López" });
        _service.Crear(new Cliente { Nombre = "María", Apellido = "Ruiz" });

        var resultado = _service.ObtenerTodos().ToList();

        Assert.True(resultado.Count >= 2);
    }

    [Fact]
    public void Actualizar_CuandoExiste_ActualizaYRetornaCliente()
    {
        var cliente = _service.Crear(new Cliente { Nombre = "Carlos", Apellido = "Soto" });
        var actualizado = new Cliente { Nombre = "Carlos", Apellido = "Soto Modificado", Telefono = "9999999" };

        var resultado = _service.Actualizar(cliente.Id, actualizado);

        Assert.NotNull(resultado);
        Assert.Equal("Soto Modificado", resultado.Apellido);
        Assert.Equal("9999999", resultado.Telefono);
    }

    [Fact]
    public void Actualizar_CuandoNoExiste_RetornaNull()
    {
        var resultado = _service.Actualizar(9999, new Cliente { Nombre = "X" });

        Assert.Null(resultado);
    }

    [Fact]
    public void Eliminar_CuandoExiste_RetornaTrue()
    {
        var cliente = _service.Crear(new Cliente { Nombre = "Luis", Apellido = "Mora" });

        var resultado = _service.Eliminar(cliente.Id);

        Assert.True(resultado);
        Assert.Null(_service.ObtenerPorId(cliente.Id));
    }

    [Fact]
    public void Eliminar_CuandoNoExiste_RetornaFalse()
    {
        var resultado = _service.Eliminar(9999);

        Assert.False(resultado);
    }
}
