using GYM_System.Data;
using GYM_System.Services;
using Microsoft.EntityFrameworkCore;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// Configure SQL Server Database Context
builder.Services.AddDbContext<GymDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Google Sheets Service to the DI container as a Singleton
builder.Services.AddSingleton<GoogleSheetsService>();

// --- PDF Service Configuration ---
var pdfProvider = builder.Configuration["AppSettings:PdfProvider"] ?? "QuestPDF";

if (pdfProvider.Equals("wkhtmltopdf", StringComparison.OrdinalIgnoreCase))
{
    // wkhtmltopdf (direct exe) requires Razor rendering to HTML.
    builder.Services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
    builder.Services.AddScoped<IPdfService, WkHtmlToPdfService>();
}
else if (pdfProvider.Equals("playwright", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
    builder.Services.AddScoped<IPdfService, PlaywrightService>();
}
else
{
    builder.Services.AddScoped<IPdfService, QuestPdfService>();
}



var app = builder.Build();

// Apply pending EF Core migrations at startup
if (args.Contains("--migrate"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GymDbContext>();
    db.Database.Migrate();
    return;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
