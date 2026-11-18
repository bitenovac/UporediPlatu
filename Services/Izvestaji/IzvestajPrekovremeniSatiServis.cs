using UporediPlate.Models;

namespace UporediPlate.Services.Izvestaji;

public class IzvestajPrekovremeniSatiServis
{
    private readonly IzvestajPomocnik _pomocnik;

    public IzvestajPrekovremeniSatiServis(IzvestajPomocnik pomocnik) => _pomocnik = pomocnik;

    public async Task<List<IzvestajPrekovremeniSatiViewModel>> GenerisiAsync(List<string> izabraniKljucevi)
    {
        var zuZapisi = await _pomocnik.UcitajIzabraneZuZapiseAsync(izabraniKljucevi);
        var rezultat = new List<IzvestajPrekovremeniSatiViewModel>();

        foreach (var zu in zuZapisi)
        {
            var ukupnoDodatnih = zu.PrekovremeniSati + zu.DezurstvoSati + zu.PoPozivuSati + zu.PripravnostSati;

            if (ukupnoDodatnih > 0)
            {
                rezultat.Add(new IzvestajPrekovremeniSatiViewModel
                {
                    SifraZuOriginal = zu.SifraZu ?? string.Empty,
                    SifraZuNormalizovana = _pomocnik.NormalizujSifruZu(zu.SifraZu),
                    NazivZu = zu.NazivZu ?? string.Empty,
                    PeriodUvoza = zu.PeriodUvoza,
                    Jmbg = zu.Jmbg ?? string.Empty,
                    Ime = zu.Ime ?? string.Empty,
                    Prezime = zu.Prezime ?? string.Empty,
                    RadnoMesto = zu.RadnoMesto ?? string.Empty,
                    PrekovremeniSati = zu.PrekovremeniSati,
                    DezurstvoSati = zu.DezurstvoSati,
                    PoPozivuSati = zu.PoPozivuSati,
                    PripravnostSati = zu.PripravnostSati,
                    UkupnoDodatnihSati = ukupnoDodatnih,
                    RadnoVreme = zu.SatiUkupno
                });
            }
        }

        return rezultat
            .OrderBy(x => x.SifraZuNormalizovana)
            .ThenByDescending(x => x.UkupnoDodatnihSati)
            .ThenBy(x => x.Prezime)
            .ThenBy(x => x.Ime)
            .ToList();
    }
}