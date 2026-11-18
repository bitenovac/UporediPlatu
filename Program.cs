using Microsoft.EntityFrameworkCore;
using UporediPlate;
using UporediPlate.Services;

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