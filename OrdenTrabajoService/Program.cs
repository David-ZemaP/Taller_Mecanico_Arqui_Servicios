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

app.MapGet("/api/ordenes", (OrdenTrabajoService service) => service.ObtenerTodos());
app.MapGet("/api/ordenes/{id:int}", (OrdenTrabajoService service, int id) => service.ObtenerPorId(id));
app.MapGet("/api/clientes", (ClienteService service) => service.ObtenerTodos());
app.MapGet("/api/vehiculos", (VehiculoService service) => service.ObtenerTodos());
app.MapPost("/api/ordenes", (OrdenTrabajoService service, OrdenTrabajo ordenTrabajo) => service.Crear(ordenTrabajo, usuarioId: 1));
app.MapPost("/api/ordenes/{id:int}/anular", (OrdenTrabajoService service, int id) => service.Anular(id, usuarioId: 1));

app.Run();
