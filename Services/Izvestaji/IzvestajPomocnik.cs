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

    // Шифра ЗУ за назив листа/фајла: оригинална ако постоји, иначе нормализована
    public static string SifraZaPrikaz(string? sifraOriginal, string sifraNormalizovana) =>
        string.IsNullOrWhiteSpace(sifraOriginal) ? sifraNormalizovana : sifraOriginal;

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
            if (VremenskiKljuc.PokusajParsiranje(kljuc, out var pocetak))
            {
                var zapisiFajla = await _context.ZuUvozi
                    .AsNoTracking()
                    .IzUvoza(pocetak)
                    .ToListAsync();

                zuZapisi.AddRange(zapisiFajla);
            }
        }

        return zuZapisi;
    }
}