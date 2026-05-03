var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/api/users", () => new[]
{
    new { Id = 1, UserName = "admin", DisplayName = "Administrador" },
    new { Id = 2, UserName = "operador", DisplayName = "Operador de taller" }
});

app.Run();
