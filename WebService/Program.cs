var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Registrar servicios de reportes
builder.Services.AddScoped<Taller_Mecanico_WebService.Helpers.AuditInfoHelper>();
builder.Services.AddScoped<Taller_Mecanico_WebService.Helpers.ReportFormatter>();
builder.Services.AddScoped<Taller_Mecanico_WebService.Services.Reports.IPDFReportService, Taller_Mecanico_WebService.Services.Reports.PDFReportService>();
builder.Services.AddScoped<Taller_Mecanico_WebService.Services.Reports.IExcelReportService, Taller_Mecanico_WebService.Services.Reports.ExcelReportService>();
builder.Services.AddScoped<Taller_Mecanico_WebService.Services.Reports.IChartService, Taller_Mecanico_WebService.Services.Reports.ChartService>();

// Registrar HttpClient para consumir APIs
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
app.MapControllers();

app.Run();
