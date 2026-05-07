using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// HTTP Clients para consumir APIs
var usersServiceUrl = builder.Configuration["UsersServiceBaseUrl"] ?? "http://localhost:5297";
builder.Services.AddHttpClient("UsersApi", c => c.BaseAddress = new Uri(usersServiceUrl));
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// Servicios de reportes
builder.Services.AddScoped<Taller_Mecanico_WebService.Helpers.AuditInfoHelper>();
builder.Services.AddScoped<Taller_Mecanico_WebService.Helpers.ReportFormatter>();
builder.Services.AddScoped<Taller_Mecanico_WebService.Services.Reports.IPDFReportService,
    Taller_Mecanico_WebService.Services.Reports.PDFReportService>();
builder.Services.AddScoped<Taller_Mecanico_WebService.Services.Reports.IExcelReportService,
    Taller_Mecanico_WebService.Services.Reports.ExcelReportService>();
builder.Services.AddScoped<Taller_Mecanico_WebService.Services.Reports.IChartService,
    Taller_Mecanico_WebService.Services.Reports.ChartService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapControllers();

app.Run();
