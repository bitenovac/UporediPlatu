using Microsoft.EntityFrameworkCore;
using UporediPlate.Models;

namespace UporediPlate.Services.Izvestaji;

public class IzvestajNeslaganjeKoeficijenataServis
{
    private readonly AppDbContext _context;
    private readonly IzvestajPomocnik _pomocnik;

    public IzvestajNeslaganjeKoeficijenataServis(AppDbContext context, IzvestajPomocnik pomocnik)
    {
        _context = context;
        _pomocnik = pomocnik;
    }

    public async Task<List<IzvestajNeslaganjeKoeficijenataViewModel>> GenerisiAsync(List<string> izabraniKljucevi)
    {
        var rezultat = new List<IzvestajNeslaganjeKoeficijenataViewModel>();
        var zuZapisi = await _pomocnik.UcitajIzabraneZuZapiseAsync(izabraniKljucevi);

        if (zuZapisi.Count == 0)
        {
            return rezultat;
        }

        var periodi = zuZapisi.Select(x => x.PeriodUvoza.Date).Distinct().ToList();
        var normSifre = zuZapisi
            .Select(x => _pomocnik.NormalizujSifruZu(x.SifraZu))
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct()
            .ToList();

        var rfzoZapisi = await _context.RfzoUvozi
            .AsNoTracking()
            .Where(x => periodi.Contains(x.PeriodUvoza.Date))
            .ToListAsync();

        var rfzoPoKljucu = rfzoZapisi
            .Where(x => !string.IsNullOrWhiteSpace(x.Jmbg) &&
                        normSifre.Contains(_pomocnik.NormalizujSifruZu(x.SifraZu)))
            .GroupBy(x => (
                Sifra: _pomocnik.NormalizujSifruZu(x.SifraZu),
                Period: x.PeriodUvoza.Date,
                Jmbg: x.Jmbg.Trim()
            ))
            .ToDictionary(g => g.Key, g => g.ToList());

        const decimal tolerancija = 0.0001m;

        foreach (var zu in zuZapisi)
        {
            if (string.IsNullOrWhiteSpace(zu.Jmbg))
            {
                continue;
            }

            var normSifra = _pomocnik.NormalizujSifruZu(zu.SifraZu);
            var kljuc = (Sifra: normSifra, Period: zu.PeriodUvoza.Date, Jmbg: zu.Jmbg.Trim());

            if (!rfzoPoKljucu.TryGetValue(kljuc, out var poklapanjaRfzo))
            {
                continue;
            }

            var rfzo = poklapanjaRfzo
                .OrderByDescending(r => r.Koef1 ?? 0m)
                .First();

            var osnovniRfzo = Math.Round(rfzo.Koef1 ?? 0m, 4);
            var ukupniRfzo = Math.Round(
                (rfzo.Koef1 ?? 0m) +
                (rfzo.DodKzaRuk ?? 0m) +
                (rfzo.DodKzaSiNzv ?? 0m) +
                (rfzo.Dodatni036 ?? 0m), 4);

            var osnovniZu = Math.Round(zu.OsnovniKoef, 4);
            var ukupniZu = Math.Round(zu.UkupniKoef, 4);

            var razlikaOsnovni = Math.Abs(osnovniRfzo - osnovniZu) >= tolerancija;
            var razlikaUkupni = Math.Abs(ukupniRfzo - ukupniZu) >= tolerancija;

            if (razlikaOsnovni || razlikaUkupni)
            {
                rezultat.Add(new IzvestajNeslaganjeKoeficijenataViewModel
                {
                    SifraZuOriginal = zu.SifraZu,
                    SifraZuNormalizovana = normSifra,
                    NazivZu = string.IsNullOrWhiteSpace(zu.NazivZu) ? rfzo.NazivZU ?? string.Empty : zu.NazivZu,
                    PeriodUvoza = zu.PeriodUvoza,
                    Jmbg = zu.Jmbg.Trim(),
                    Ime = zu.Ime?.Trim() ?? string.Empty,
                    Prezime = zu.Prezime?.Trim() ?? string.Empty,
                    NazivRM =
                        string.IsNullOrWhiteSpace(rfzo.NazivRM) ? rfzo.NazivZanimanja ?? "-" : rfzo.NazivRM.Trim(),
                    OsnovniKoefRfzo = osnovniRfzo,
                    OsnovniKoefZu = osnovniZu,
                    UkupniKoefRfzo = ukupniRfzo,
                    UkupniKoefZu = ukupniZu
                });
            }
        }

        return rezultat
            .OrderBy(x => x.SifraZuNormalizovana)
            .ThenBy(x => x.Prezime)
            .ThenBy(x => x.Ime)
            .ToList();
    }
}