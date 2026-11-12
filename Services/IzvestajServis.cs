using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UporediPlate.Models;

namespace UporediPlate.Services
{
    public class IzvestajServis
    {
        private readonly AppDbContext _context;

        public IzvestajServis(AppDbContext context)
        {
            _context = context;
        }

        // Помоћна метода за уклањање водеће јединице из ЗУ шифре установе (9 -> 8 цифара)
        private string NormalizujSifruZu(string? sifraZu)
        {
            if (string.IsNullOrWhiteSpace(sifraZu)) return string.Empty;
            string ociscena = sifraZu.Trim();

            if (ociscena.Length == 9 && ociscena.StartsWith("1"))
            {
                return ociscena.Substring(1);
            }
            return ociscena;
        }

        // ИЗВЕШТАЈ 1: Радници из ЗУ фајлова који нису пронађени у РФЗО евиденцији (по ЈМБГ-у и Шифри ЗУ)
        public async Task<List<IzvestajNepronadjeniViewModel>> GenerisiIzvestajNepronadjeniURfzoAsync(List<string> izabraniKljucevi)
        {
            var rezultat = new List<IzvestajNepronadjeniViewModel>();
            if (izabraniKljucevi == null || izabraniKljucevi.Count == 0) return rezultat;

            // 1. Прикупљамо све ЗУ записе за изабране фајлове
            var zuZapisi = new List<ZuUvoz>();
            foreach (var kljuc in izabraniKljucevi)
            {
                if (kljuc.Length >= 14)
                {
                    int godina = int.Parse(kljuc.Substring(0, 4));
                    int mesec = int.Parse(kljuc.Substring(4, 2));
                    int dan = int.Parse(kljuc.Substring(6, 2));
                    int sat = int.Parse(kljuc.Substring(8, 2));
                    int minut = int.Parse(kljuc.Substring(10, 2));
                    int sekund = int.Parse(kljuc.Substring(12, 2));

                    DateTime pocetak = new DateTime(godina, mesec, dan, sat, minut, sekund);
                    DateTime kraj = pocetak.AddSeconds(1);

                    var zapisiFajla = await _context.ZuUvozi
                        .AsNoTracking()
                        .Where(x => x.DatumUvozaPodataka >= pocetak && x.DatumUvozaPodataka < kraj)
                        .ToListAsync();

                    zuZapisi.AddRange(zapisiFajla);
                }
            }

            if (zuZapisi.Count == 0) return rezultat;

            // 2. Извлачимо све јединствене периоде (Година/Месец) из изабраних ЗУ фајлова
            var periodi = zuZapisi
                .Select(x => new { x.PeriodUvoza.Year, x.PeriodUvoza.Month })
                .Distinct()
                .ToList();

            // 3. Учитавамо све РФЗО записе за те периоде ради брзог поређења у меморији
            var rfzoZapisi = new List<RfzoUvoz>();
            foreach (var p in periodi)
            {
                var rfzoZaPeriod = await _context.RfzoUvozi
                    .AsNoTracking()
                    .Where(r => r.PeriodUvoza.Year == p.Year && r.PeriodUvoza.Month == p.Month)
                    .ToListAsync();

                rfzoZapisi.AddRange(rfzoZaPeriod);
            }

            // Правимо брзе скупове за претрагу (HashSet) по периоду, ЈМБГ-у и Шифри ЗУ
            var rfzoParovi = new HashSet<string>(
                rfzoZapisi.Select(r => $"{r.PeriodUvoza:yyyyMM}|{(r.Jmbg ?? "").Trim()}|{(r.SifraZu ?? "").Trim()}")
            );

            var rfzoSamoJmbg = new HashSet<string>(
                rfzoZapisi.Select(r => $"{r.PeriodUvoza:yyyyMM}|{(r.Jmbg ?? "").Trim()}")
            );

            // 4. Проверавамо сваки ЗУ запис
            foreach (var zu in zuZapisi)
            {
                string jmbgClean = (zu.Jmbg ?? string.Empty).Trim();
                string sifraNorm = NormalizujSifruZu(zu.SifraZu);
                string periodKljuc = zu.PeriodUvoza.ToString("yyyyMM");

                string punKljuc = $"{periodKljuc}|{jmbgClean}|{sifraNorm}";

                if (!rfzoParovi.Contains(punKljuc))
                {
                    // Утврђујемо тачан разлог да би извештај био информативнији
                    string razlog = rfzoSamoJmbg.Contains($"{periodKljuc}|{jmbgClean}")
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
}
