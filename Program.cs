using Microsoft.EntityFrameworkCore;
using PdfSharp.Fonts;
using UporediPlate;
using UporediPlate.Services;

// Апликација ради искључиво на Windows-у
if (!OperatingSystem.IsWindows())
{
    Console.Error.WriteLine("Апликација UporediPlate може да се покрене само на Windows оперативном систему.");
    return 1;
}

// PDFsharp користи системске фонтове Windows-а (Arial са ћирилицом)
GlobalFontSettings.UseWindowsFontsUnderWindows = true;

var builder = WebApplication.CreateBuilder(args);

// 1. Повлачење сигурног конекционог низа из User Secrets или appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Регистрација контекста базе података у контејнер сервиса
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Регистрација свих сервиса апликације (увоз, извештаји, Excel, PDF)
builder.Services.DodajAplikativneServise();

// Додавање подршке за MVC контролере и погледе
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Конфигурација протока извршавања HTTP захтева (Pipeline)
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