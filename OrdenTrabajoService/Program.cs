using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;
using OrdenTrabajoService.Infrastructure.Clients;
using OrdenTrabajoService.Infrastructure.Persistence;
using OrdenTrabajoService.Infrastructure.Repositories;
using OrdenTrabajoService.Infrastructure.Services;
using Taller_Mecanico_Arqui.Application.Common;
using Taller_Mecanico_Arqui.Application.Facades;
using Taller_Mecanico_Arqui.Application.UseCases.Clientes;
using Taller_Mecanico_Arqui.Application.UseCases.Empleados;
using Taller_Mecanico_Arqui.Application.UseCases.OrdenTrabajo;
using Taller_Mecanico_Arqui.Application.UseCases.Productos;
using Taller_Mecanico_Arqui.Application.UseCases.Servicios;
using Taller_Mecanico_Arqui.Application.UseCases.Vehiculos;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration ---
builder.Services.AddSingleton<OrdenTrabajoServiceConfig>(sp =>
{
    var config = new OrdenTrabajoServiceConfig
    {
        DefaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing DefaultConnection connection string.")
    };
    return config;
});

// --- Infrastructure (Persistence) ---
builder.Services.AddSingleton<ISqlConnectionFactory>(sp =>
{
    return new NpgsqlConnectionFactory(builder.Configuration);
});

// --- Repositories ---
builder.Services.AddScoped<IOrdenTrabajoRepository, OrdenTrabajoService.Infrastructure.Repositories.OrdenTrabajoRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
builder.Services.AddScoped<IVehiculoRepository, VehiculoRepository>();
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IServicioRepository, ServicioRepository>();

// --- Generic Repositories (required by UseCases) ---
builder.Services.AddScoped<IRepository<Producto>, ProductoRepository>();
builder.Services.AddScoped<IRepository<Servicio>, ServicioRepository>();
builder.Services.AddScoped<IRepository<Cliente>, ClienteRepository>();
builder.Services.AddScoped<IRepository<Empleado>, EmpleadoRepository>();
builder.Services.AddScoped<IRepository<Vehiculo>, VehiculoRepository>();

// --- External Service Clients ---
var usersServiceUrl = builder.Configuration["UsersService:BaseUrl"] ?? "http://localhost:5005";
builder.Services.AddHttpClient<IUsersServiceClient, UsersServiceClient>(client =>
{
    client.BaseAddress = new Uri(usersServiceUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

// --- Use Cases: OrdenTrabajo ---
builder.Services.AddScoped<CreateOrdenTrabajoUseCase>();
builder.Services.AddScoped<UpdateOrdenTrabajoUseCase>();
builder.Services.AddScoped<GetOrdenTrabajoByIdUseCase>();
builder.Services.AddScoped<GetAllOrdenesTrabajoUseCase>();
builder.Services.AddScoped<SetAnulacionOrdenTrabajoUseCase>();

// --- Use Cases: Clientes ---
builder.Services.AddScoped<CreateClienteUseCase>();
builder.Services.AddScoped<UpdateClienteUseCase>();
builder.Services.AddScoped<GetClienteByIdUseCase>();
builder.Services.AddScoped<GetAllClientesUseCase>();
builder.Services.AddScoped<DeleteClienteUseCase>();

// --- Use Cases: Empleados ---
builder.Services.AddScoped<CreateEmpleadoUseCase>();
builder.Services.AddScoped<UpdateEmpleadoUseCase>();
builder.Services.AddScoped<GetEmpleadoByIdUseCase>();
builder.Services.AddScoped<GetAllEmpleadosUseCase>();
builder.Services.AddScoped<DeleteEmpleadoUseCase>();

// --- Use Cases: Vehiculos ---
builder.Services.AddScoped<CreateVehiculoUseCase>();
builder.Services.AddScoped<UpdateVehiculoUseCase>();
builder.Services.AddScoped<GetVehiculoByIdUseCase>();
builder.Services.AddScoped<GetAllVehiculosUseCase>();
builder.Services.AddScoped<GetVehiculosByClienteIdUseCase>();
builder.Services.AddScoped<DeleteVehiculoUseCase>();

// --- Use Cases: Productos ---
builder.Services.AddScoped<CreateProductoUseCase>();
builder.Services.AddScoped<UpdateProductoUseCase>();
builder.Services.AddScoped<GetProductoByIdUseCase>();
builder.Services.AddScoped<GetAllProductosUseCase>();
builder.Services.AddScoped<DeleteProductoUseCase>();

// --- Use Cases: Servicios ---
builder.Services.AddScoped<CreateServicioUseCase>();
builder.Services.AddScoped<UpdateServicioUseCase>();
builder.Services.AddScoped<GetServicioByIdUseCase>();
builder.Services.AddScoped<GetAllServiciosUseCase>();
builder.Services.AddScoped<DeleteServicioUseCase>();

// --- Facades: OrdenTrabajo ---
builder.Services.AddScoped<OrdenTrabajoCreate>();
builder.Services.AddScoped<OrdenTrabajoAnular>();
builder.Services.AddScoped<UpdateProductStocks>();

// --- JWT Authentication ---
var jwtSecret = builder.Configuration["JwtSettings:Secret"] 
    ?? throw new InvalidOperationException("JWT Secret no configurado.");
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "TallerMecanicoUsersApi";
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? "TallerMecanicoClients";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

// --- Current User Service ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
