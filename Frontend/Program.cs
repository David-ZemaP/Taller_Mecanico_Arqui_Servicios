using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Taller_Mecanico_Arqui.Frontend.Adapters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<JwtAuthHandler>();

// ============================================
// Authentication (Cookie-based for session)
// ============================================
builder.Services.AddAuthentication("FrontendScheme")
    .AddCookie("FrontendScheme", options =>
    {
        options.LoginPath = "/Login";
        options.AccessDeniedPath = "/AccessDenied";
    });

// ============================================
// HttpClient Factories para los servicios
// ============================================

// OrdenTrabajoService
builder.Services.AddHttpClient<IOrdenTrabajoAdapter, OrdenTrabajoAdapter>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:OrdenTrabajoServiceUrl"] ?? "http://localhost:5229";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).AddHttpMessageHandler<JwtAuthHandler>();

// Clientes
builder.Services.AddHttpClient<IClienteAdapter, ClienteAdapter>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:ClientesServiceUrl"] ?? "http://localhost:5229";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).AddHttpMessageHandler<JwtAuthHandler>();

// Empleados
builder.Services.AddHttpClient<IEmpleadoAdapter, EmpleadoAdapter>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:EmpleadosServiceUrl"] ?? "http://localhost:5229";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).AddHttpMessageHandler<JwtAuthHandler>();

// Vehiculos
builder.Services.AddHttpClient<IVehiculoAdapter, VehiculoAdapter>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:VehiculosServiceUrl"] ?? "http://localhost:5229";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).AddHttpMessageHandler<JwtAuthHandler>();

builder.Services.AddHttpClient<ICatalogosVehiculosAdapter, CatalogosVehiculosAdapter>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:VehiculosServiceUrl"] ?? "http://localhost:5229";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).AddHttpMessageHandler<JwtAuthHandler>();

// Productos
builder.Services.AddHttpClient<IProductoAdapter, ProductoAdapter>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:ProductosServiceUrl"] ?? "http://localhost:5229";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).AddHttpMessageHandler<JwtAuthHandler>();

// Servicios
builder.Services.AddHttpClient<IServicioAdapter, ServicioAdapter>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:ServiciosServiceUrl"] ?? "http://localhost:5229";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
}).AddHttpMessageHandler<JwtAuthHandler>();

// UsersService
builder.Services.AddHttpClient<IUsersServiceAdapter, UsersServiceAdapter>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:UsersServiceUrl"] ?? "http://localhost:5297";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();