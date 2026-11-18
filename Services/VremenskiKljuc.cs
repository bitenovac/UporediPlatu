using System.Globalization;
using UporediPlate.Models;

namespace UporediPlate.Services;

// Временски кључ увоза (yyyyMMddHHmmss) - јединствено идентификује један учитан фајл
public static class VremenskiKljuc
{
    private const string Format = "yyyyMMddHHmmss";

    public static string Formiraj(DateTime datumUvoza) => datumUvoza.ToString(Format, CultureInfo.InvariantCulture);

    public static bool PokusajParsiranje(string? kljuc, out DateTime pocetakSekunde)
    {
        pocetakSekunde = default;
        return kljuc is { Length: >= 14 }
               && DateTime.TryParseExact(kljuc[..14], Format, CultureInfo.InvariantCulture, DateTimeStyles.None,
                   out pocetakSekunde);
    }

    // Сви записи учитани у истој секунди припадају истом увозу
    public static IQueryable<T> IzUvoza<T>(this IQueryable<T> upit, DateTime pocetakSekunde) where T : IUvozniZapis
    {
        var krajSekunde = pocetakSekunde.AddSeconds(1);
        return upit.Where(x => x.DatumUvozaPodataka >= pocetakSekunde && x.DatumUvozaPodataka < krajSekunde);
    }
}
