using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Taller_Mecanico_Arqui.Application.Facades;
using Taller_Mecanico_Arqui.Application.UseCases.OrdenTrabajo;
using Taller_Mecanico_Arqui.Domain.Entities;
using Taller_Mecanico_Arqui.Domain.Ports;
using Taller_Mecanico_Arqui.Infrastructure.Persistence;
using Taller_Mecanico_Arqui.Infrastructure.Persistence.Repositories;
using Taller_Mecanico_Arqui.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

// JWT Authentication (same key as UsersService)
var jwtKey = builder.Configuration["JwtSettings:Secret"] ?? "SuperSecretaClaveDePruebaParaTallerMecanico2026DebeMasLargoPorSeguridadMinimo32!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// DB connection factory
var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=host.docker.internal;Port=5433;Database=TallerMecanico;Username=admin;Password=tallermecanico2026";
builder.Services.AddSingleton<ISqlConnectionFactory>(new NpgsqlConnectionFactory(connStr));

// Infrastructure helpers
builder.Services.AddScoped<SqlEntityQueryService>();
builder.Services.AddScoped<AuthenticationHelper>();

// Repositories (Factory Method pattern — creators expose IRepository<T>)
builder.Services.AddScoped<IOrdenTrabajoRepository, OrdenTrabajoRepository>();
builder.Services.AddScoped<IVehiculoRepository, VehiculoRepository>();
builder.Services.AddScoped<IRepository<Producto>, ProductoRepository>();
builder.Services.AddScoped<IRepository<Servicio>, ServicioRepository>();

// Use Cases
builder.Services.AddScoped<CreateOrdenTrabajoUseCase>();
builder.Services.AddScoped<UpdateOrdenTrabajoUseCase>();
builder.Services.AddScoped<GetAllOrdenesTrabajoUseCase>();
builder.Services.AddScoped<GetOrdenTrabajoByIdUseCase>();
builder.Services.AddScoped<SetAnulacionOrdenTrabajoUseCase>();

// Facades
builder.Services.AddScoped<UpdateProductStocks>();
builder.Services.AddScoped<OrdenTrabajoCreate>();
builder.Services.AddScoped<OrdenTrabajoAnular>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
