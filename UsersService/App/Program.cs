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
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.GetClientesUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.UpdateUserUseCase>();


builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.ChangePasswordUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.ResetPasswordUseCase>();


builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Users.DeleteUserUseCase>();

var app = builder.Build();

await SeedDefaultAdminAsync(app.Services);

app.UseHttpsRedirection();


app.UseAuthentication(); 
app.UseRequirePasswordChange();
app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task SeedDefaultAdminAsync(IServiceProvider services)
{
    const string adminEmail = "administrador.principal@taller.com";
    const string adminPassword = "ap100000";
    const int adminCi = 100000;

    try
    {
        using var scope = services.CreateScope();
        var connectionFactory = scope.ServiceProvider
            .GetRequiredService<Taller_Mecanico_Users.Framework.Persistence.ISqlConnectionFactory>();
        var passwordHasher = scope.ServiceProvider
            .GetRequiredService<Taller_Mecanico_Users.Domain.Ports.IPasswordHasher>();

        await using var conn = connectionFactory.CreateConnection();
        await conn.OpenAsync();

        // Skip if admin user already exists
        var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(1) FROM usuariologin WHERE email = @Email;";
        var p = checkCmd.CreateParameter(); p.ParameterName = "@Email"; p.Value = adminEmail;
        checkCmd.Parameters.Add(p);
        var count = Convert.ToInt64(await ((System.Data.Common.DbCommand)checkCmd).ExecuteScalarAsync());
        if (count > 0) return;

        // Get or create the admin employee (NivelAcceso = Gerente)
        var getEmpCmd = conn.CreateCommand();
        getEmpCmd.CommandText = "SELECT empleadoid FROM empleado WHERE ci = @Ci LIMIT 1;";
        var p2 = getEmpCmd.CreateParameter(); p2.ParameterName = "@Ci"; p2.Value = adminCi;
        getEmpCmd.Parameters.Add(p2);
        var empIdObj = await ((System.Data.Common.DbCommand)getEmpCmd).ExecuteScalarAsync();

        int empleadoId;
        if (empIdObj is null || empIdObj == DBNull.Value)
        {
            var insEmpCmd = conn.CreateCommand();
            insEmpCmd.CommandText = @"
                INSERT INTO empleado
                    (nombre, primerapellido, ci, telefono, fechacontratacion, tipoempleado, estadolaboral, nivelacceso)
                VALUES
                    ('Administrador', 'Principal', @Ci, 0, CURRENT_TIMESTAMP, 'Administrador', 'Activo', 'Gerente')
                RETURNING empleadoid;";
            var p3 = insEmpCmd.CreateParameter(); p3.ParameterName = "@Ci"; p3.Value = adminCi;
            insEmpCmd.Parameters.Add(p3);
            empleadoId = Convert.ToInt32(await ((System.Data.Common.DbCommand)insEmpCmd).ExecuteScalarAsync());
        }
        else
        {
            empleadoId = Convert.ToInt32(empIdObj);
        }

        // Create admin login user (RequiereCambioPassword = FALSE so no forced reset)
        var passwordHash = passwordHasher.HashPassword(adminPassword);
        var insUserCmd = conn.CreateCommand();
        insUserCmd.CommandText = @"
            INSERT INTO usuariologin
                (empleadoid, email, passwordhash, activo, requierecambiopassword, escliente)
            VALUES
                (@EmpleadoId, @Email, @PasswordHash, TRUE, FALSE, FALSE);";
        var pa = insUserCmd.CreateParameter(); pa.ParameterName = "@EmpleadoId"; pa.Value = empleadoId;
        var pb = insUserCmd.CreateParameter(); pb.ParameterName = "@Email";      pb.Value = adminEmail;
        var pc = insUserCmd.CreateParameter(); pc.ParameterName = "@PasswordHash"; pc.Value = passwordHash;
        insUserCmd.Parameters.Add(pa);
        insUserCmd.Parameters.Add(pb);
        insUserCmd.Parameters.Add(pc);
        await ((System.Data.Common.DbCommand)insUserCmd).ExecuteNonQueryAsync();

        Console.WriteLine("[Seed] Usuario administrador creado correctamente.");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"[Seed] No se pudo crear el usuario administrador: {ex.Message}");
    }
}