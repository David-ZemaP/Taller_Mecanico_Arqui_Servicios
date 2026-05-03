using TallerMecanico.Core.Interfaces;
using TallerMecanico.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Taller Mecánico API", Version = "v1" });
});

// Registrar servicios como Singleton para mantener datos en memoria
builder.Services.AddSingleton<IClienteService, ClienteService>();
builder.Services.AddSingleton<IVehiculoService, VehiculoService>();
builder.Services.AddSingleton<IServicioService, ServicioService>();
builder.Services.AddSingleton<IOrdenTrabajoService, OrdenTrabajoService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Taller Mecánico API v1"));

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
