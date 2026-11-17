using Microsoft.Extensions.DependencyInjection;
using UporediPlate.Services.Excel;
using UporediPlate.Services.Izvestaji;
using UporediPlate.Services.Pdf;

namespace UporediPlate.Services
{
    public static class RegistracijaServisa
    {
        public static IServiceCollection DodajAplikativneServise(this IServiceCollection services)
        {
            // Сервиси за брзи увоз података
            services.AddScoped<UvozRfzoServis>();
            services.AddScoped<UvozZuServis>();

            // Сервиси за генерисање извештаја и координацију
            services.AddScoped<IzvestajPomocnik>();
            services.AddScoped<KoordinatorIzvestajaServis>();
            services.AddScoped<IzvestajNepronadjeniURfzoServis>();
            services.AddScoped<IzvestajNepronadjeniUZuServis>();
            services.AddScoped<IzvestajNeslaganjeKoeficijenataServis>();
            services.AddScoped<IzvestajPrekovremeniSatiServis>();

            // Сервиси за извоз извештаја у Excel
            services.AddScoped<ExcelPomocnik>();
            services.AddScoped<ExcelNepronadjeniURfzoServis>();
            services.AddScoped<ExcelNepronadjeniUZuServis>();
            services.AddScoped<ExcelNeslaganjeKoeficijenataServis>();
            services.AddScoped<ExcelPrekovremeniSatiServis>();

            // Сервиси за извоз извештаја у PDF / ZIP
            services.AddScoped<PdfPomocnik>();
            services.AddScoped<PdfNepronadjeniURfzoServis>();
            services.AddScoped<PdfNepronadjeniUZuServis>();
            services.AddScoped<PdfNeslaganjeKoeficijenataServis>();
            services.AddScoped<PdfPrekovremeniSatiServis>();

            return services;
        }
    }
}
