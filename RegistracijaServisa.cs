using UporediPlate.Izvestaji;
using UporediPlate.Izvestaji.Okvir;
using UporediPlate.Uvoz;

namespace UporediPlate;

public static class RegistracijaServisa
{
    public static IServiceCollection DodajAplikativneServise(this IServiceCollection services)
    {
        // Увоз РФЗО и ЗУ Excel фајлова
        services.AddScoped<UvozRfzoServis>();
        services.AddScoped<UvozZuServis>();

        // Извештаји - редослед регистрације је редослед картица у обједињеном приказу
        services.AddScoped<IzvestajPomocnik>();
        services.AddScoped<RegistarIzvestaja>();
        services.AddScoped<IIzvestaj, NepronadjeniURfzoIzvestaj>();
        services.AddScoped<IIzvestaj, NepronadjeniUZuIzvestaj>();
        services.AddScoped<IIzvestaj, NeslaganjeKoeficijenataIzvestaj>();
        services.AddScoped<IIzvestaj, PrekovremeniSatiIzvestaj>();
        services.AddScoped<IIzvestaj, Preko65GodinaIzvestaj>();
        services.AddScoped<IIzvestaj, NepunoRadnoVremeIzvestaj>();

        return services;
    }
}
