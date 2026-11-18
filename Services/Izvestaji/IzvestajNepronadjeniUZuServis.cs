using Microsoft.EntityFrameworkCore;
using UporediPlate.Models;

namespace UporediPlate.Services.Izvestaji;

public class IzvestajNepronadjeniUZuServis
{
    private readonly AppDbContext _context;
    private readonly IzvestajPomocnik _pomocnik;

    public IzvestajNepronadjeniUZuServis(AppDbContext context, IzvestajPomocnik pomocnik)
    {
        _context = context;
        _pomocnik = pomocnik;
    }

    public async Task<List<IzvestajNepronadjeniUZuViewModel>> GenerisiAsync(List<string> izabraniKljucevi)
    {
        var rezultat = new List<IzvestajNepronadjeniUZuViewModel>();
        var zuZapisi = await _pomocnik.UcitajIzabraneZuZapiseAsync(izabraniKljucevi);

        if (zuZapisi.Count == 0)
        {
            return rezultat;
        }

        var izabraneUstanoveIPeriodi = zuZapisi
            .Select(z => new
            {
                z.PeriodUvoza.Year,
                z.PeriodUvoza.Month,
                SifraNorm = _pomocnik.NormalizujSifruZu(z.SifraZu),
                z.NazivZu
            })
            .Distinct()
            .ToList();

        var dozvoljeniParoviUstanova = new HashSet<string>(
            izabraneUstanoveIPeriodi.Select(u => $"{u.Year:D4}{u.Month:D2}|{u.SifraNorm}")
        );

        var naziviZuMapa = izabraneUstanoveIPeriodi
            .GroupBy(u => u.SifraNorm)
            .ToDictionary(g => g.Key, g => g.First().NazivZu);

        var zuParoviRadnika = new HashSet<string>(
            zuZapisi.Select(z =>
                $"{z.PeriodUvoza:yyyyMM}|{(z.Jmbg ?? "").Trim()}|{_pomocnik.NormalizujSifruZu(z.SifraZu)}")
        );

        var zuSamoJmbg = new HashSet<string>(
            zuZapisi.Select(z => $"{z.PeriodUvoza:yyyyMM}|{(z.Jmbg ?? "").Trim()}")
        );

        var periodi = izabraneUstanoveIPeriodi
            .Select(x => new { x.Year, x.Month })
            .Distinct()
            .ToList();

        var rfzoZapisi = new List<RfzoUvoz>();
        foreach (var p in periodi)
        {
            var rfzoZaPeriod = await _context.RfzoUvozi
                .AsNoTracking()
                .Where(r => r.PeriodUvoza.Year == p.Year && r.PeriodUvoza.Month == p.Month)
                .ToListAsync();

            rfzoZapisi.AddRange(rfzoZaPeriod);
        }

        foreach (var rfzo in rfzoZapisi)
        {
            var sifraRfzo = (rfzo.SifraZu ?? string.Empty).Trim();
            var periodKljuc = rfzo.PeriodUvoza.ToString("yyyyMM");
            var kljucUstanove = $"{periodKljuc}|{sifraRfzo}";

            if (!dozvoljeniParoviUstanova.Contains(kljucUstanove))
            {
                continue;
            }

            var jmbgClean = (rfzo.Jmbg ?? string.Empty).Trim();
            var punKljucRadnika = $"{periodKljuc}|{jmbgClean}|{sifraRfzo}";

            if (!zuParoviRadnika.Contains(punKljucRadnika))
            {
                var razlog = zuSamoJmbg.Contains($"{periodKljuc}|{jmbgClean}")
                    ? "ЈМБГ постоји у другом изабраном ЗУ фајлу, али не у овој установи"
                    : "Радник је на списку РФЗО-а, али није достављен у ЗУ фајлу";

                var nazivUstanove = !string.IsNullOrWhiteSpace(rfzo.NazivZU)
                    ? rfzo.NazivZU
                    : naziviZuMapa.ContainsKey(sifraRfzo)
                        ? naziviZuMapa[sifraRfzo]
                        : string.Empty;

                rezultat.Add(new IzvestajNepronadjeniUZuViewModel
                {
                    SifraZuNormalizovana = sifraRfzo,
                    NazivZu = nazivUstanove,
                    NazivOJ = rfzo.NazivOJ ?? string.Empty,
                    Jmbg = jmbgClean,
                    Prezime = rfzo.Prezime ?? string.Empty,
                    Ime = rfzo.Ime ?? string.Empty,
                    NazivZanimanja = rfzo.NazivZanimanja ?? string.Empty,
                    NazivRM = rfzo.NazivRM ?? string.Empty,
                    Koef1 = rfzo.Koef1,
                    ProcRVr = rfzo.ProcRVr,
                    DatumPrestankaRO = rfzo.DatumPrestankaRO,
                    PeriodUvoza = rfzo.PeriodUvoza,
                    NazivFajlaRfzo = rfzo.NazivFajla ?? string.Empty,
                    RazlogNeslaganja = razlog
                });
            }
        }

        return rezultat
            .OrderBy(x => x.PeriodUvoza)
            .ThenBy(x => x.SifraZuNormalizovana)
            .ThenBy(x => x.Prezime)
            .ThenBy(x => x.Ime)
            .ToList();
    }
}