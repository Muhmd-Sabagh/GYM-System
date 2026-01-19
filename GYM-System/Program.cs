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

// Add PdfService to the DI container as a Scoped service
builder.Services.AddScoped<PdfService>();

// Configure Kestrel to listen on port 5129 and any IP address
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(IPAddress.Any, 5129);
});

// Automatically open the browser to the application URL when it starts
//System.Diagnostics.Process.Start(new ProcessStartInfo
//{
//    FileName = $"http://{HomeController.GetLocalIpAddress()}:5129",
//    UseShellExecute = true
//});

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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
