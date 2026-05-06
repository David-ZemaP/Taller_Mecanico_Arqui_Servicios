using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 1. Leer configuración JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

// 2. Configurar Autenticación JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// 3. Dependency Injection registrations
builder.Services.AddScoped<Taller_Mecanico_Users.Framework.Persistence.ISqlConnectionFactory, 
    Taller_Mecanico_Users.App.Infrastructure.SqlConnectionFactory>();
builder.Services.AddScoped<Taller_Mecanico_Users.Domain.Ports.IUsuarioLoginRepository, 
    Taller_Mecanico_Users.Data.Repositories.UsuarioLoginRepository>();
builder.Services.AddScoped<Taller_Mecanico_Users.Framework.Services.IAuthenticationHelper, 
    Taller_Mecanico_Users.App.Services.AuthenticationHelper>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Taller_Mecanico_Users.Framework.Services.IMailSender, 
    Taller_Mecanico_Users.App.Services.DummyMailSender>();

// Use Cases - Usuarios
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.CreateUserUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.GetUserByIdUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.GetUsersUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.UpdateUserUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.ChangePasswordUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.ResetPasswordUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.DeleteUserUseCase>();

// Reports - Use Cases
builder.Services.AddScoped<Taller_Mecanico_Users.Domain.Ports.IReportRepository,
    Taller_Mecanico_Users.Data.Repositories.ReportRepository>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Reports.GetClienteReportUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Reports.GetServicesMetricsUseCase>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseHttpsRedirection();
app.UseCors("AllowAll");

// 4. ¡IMPORTANTE! UseAuthentication debe ir ANTES de UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
