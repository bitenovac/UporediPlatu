using Microsoft.EntityFrameworkCore;
using UporediPlate.Models;

namespace UporediPlate.Services.Izvestaji;

public class IzvestajPomocnik
{
    private readonly AppDbContext _context;

    public IzvestajPomocnik(AppDbContext context) => _context = context;

    // Уклања водећу јединицу из ЗУ шифре установе (9 -> 8 цифара)
    public string NormalizujSifruZu(string? sifraZu)
    {
        if (string.IsNullOrWhiteSpace(sifraZu))
        {
            return string.Empty;
        }

        var ociscena = sifraZu.Trim();

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
        if (izabraniKljucevi == null || izabraniKljucevi.Count == 0)
        {
            return zuZapisi;
        }

        foreach (var kljuc in izabraniKljucevi)
        {
            if (kljuc.Length >= 14)
            {
                var godina = int.Parse(kljuc.Substring(0, 4));
                var mesec = int.Parse(kljuc.Substring(4, 2));
                var dan = int.Parse(kljuc.Substring(6, 2));
                var sat = int.Parse(kljuc.Substring(8, 2));
                var minut = int.Parse(kljuc.Substring(10, 2));
                var sekund = int.Parse(kljuc.Substring(12, 2));

                var pocetak = new DateTime(godina, mesec, dan, sat, minut, sekund);
                var kraj = pocetak.AddSeconds(1);

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