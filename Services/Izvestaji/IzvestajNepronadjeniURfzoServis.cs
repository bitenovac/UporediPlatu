using Microsoft.EntityFrameworkCore;
using UporediPlate.Models;

namespace UporediPlate.Services.Izvestaji;

public class IzvestajNepronadjeniURfzoServis
{
    private readonly AppDbContext _context;
    private readonly IzvestajPomocnik _pomocnik;

    public IzvestajNepronadjeniURfzoServis(AppDbContext context, IzvestajPomocnik pomocnik)
    {
        _context = context;
        _pomocnik = pomocnik;
    }

    public async Task<List<IzvestajNepronadjeniViewModel>> GenerisiAsync(List<string> izabraniKljucevi)
    {
        var rezultat = new List<IzvestajNepronadjeniViewModel>();
        var zuZapisi = await _pomocnik.UcitajIzabraneZuZapiseAsync(izabraniKljucevi);

        if (zuZapisi.Count == 0)
        {
            return rezultat;
        }

        var periodi = zuZapisi
            .Select(x => new { x.PeriodUvoza.Year, x.PeriodUvoza.Month })
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

        var rfzoParovi = new HashSet<string>(
            rfzoZapisi.Select(r => $"{r.PeriodUvoza:yyyyMM}|{(r.Jmbg ?? "").Trim()}|{(r.SifraZu ?? "").Trim()}")
        );

        var rfzoSamoJmbg = new HashSet<string>(
            rfzoZapisi.Select(r => $"{r.PeriodUvoza:yyyyMM}|{(r.Jmbg ?? "").Trim()}")
        );

        foreach (var zu in zuZapisi)
        {
            var jmbgClean = (zu.Jmbg ?? string.Empty).Trim();
            var sifraNorm = _pomocnik.NormalizujSifruZu(zu.SifraZu);
            var periodKljuc = zu.PeriodUvoza.ToString("yyyyMM");

            var punKljuc = $"{periodKljuc}|{jmbgClean}|{sifraNorm}";

            if (!rfzoParovi.Contains(punKljuc))
            {
                var razlog = rfzoSamoJmbg.Contains($"{periodKljuc}|{jmbgClean}")
                    ? "ЈМБГ постоји у РФЗО, али под другом шифром ЗУ"
                    : "ЈМБГ није пронађен у РФЗО евиденцији за овај период";

                rezultat.Add(new IzvestajNepronadjeniViewModel
                {
                    SifraZuOriginal = zu.SifraZu,
                    SifraZuNormalizovana = sifraNorm,
                    NazivZu = zu.NazivZu,
                    Nivo = zu.Nivo,
                    Jmbg = jmbgClean,
                    Prezime = zu.Prezime,
                    Ime = zu.Ime,
                    RadnoMesto = zu.RadnoMesto,
                    UkupniKoef = zu.UkupniKoef,
                    UkupanNeto = zu.UkupanNeto,
                    Bruto1 = zu.Bruto1,
                    PeriodUvoza = zu.PeriodUvoza,
                    NazivFajla = zu.NazivFajla,
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