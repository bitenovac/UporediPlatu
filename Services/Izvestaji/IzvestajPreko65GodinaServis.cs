using UporediPlate.Models;

namespace UporediPlate.Services.Izvestaji;

public class IzvestajPreko65GodinaServis
{
    private readonly IzvestajPomocnik _pomocnik;

    public IzvestajPreko65GodinaServis(IzvestajPomocnik pomocnik) => _pomocnik = pomocnik;

    public async Task<List<IzvestajPreko65GodinaViewModel>> GenerisiAsync(List<string> izabraniKljucevi)
    {
        var zuZapisi = await _pomocnik.UcitajIzabraneZuZapiseAsync(izabraniKljucevi);
        var rezultat = new List<IzvestajPreko65GodinaViewModel>();

        foreach (var zu in zuZapisi)
        {
            if (!PokusajParsiranjeDatumaIzJmbg(zu.Jmbg, out var datumRodjenja))
            {
                continue;
            }

            var datum65 = datumRodjenja.AddYears(65);

            // Poslednji dan obračunskog meseca (npr. za 11.2026 to je 30.11.2026)
            var poslednjiDanMeseca = DateTime.DaysInMonth(zu.PeriodUvoza.Year, zu.PeriodUvoza.Month);
            var krajObracunskogMeseca = new DateTime(zu.PeriodUvoza.Year, zu.PeriodUvoza.Month, poslednjiDanMeseca);

            // Prikazujemo sve koji imaju >= 65 godina ili u tom mesecu pune 65 godina
            if (datum65 <= krajObracunskogMeseca)
            {
                var puni65UTomMesecu = datum65.Year == zu.PeriodUvoza.Year &&
                                       datum65.Month == zu.PeriodUvoza.Month;

                rezultat.Add(new IzvestajPreko65GodinaViewModel
                {
                    SifraZuOriginal = zu.SifraZu ?? string.Empty,
                    SifraZuNormalizovana = _pomocnik.NormalizujSifruZu(zu.SifraZu),
                    NazivZu = zu.NazivZu ?? string.Empty,
                    PeriodUvoza = zu.PeriodUvoza,
                    Jmbg = zu.Jmbg?.Trim() ?? string.Empty,
                    Ime = zu.Ime ?? string.Empty,
                    Prezime = zu.Prezime ?? string.Empty,
                    DatumRodjenja = datumRodjenja,
                    Datum65Rodjendana = datum65,
                    Puni65UTomMesecu = puni65UTomMesecu,
                    RedovanRad = zu.SatiUkupno,
                    PrekovremeniSati = zu.PrekovremeniSati,
                    DezurstvoSati = zu.DezurstvoSati,
                    PoPozivuSati = zu.PoPozivuSati,
                    PripravnostSati = zu.PripravnostSati,
                    UkupanNeto = zu.UkupanNeto
                });
            }
        }

        return rezultat
            .OrderBy(x => x.SifraZuNormalizovana)
            .ThenByDescending(x => x.DatumRodjenja)
            .ThenBy(x => x.Prezime)
            .ThenBy(x => x.Ime)
            .ToList();
    }

    private bool PokusajParsiranjeDatumaIzJmbg(string? jmbg, out DateTime datumRodjenja)
    {
        datumRodjenja = default;
        if (string.IsNullOrWhiteSpace(jmbg))
        {
            return false;
        }

        var cist = jmbg.Trim();
        if (cist.Length < 7)
        {
            return false;
        }

        if (!int.TryParse(cist.Substring(0, 2), out var dan) ||
            !int.TryParse(cist.Substring(2, 2), out var mesec) ||
            !int.TryParse(cist.Substring(4, 3), out var trocifrenaGodina))
        {
            return false;
        }

        // Peta cifra u JMBG-u (indeks 4): ako je '0' -> 20xx, inače (npr. '9') -> 19xx
        var punaGodina = cist[4] == '0'
            ? 2000 + trocifrenaGodina
            : 1000 + trocifrenaGodina;

        try
        {
            datumRodjenja = new DateTime(punaGodina, mesec, dan);
            return true;
        }
        catch
        {
            return false;
        }
    }
}