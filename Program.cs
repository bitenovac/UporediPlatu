using Microsoft.EntityFrameworkCore;
using PdfSharp.Fonts;
using UporediPlate;
using UporediPlate.Data;

if (!OperatingSystem.IsWindows())
{
    Console.Error.WriteLine("Апликација UporediPlate може да се покрене само на Windows оперативном систему.");
    return 1;
}

GlobalFontSettings.UseWindowsFontsUnderWindows = true;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.DodajAplikativneServise();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        "default",
        "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
return 0;