using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Taller_Mecanico_Users.App.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();


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


builder.Services.AddScoped<Taller_Mecanico_Users.Framework.Persistence.ISqlConnectionFactory, 
    Taller_Mecanico_Users.App.Infrastructure.SqlConnectionFactory>();


builder.Services.AddScoped<Taller_Mecanico_Users.Framework.Services.IAuthenticationHelper, 
    Taller_Mecanico_Users.App.Services.AuthenticationHelper>();
builder.Services.AddHttpContextAccessor();


builder.Services.AddSingleton<Taller_Mecanico_Users.Framework.Services.SmtpSettings>();
var smtpEnabled = builder.Configuration.GetValue<bool>("Smtp:Enabled");
if (smtpEnabled)
{
    builder.Services.AddScoped<Taller_Mecanico_Users.App.Services.SmtpMailSender>();
    builder.Services.AddScoped<Taller_Mecanico_Users.Framework.Services.IMailSender>(sp => sp.GetRequiredService<Taller_Mecanico_Users.App.Services.SmtpMailSender>());
    builder.Services.AddScoped<Taller_Mecanico_Users.Domain.Ports.IMailSender>(sp => sp.GetRequiredService<Taller_Mecanico_Users.App.Services.SmtpMailSender>());
}
else
{
    builder.Services.AddScoped<Taller_Mecanico_Users.App.Services.DummyMailSender>();
    builder.Services.AddScoped<Taller_Mecanico_Users.Framework.Services.IMailSender>(sp => sp.GetRequiredService<Taller_Mecanico_Users.App.Services.DummyMailSender>());
    builder.Services.AddScoped<Taller_Mecanico_Users.Domain.Ports.IMailSender>(sp => sp.GetRequiredService<Taller_Mecanico_Users.App.Services.DummyMailSender>());
}

builder.Services.AddScoped<Taller_Mecanico_Users.Framework.Services.IPasswordSecurity, 
    Taller_Mecanico_Users.Framework.Services.PasswordSecurityService>();
builder.Services.AddScoped<Taller_Mecanico_Users.Domain.Ports.IPasswordSecurity, 
    Taller_Mecanico_Users.Framework.Services.PasswordSecurityService>();


builder.Services.AddScoped<Taller_Mecanico_Users.Framework.Services.IPasswordHasher, 
    Taller_Mecanico_Users.Framework.Services.BcryptPasswordHasher>();
builder.Services.AddScoped<Taller_Mecanico_Users.Domain.Ports.IPasswordHasher, 
    Taller_Mecanico_Users.Framework.Services.BcryptPasswordHasher>();


builder.Services.AddSingleton<Taller_Mecanico_Users.Framework.Services.IJwtSettings, 
    Taller_Mecanico_Users.Framework.Services.JwtSettings>();
builder.Services.AddScoped<Taller_Mecanico_Users.Framework.Services.IJwtTokenGenerator, 
    Taller_Mecanico_Users.Framework.Services.JwtTokenGenerator>();


builder.Services.AddScoped<Taller_Mecanico_Users.Framework.Services.IAuditService, 
    Taller_Mecanico_Users.Framework.Services.AuditService>();



builder.Services.AddScoped<Taller_Mecanico_Users.Domain.Ports.IUsuarioLoginRepository, 
    Taller_Mecanico_Users.Data.Repositories.UsuarioLoginRepository>();


builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.LoginUseCase>();


builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.CreateUserUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.GetUserByIdUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.GetUsersUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.UpdateUserUseCase>();


builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.ChangePasswordUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.ResetPasswordUseCase>();


builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.DeleteUserUseCase>();

var app = builder.Build();

app.UseHttpsRedirection();


app.UseAuthentication(); 
app.UseRequirePasswordChange();
app.UseAuthorization();

app.MapControllers();

app.Run();