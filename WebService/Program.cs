using TallerMecanico.Services;
using TallerMecanico.Services.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSingleton<RepositoryCreator>(sp => new PostgreSqlRepositoryCreator(builder.Configuration.GetConnectionString("TallerMecanico")));
builder.Services.AddSingleton<ClienteService>();
builder.Services.AddSingleton<VehiculoService>();
builder.Services.AddSingleton<ProductoService>();
builder.Services.AddSingleton<ServicioCatalogoService>();
builder.Services.AddSingleton<OrdenTrabajoService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();

app.Run();
