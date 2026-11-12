using Microsoft.EntityFrameworkCore;
using UporediPlate;
using UporediPlate.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Повлачење сигурног конекционог низа из User Secrets или appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Регистрација контекста базе података у контејнер сервиса
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Регистрација сервиса за брзи увоз РФЗО података
builder.Services.AddScoped<UvozRfzoServis>();

// 4. Регистрација сервиса за брзи увоз података из Здравствених Установа
builder.Services.AddScoped<UvozZuServis>();

// 5. Регистрација сервиса за генерисање извештаја и поређење података
builder.Services.AddScoped<IzvestajServis>();

// 6. Регистрација сервиса за извоз извештаја у Excel
builder.Services.AddScoped<ExcelIzvozServis>();

// 7. Регистрација сервиса за извоз извештаја у PDF / ZIP
builder.Services.AddScoped<PdfIzvozServis>();

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
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
