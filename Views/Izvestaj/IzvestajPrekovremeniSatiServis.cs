using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UporediPlate.Models;

namespace UporediPlate.Services.Izvestaji
{
    public class IzvestajPrekovremeniSatiServis
    {
        private readonly IzvestajPomocnik _pomocnik;

        public IzvestajPrekovremeniSatiServis(IzvestajPomocnik pomocnik)
        {
            _pomocnik = pomocnik;
        }

        public async Task<List<IzvestajPrekovremeniSatiViewModel>> GenerisiAsync(List<string> izabraniKljucevi)
        {
            var zuZapisi = await _pomocnik.UcitajIzabraneZuZapiseAsync(izabraniKljucevi);
            var rezultat = new List<IzvestajPrekovremeniSatiViewModel>();

            foreach (var zu in zuZapisi)
            {
                decimal ukupnoDodatnihSati =
                    zu.PrekovremeniSati +
                    zu.DezurstvoSati +
                    zu.PoPozivuSati +
                    zu.PripravnostSati;

                if (ukupnoDodatnihSati > 0)
                {
                    rezultat.Add(new IzvestajPrekovremeniSatiViewModel
                    {
                        SifraZuOriginal = zu.SifraZu,
                        SifraZuNormalizovana = _pomocnik.NormalizujSifruZu(zu.SifraZu),
                        NazivZu = zu.NazivZu,
                        PeriodUvoza = zu.PeriodUvoza,
                        Jmbg = zu.Jmbg,
                        Ime = zu.Ime,
                        Prezime = zu.Prezime,
                        RadnoMesto = zu.RadnoMesto,
                        PrekovremeniSati = zu.PrekovremeniSati,
                        DezurstvoSati = zu.DezurstvoSati,
                        PoPozivuSati = zu.PoPozivuSati,
                        PripravnostSati = zu.PripravnostSati,
                        UkupnoDodatnihSati = ukupnoDodatnihSati,
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
}
