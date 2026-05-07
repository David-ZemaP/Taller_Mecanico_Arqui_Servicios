using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

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

// Infrastructure
builder.Services.AddScoped<Taller_Mecanico_Users.Framework.Persistence.ISqlConnectionFactory,
    Taller_Mecanico_Users.App.Infrastructure.SqlConnectionFactory>();

// Repositories
builder.Services.AddScoped<Taller_Mecanico_Users.Domain.Ports.IUsuarioLoginRepository,
    Taller_Mecanico_Users.Data.Repositories.UsuarioLoginRepository>();
builder.Services.AddScoped<Taller_Mecanico_Users.Domain.Ports.IReportRepository,
    Taller_Mecanico_Users.Data.Repositories.ReportRepository>();

// Services
builder.Services.AddScoped<Taller_Mecanico_Users.Framework.Services.IAuthenticationHelper,
    Taller_Mecanico_Users.App.Services.AuthenticationHelper>();
builder.Services.AddScoped<Taller_Mecanico_Users.Framework.Services.IMailSender,
    Taller_Mecanico_Users.App.Services.DummyMailSender>();
builder.Services.AddScoped<Taller_Mecanico_Users.App.Services.UsernameGenerator>();

builder.Services.AddHttpContextAccessor();

// Use Cases - Usuarios
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.CreateUserUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.GetUserByIdUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.GetUsersUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.UpdateUserUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.ChangePasswordUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.ResetPasswordUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.DeleteUserUseCase>();

// Use Cases - Reportes
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Reports.GetClientesVehiculosUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Reports.GetServiciosOrdenesUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Reports.GetServicesMetricsUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Reports.GetClienteReportUseCase>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
