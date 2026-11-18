using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UporediPlate.Models;

namespace UporediPlate.Services.Izvestaji
{
    public class IzvestajNepunoRadnoVremeServis
    {
        private readonly AppDbContext _context;
        private readonly IzvestajPomocnik _pomocnik;

        public IzvestajNepunoRadnoVremeServis(AppDbContext context, IzvestajPomocnik pomocnik)
        {
            _context = context;
            _pomocnik = pomocnik;
        }

        public async Task<List<IzvestajNepunoRadnoVremeViewModel>> GenerisiAsync(List<string> izabraniKljucevi)
        {
            var rezultat = new List<IzvestajNepunoRadnoVremeViewModel>();
            var zuZapisi = await _pomocnik.UcitajIzabraneZuZapiseAsync(izabraniKljucevi);

            if (zuZapisi.Count == 0)
                return rezultat;

            // Za svaku ustanovu i period nalazimo maksimalni fond sati (Max SatiUkupno iz ZU tabele)
            var maxSatiPoZuIPeriodu = zuZapisi
                .GroupBy(x => (Sifra: _pomocnik.NormalizujSifruZu(x.SifraZu), Period: x.PeriodUvoza.Date))
                .ToDictionary(g => g.Key, g => g.Max(z => z.SatiUkupno));

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
                .Where(x => !string.IsNullOrWhiteSpace(x.Jmbg) && normSifre.Contains(_pomocnik.NormalizujSifruZu(x.SifraZu)))
                .GroupBy(x => (
                    Sifra: _pomocnik.NormalizujSifruZu(x.SifraZu),
                    Period: x.PeriodUvoza.Date,
                    Jmbg: x.Jmbg.Trim()
                ))
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var zu in zuZapisi)
            {
                if (string.IsNullOrWhiteSpace(zu.Jmbg))
                    continue;

                string normSifra = _pomocnik.NormalizujSifruZu(zu.SifraZu);
                var kljuc = (Sifra: normSifra, Period: zu.PeriodUvoza.Date, Jmbg: zu.Jmbg.Trim());

                if (!rfzoPoKljucu.TryGetValue(kljuc, out var poklapanjaRfzo))
                    continue;

                var rfzo = poklapanjaRfzo
                    .OrderBy(r => r.ProcRVr ?? 100m)
                    .First();

                // Svi zaposleni kod kojih je u RFZO evidenciji procenat radnog vremena manji od 100%
                if (rfzo.ProcRVr.HasValue && rfzo.ProcRVr.Value < 100m)
                {
                    maxSatiPoZuIPeriodu.TryGetValue((Sifra: normSifra, Period: zu.PeriodUvoza.Date), out decimal maxFondUstanove);
                    decimal dozvoljeniSati = Math.Round(maxFondUstanove * (rfzo.ProcRVr.Value / 100m), 2);

                    string radnoMesto = !string.IsNullOrWhiteSpace(rfzo.NazivRM)
                        ? rfzo.NazivRM.Trim()
                        : (!string.IsNullOrWhiteSpace(zu.RadnoMesto) ? zu.RadnoMesto.Trim() : "-");

                    rezultat.Add(new IzvestajNepunoRadnoVremeViewModel
                    {
                        SifraZuOriginal = zu.SifraZu,
                        SifraZuNormalizovana = normSifra,
                        NazivZu = string.IsNullOrWhiteSpace(zu.NazivZu) ? (rfzo.NazivZU ?? string.Empty) : zu.NazivZu,
                        PeriodUvoza = zu.PeriodUvoza,
                        Jmbg = zu.Jmbg.Trim(),
                        Ime = zu.Ime?.Trim() ?? string.Empty,
                        Prezime = zu.Prezime?.Trim() ?? string.Empty,
                        RadnoMesto = radnoMesto,
                        ProcenatRadnogVremenaRfzo = rfzo.ProcRVr.Value,
                        MaksimalnoDozvoljeniSati = dozvoljeniSati,
                        RedovanRadZu = zu.SatiUkupno,
                        PrekovremeniSatiZu = zu.PrekovremeniSati,
                        DezurstvoSatiZu = zu.DezurstvoSati,
                        UkupanNetoZu = zu.UkupanNeto
                    });
                }
            }

            return rezultat
                .OrderBy(x => x.SifraZuNormalizovana)
                .ThenBy(x => x.ProcenatRadnogVremenaRfzo)
                .ThenBy(x => x.Prezime)
                .ThenBy(x => x.Ime)
                .ToList();
        }
    }
}
