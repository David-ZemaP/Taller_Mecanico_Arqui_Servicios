using Microsoft.AspNetCore.Authentication.Cookies;
using WebService.Adapters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccesoDenegado";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// HttpClient para OrdenTrabajoService (S1)
var s1Url = builder.Configuration["ServiceUrls:OrdenTrabajoService"]
    ?? throw new InvalidOperationException("ServiceUrls:OrdenTrabajoService no configurada.");

builder.Services.AddHttpClient<OrdenTrabajoAdapter>(client =>
{
    client.BaseAddress = new Uri(s1Url);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var usersServiceUrl = builder.Configuration["ServiceUrls:UsersService"]
    ?? throw new InvalidOperationException("ServiceUrls:UsersService no configurada.");

builder.Services.AddHttpClient<UsersServiceAdapter>(client =>
{
    client.BaseAddress = new Uri(usersServiceUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<ClientesAdapter>(client =>
{
    client.BaseAddress = new Uri(usersServiceUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddHttpClient<EmpleadosAdapter>(client =>
{
    client.BaseAddress = new Uri(usersServiceUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
