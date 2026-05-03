using TallerMecanico.Core.Models;
using TallerMecanico.Services;
using TallerMecanico.Services.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<RepositoryCreator>(sp => new PostgreSqlRepositoryCreator(builder.Configuration.GetConnectionString("TallerMecanico")));
builder.Services.AddSingleton<ClienteService>();
builder.Services.AddSingleton<VehiculoService>();
builder.Services.AddSingleton<ProductoService>();
builder.Services.AddSingleton<ServicioCatalogoService>();
builder.Services.AddSingleton<OrdenTrabajoService>();

var app = builder.Build();

app.MapGet("/api/clientes", (ClienteService service) => service.ObtenerTodos());
app.MapGet("/api/clientes/{id:int}", (ClienteService service, int id) => service.ObtenerPorId(id));
app.MapPost("/api/clientes", (ClienteService service, Cliente cliente) => service.Crear(cliente));
app.MapPut("/api/clientes/{id:int}", (ClienteService service, int id, Cliente cliente) => service.Actualizar(id, cliente));
app.MapDelete("/api/clientes/{id:int}", (ClienteService service, int id) => service.Eliminar(id, usuarioId: 1));

app.MapGet("/api/vehiculos", (VehiculoService service) => service.ObtenerTodos());
app.MapGet("/api/vehiculos/{id:int}", (VehiculoService service, int id) => service.ObtenerPorId(id));
app.MapGet("/api/vehiculos/cliente/{clienteId:int}", (VehiculoService service, int clienteId) => service.ObtenerPorCliente(clienteId));
app.MapPost("/api/vehiculos", (VehiculoService service, Vehiculo vehiculo) => service.Crear(vehiculo));
app.MapPut("/api/vehiculos/{id:int}", (VehiculoService service, int id, Vehiculo vehiculo) => service.Actualizar(id, vehiculo));
app.MapDelete("/api/vehiculos/{id:int}", (VehiculoService service, int id) => service.Eliminar(id, usuarioId: 1));

app.MapGet("/api/productos", (ProductoService service) => service.ObtenerTodos());
app.MapGet("/api/productos/{id:int}", (ProductoService service, int id) => service.ObtenerPorId(id));

app.MapGet("/api/servicios", (ServicioCatalogoService service) => service.ObtenerTodos());

app.MapGet("/api/ordenes", (OrdenTrabajoService service) => service.ObtenerTodos());
app.MapGet("/api/ordenes/{id:int}", (OrdenTrabajoService service, int id) => service.ObtenerPorId(id));
app.MapGet("/api/ordenes/cliente/{clienteId:int}", (OrdenTrabajoService service, int clienteId) => service.ObtenerPorCliente(clienteId));
app.MapPost("/api/ordenes", (OrdenTrabajoService service, OrdenTrabajo ordenTrabajo) => service.Crear(ordenTrabajo, usuarioId: 1));
app.MapPut("/api/ordenes/{id:int}", (OrdenTrabajoService service, int id, OrdenTrabajo ordenTrabajo) => service.Actualizar(id, ordenTrabajo));
app.MapPatch("/api/ordenes/{id:int}/estado", (OrdenTrabajoService service, int id, EstadoOrden estado) => service.CambiarEstado(id, estado, usuarioId: 1));
app.MapPost("/api/ordenes/{id:int}/anular", (OrdenTrabajoService service, int id) => service.Anular(id, usuarioId: 1));

app.Run();