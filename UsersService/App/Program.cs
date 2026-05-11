using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Taller_Mecanico_Users.App.Middleware;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

Env.Load(Path.Combine(AppContext.BaseDirectory, ".env"));

// Add environment variables to configuration (after loading .env)
builder.Configuration.AddEnvironmentVariables();

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


builder.Services.AddScoped<Taller_Mecanico_Users.Application.Persistence.ISqlConnectionFactory, 
    Taller_Mecanico_Users.App.Infrastructure.SqlConnectionFactory>();


builder.Services.AddScoped<Taller_Mecanico_Users.Application.Services.IAuthenticationHelper, 
    Taller_Mecanico_Users.App.Services.AuthenticationHelper>();
builder.Services.AddHttpContextAccessor();


builder.Services.AddSingleton<Taller_Mecanico_Users.Application.Services.SmtpSettings>();
var smtpEnabled = builder.Configuration.GetValue<bool>("Smtp:Enabled");
if (smtpEnabled)
{
    builder.Services.AddScoped<Taller_Mecanico_Users.Application.Services.SmtpMailSender>();
    builder.Services.AddScoped<Taller_Mecanico_Users.Application.Services.IMailSender>(sp => sp.GetRequiredService<Taller_Mecanico_Users.Application.Services.SmtpMailSender>());
    builder.Services.AddScoped<Taller_Mecanico_Users.Domain.Ports.IMailSender>(sp => sp.GetRequiredService<Taller_Mecanico_Users.Application.Services.SmtpMailSender>());
}
else
{
    builder.Services.AddScoped<Taller_Mecanico_Users.Application.Services.DummyMailSender>();
    builder.Services.AddScoped<Taller_Mecanico_Users.Application.Services.IMailSender>(sp => sp.GetRequiredService<Taller_Mecanico_Users.Application.Services.DummyMailSender>());
    builder.Services.AddScoped<Taller_Mecanico_Users.Domain.Ports.IMailSender>(sp => sp.GetRequiredService<Taller_Mecanico_Users.Application.Services.DummyMailSender>());
}

builder.Services.AddScoped<Taller_Mecanico_Users.Application.Services.IPasswordSecurity, 
    Taller_Mecanico_Users.Application.Services.PasswordSecurityService>();
builder.Services.AddScoped<Taller_Mecanico_Users.Domain.Ports.IPasswordSecurity, 
    Taller_Mecanico_Users.Application.Services.PasswordSecurityService>();


builder.Services.AddScoped<Taller_Mecanico_Users.Application.Services.IPasswordHasher, 
    Taller_Mecanico_Users.Application.Services.BcryptPasswordHasher>();
builder.Services.AddScoped<Taller_Mecanico_Users.Domain.Ports.IPasswordHasher, 
    Taller_Mecanico_Users.Application.Services.BcryptPasswordHasher>();


builder.Services.AddSingleton<Taller_Mecanico_Users.Application.Services.IJwtSettings, 
    Taller_Mecanico_Users.Application.Services.JwtSettings>();
builder.Services.AddScoped<Taller_Mecanico_Users.Application.Services.IJwtTokenGenerator, 
    Taller_Mecanico_Users.Application.Services.JwtTokenGenerator>();


builder.Services.AddScoped<Taller_Mecanico_Users.Application.Services.IAuditService, 
    Taller_Mecanico_Users.Application.Services.AuditService>();



builder.Services.AddScoped<Taller_Mecanico_Users.Domain.Ports.IUsuarioLoginRepository,
    Taller_Mecanico_Users.Data.Repositories.UsuarioLoginRepository>();

builder.Services.AddScoped<Taller_Mecanico_Users.Domain.Ports.IEmpleadoRepository,
    Taller_Mecanico_Users.Data.Repositories.EmpleadoRepository>();

builder.Services.AddScoped<Taller_Mecanico_Users.Domain.Ports.IRolRepository,
    Taller_Mecanico_Users.Data.Repositories.RolRepository>();

builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Empleados.GetEmpleadosUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Empleados.CreateEmpleadoUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Empleados.UpdateEmpleadoUseCase>();
builder.Services.AddScoped<Taller_Mecanico_Users.UseCases.Empleados.DeleteEmpleadoUseCase>();


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
            .GetRequiredService<Taller_Mecanico_Users.Application.Persistence.ISqlConnectionFactory>();
        var passwordHasher = scope.ServiceProvider
            .GetRequiredService<Taller_Mecanico_Users.Domain.Ports.IPasswordHasher>();

        await using var conn = connectionFactory.CreateConnection();
        await conn.OpenAsync();

        // Ensure admin employee exists and is active (CI = 100000)
        var getEmpCmd = conn.CreateCommand();
        getEmpCmd.CommandText = "SELECT empleadoid, isdeleted FROM empleado WHERE ci = @Ci LIMIT 1;";
        var p2 = getEmpCmd.CreateParameter(); p2.ParameterName = "@Ci"; p2.Value = adminCi;
        getEmpCmd.Parameters.Add(p2);

        int empleadoId;
        using (var empReader = await ((System.Data.Common.DbCommand)getEmpCmd).ExecuteReaderAsync())
        {
            if (await empReader.ReadAsync())
            {
                empleadoId = empReader.GetInt32(0);
                var isDeleted = empReader.GetBoolean(1);
                await empReader.CloseAsync();
                if (isDeleted)
                {
                    var restoreCmd = conn.CreateCommand();
                    restoreCmd.CommandText = "UPDATE empleado SET isdeleted = FALSE, estadolaboral = 'Activo' WHERE empleadoid = @Id;";
                    var pr = restoreCmd.CreateParameter(); pr.ParameterName = "@Id"; pr.Value = empleadoId;
                    restoreCmd.Parameters.Add(pr);
                    await ((System.Data.Common.DbCommand)restoreCmd).ExecuteNonQueryAsync();
                    Console.WriteLine("[Seed] Empleado administrador restaurado (estaba eliminado).");
                }
            }
            else
            {
                await empReader.CloseAsync();
                var insEmpCmd = conn.CreateCommand();
                insEmpCmd.CommandText = @"
                    INSERT INTO empleado
                        (nombre, primerapellido, ci, telefono, fechacontratacion, tipoempleado, estadolaboral, nivelacceso, isdeleted)
                    VALUES
                        ('Administrador', 'Principal', @Ci, 0, CURRENT_TIMESTAMP, 'Administrador', 'Activo', 'Gerente', FALSE)
                    RETURNING empleadoid;";
                var p3 = insEmpCmd.CreateParameter(); p3.ParameterName = "@Ci"; p3.Value = adminCi;
                insEmpCmd.Parameters.Add(p3);
                empleadoId = Convert.ToInt32(await ((System.Data.Common.DbCommand)insEmpCmd).ExecuteScalarAsync());
                Console.WriteLine("[Seed] Empleado administrador creado.");
            }
        }

        // Ensure admin login user exists
        var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT usuariologinid FROM usuariologin WHERE email = @Email;";
        var p = checkCmd.CreateParameter(); p.ParameterName = "@Email"; p.Value = adminEmail;
        checkCmd.Parameters.Add(p);
        var existingUserId = await ((System.Data.Common.DbCommand)checkCmd).ExecuteScalarAsync();

        // Get rolId for "Gerente"
        var getRolCmd = conn.CreateCommand();
        getRolCmd.CommandText = "SELECT rolid FROM rol WHERE LOWER(nombre) = 'gerente' LIMIT 1;";
        var rolIdObj = await ((System.Data.Common.DbCommand)getRolCmd).ExecuteScalarAsync();
        var gerenteRolId = rolIdObj != null ? Convert.ToInt32(rolIdObj) : (int?)null;

        if (existingUserId != null)
        {
            // Update existing user with rol de Gerente
            var updateCmd = conn.CreateCommand();
            updateCmd.CommandText = "UPDATE usuariologin SET rolid = @RolId WHERE usuariologinid = @UserId;";
            var up = updateCmd.CreateParameter(); up.ParameterName = "@RolId"; up.Value = gerenteRolId ?? (object)DBNull.Value;
            var uid = updateCmd.CreateParameter(); uid.ParameterName = "@UserId"; uid.Value = existingUserId;
            updateCmd.Parameters.Add(up);
            updateCmd.Parameters.Add(uid);
            await ((System.Data.Common.DbCommand)updateCmd).ExecuteNonQueryAsync();
            Console.WriteLine("[Seed] Usuario administrador actualizado con rol de Gerente.");
            return;
        }

        // Create admin login user with rol de Gerente
        var passwordHash = passwordHasher.HashPassword(adminPassword);
        var insUserCmd = conn.CreateCommand();
        insUserCmd.CommandText = @"
            INSERT INTO usuariologin
                (empleadoid, email, passwordhash, activo, requierecambiopassword, escliente, rolid)
            VALUES
                (@EmpleadoId, @Email, @PasswordHash, TRUE, FALSE, FALSE, @RolId);";
        var pa = insUserCmd.CreateParameter(); pa.ParameterName = "@EmpleadoId"; pa.Value = empleadoId;
        var pb = insUserCmd.CreateParameter(); pb.ParameterName = "@Email";      pb.Value = adminEmail;
        var pc = insUserCmd.CreateParameter(); pc.ParameterName = "@PasswordHash"; pc.Value = passwordHash;
        var pd = insUserCmd.CreateParameter(); pd.ParameterName = "@RolId"; pd.Value = gerenteRolId ?? (object)DBNull.Value;
        insUserCmd.Parameters.Add(pa);
        insUserCmd.Parameters.Add(pb);
        insUserCmd.Parameters.Add(pc);
        insUserCmd.Parameters.Add(pd);
        await ((System.Data.Common.DbCommand)insUserCmd).ExecuteNonQueryAsync();

        Console.WriteLine("[Seed] Usuario administrador creado correctamente con rol de Gerente.");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"[Seed] No se pudo crear el usuario administrador: {ex.Message}");
    }
}