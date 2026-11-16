using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UporediPlate.Models;

namespace UporediPlate.Services.Izvestaji
{
    public class IzvestajPomocnik
    {
        private readonly AppDbContext _context;

        public IzvestajPomocnik(AppDbContext context)
        {
            _context = context;
        }

        // Уклања водећу јединицу из ЗУ шифре установе (9 -> 8 цифара)
        public string NormalizujSifruZu(string? sifraZu)
        {
            if (string.IsNullOrWhiteSpace(sifraZu)) return string.Empty;
            string ociscena = sifraZu.Trim();

            if (ociscena.Length == 9 && ociscena.StartsWith("1"))
            {
                return ociscena.Substring(1);
            }
            return ociscena;
        }

        // Учитава све ЗУ записе на основу изабраних кључева (yyyyMMddHHmmss)
        public async Task<List<ZuUvoz>> UcitajIzabraneZuZapiseAsync(List<string> izabraniKljucevi)
        {
            var zuZapisi = new List<ZuUvoz>();
            if (izabraniKljucevi == null || izabraniKljucevi.Count == 0) return zuZapisi;

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

            return zuZapisi;
        }
    }
}
