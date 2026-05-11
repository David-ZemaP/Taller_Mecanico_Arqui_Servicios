using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OrdenTrabajoService.Application.Facades;
using OrdenTrabajoService.Application.UseCases;
using OrdenTrabajoService.Domain.Entities;
using OrdenTrabajoService.Domain.Interfaces;
using OrdenTrabajoService.Infrastructure.Persistence;
using OrdenTrabajoService.Infrastructure.Repositories;
using OrdenTrabajoService.Infrastructure.Services;
using Taller_Mecanico_Users.Application.Persistence;
using Taller_Mecanico_Users.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));
builder.Configuration.AddEnvironmentVariables();

// JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException("JwtSettings:SecretKey no configurada.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Infrastructure - singleton (stateless, thread-safe)
builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
builder.Services.AddSingleton<OrdenTrabajoQueryService>();

// Infrastructure - scoped
builder.Services.AddScoped<IAuthenticationHelper, ApiAuthHelper>();
builder.Services.AddScoped<IOrdenTrabajoRepository, OrdenTrabajoRepository>();
builder.Services.AddScoped<IOrdenTrabajoCatalogoRepository, OrdenTrabajoCatalogoRepository>();
builder.Services.AddScoped<IRepository<Producto>, ProductoRepository>();
builder.Services.AddScoped<IRepository<Servicio>, ServicioRepository>();
builder.Services.AddScoped<IVehiculoRepository, VehiculoRepository>();
builder.Services.AddScoped<IRepository<Cliente>, ClienteRepository>();
builder.Services.AddScoped<IRepository<Marca>, MarcaRepository>();
builder.Services.AddScoped<IRepository<Modelo>, ModeloRepository>();
builder.Services.AddScoped<IRepository<ColorVehiculo>, ColorVehiculoRepository>();

// Use Cases
builder.Services.AddScoped<GetAllOrdenesTrabajoUseCase>();
builder.Services.AddScoped<GetOrdenTrabajoByIdUseCase>();
builder.Services.AddScoped<CreateOrdenTrabajoUseCase>();
builder.Services.AddScoped<UpdateOrdenTrabajoUseCase>();
builder.Services.AddScoped<SetAnulacionOrdenTrabajoUseCase>();

// Facades
builder.Services.AddScoped<OrdenTrabajoCreate>();
builder.Services.AddScoped<OrdenTrabajoAnular>();
builder.Services.AddScoped<UpdateProductStocks>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

