using WebService.Adapters;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Autenticación con Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccesoDenegado";
    });

builder.Services.AddAuthorization();

// HttpClient para OrdenTrabajoService (S1)
var s1Url = builder.Configuration["ServiceUrls:OrdenTrabajoService"]
    ?? throw new InvalidOperationException("ServiceUrls:OrdenTrabajoService no configurada.");

builder.Services.AddHttpClient<OrdenTrabajoAdapter>(client =>
{
    client.BaseAddress = new Uri(s1Url);
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
