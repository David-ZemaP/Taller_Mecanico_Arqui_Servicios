using TallerMecanico.Core.Models;
using TallerMecanico.Services;

namespace TallerMecanico.Tests;

public class OrdenTrabajoServiceTests
{
    private readonly Services.OrdenTrabajoService _service = new();

    [Fact]
    public void Crear_DebeAsignarIdEstadoPendienteYFechaCreacion()
    {
        var orden = new OrdenTrabajo { ClienteId = 1, VehiculoId = 1, Descripcion = "Revisión general" };

        var resultado = _service.Crear(orden);

        Assert.NotNull(resultado);
        Assert.True(resultado.Id > 0);
        Assert.Equal(EstadoOrden.Pendiente, resultado.Estado);
        Assert.NotEqual(default, resultado.FechaCreacion);
    }

    [Fact]
    public void CambiarEstado_CuandoExiste_ActualizaEstado()
    {
        var orden = _service.Crear(new OrdenTrabajo { ClienteId = 1, VehiculoId = 1 });

        var resultado = _service.CambiarEstado(orden.Id, EstadoOrden.EnProceso);

        Assert.NotNull(resultado);
        Assert.Equal(EstadoOrden.EnProceso, resultado.Estado);
    }

    [Fact]
    public void CambiarEstado_ACompletada_EstableceFechaCompletado()
    {
        var orden = _service.Crear(new OrdenTrabajo { ClienteId = 1, VehiculoId = 1 });

        var resultado = _service.CambiarEstado(orden.Id, EstadoOrden.Completada);

        Assert.NotNull(resultado);
        Assert.Equal(EstadoOrden.Completada, resultado.Estado);
        Assert.NotNull(resultado.FechaCompletado);
    }

    [Fact]
    public void ObtenerPorCliente_RetornaOrdenesDelCliente()
    {
        _service.Crear(new OrdenTrabajo { ClienteId = 5, VehiculoId = 1 });
        _service.Crear(new OrdenTrabajo { ClienteId = 5, VehiculoId = 2 });
        _service.Crear(new OrdenTrabajo { ClienteId = 7, VehiculoId = 3 });

        var resultado = _service.ObtenerPorCliente(5).ToList();

        Assert.Equal(2, resultado.Count);
        Assert.All(resultado, o => Assert.Equal(5, o.ClienteId));
    }

    [Fact]
    public void Total_CalculaCorrectamenteLaSumaDeServicios()
    {
        var orden = _service.Crear(new OrdenTrabajo
        {
            ClienteId = 1,
            VehiculoId = 1,
            Servicios = new List<Servicio>
            {
                new() { Id = 1, Nombre = "Cambio Aceite", Precio = 350m },
                new() { Id = 2, Nombre = "Alineación", Precio = 500m }
            }
        });

        Assert.Equal(850m, orden.Total);
    }
}
