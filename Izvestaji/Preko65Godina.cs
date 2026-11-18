using UporediPlate.Izvestaji.Okvir;
using UporediPlate.Models;

namespace UporediPlate.Izvestaji;

// Запослени који имају или у обрачунском месецу пуне 65 година
public class Preko65GodinaStavka : IStavkaIzvestaja
{
    public string SifraZuOriginal { get; set; } = string.Empty;
    public string SifraZuNormalizovana { get; set; } = string.Empty;
    public string NazivZu { get; set; } = string.Empty;
    public DateTime PeriodUvoza { get; set; }

    public string Jmbg { get; set; } = string.Empty;
    public string Ime { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;

    public DateTime DatumRodjenja { get; set; }
    public DateTime Datum65Rodjendana { get; set; }

    // True ako radnik tačno u obračunskom mesecu i godini puni 65 godina (crveni font)
    public bool Puni65UTomMesecu { get; set; }

    // SatiUkupno iz ZU tabele pod nazivom "Редован рад"
    public decimal RedovanRad { get; set; }
    public decimal PrekovremeniSati { get; set; }
    public decimal DezurstvoSati { get; set; }
    public decimal PoPozivuSati { get; set; }
    public decimal PripravnostSati { get; set; }
    public decimal UkupanNeto { get; set; }
}

public class Preko65GodinaIzvestaj : Izvestaj<Preko65GodinaStavka>
{
    private readonly IzvestajPomocnik _pomocnik;

    public Preko65GodinaIzvestaj(IzvestajPomocnik pomocnik) => _pomocnik = pomocnik;

    public override string Kljuc => "Preko65Godina";
    public override string Naslov => "Запослени старији од 65 година живота (услов за старосну пензију)";
    public override string NaslovStranice => "Запослени старији од 65 година живота";
    public override string Opis =>
        "Приказ запослених из ЗУ евиденције који имају преко 65 година живота. Црвеним словима су означени запослени који у обрачунском месецу пуне 65 година.";
    public override string Napomena =>
        "Напомена: Црвеним словима су означени запослени који у обрачунском месецу пуне 65 година живота.";
    public override string PorukaBezRezultata => "✅ У изабраним ЗУ фајловима нема запослених који су старији од 65 година!";
    public override string NazivKartice => "5. Запослени преко<br/>65 година живота";
    public override string PrefiksFajla => "Izvestaj_Preko_65_Godina";
    public override Func<Preko65GodinaStavka, bool> IstaknutRed => x => x.Puni65UTomMesecu;

    public override IOrderedEnumerable<Preko65GodinaStavka> Sortiraj(IEnumerable<Preko65GodinaStavka> stavke) =>
        stavke.OrderBy(x => x.SifraZuNormalizovana).ThenByDescending(x => x.DatumRodjenja)
            .ThenBy(x => x.Prezime).ThenBy(x => x.Ime);

    public override IReadOnlyList<KolonaEkrana<Preko65GodinaStavka>> KoloneEkrana =>
    [
        Ekran<Preko65GodinaStavka>.Centrirano("Период", x => x.PeriodUvoza.ToString("MM.yyyy")),
        Ekran<Preko65GodinaStavka>.Jmbg(x => x.Jmbg),
        Ekran<Preko65GodinaStavka>.Tekst("Име", x => x.Ime),
        Ekran<Preko65GodinaStavka>.Tekst("Презиме", x => x.Prezime, klasaCelije: "fw-semibold"),
        Ekran<Preko65GodinaStavka>.Centrirano("Датум<br/>рођења", x => x.DatumRodjenja.ToString("dd.MM.yyyy")),
        Ekran<Preko65GodinaStavka>.Broj("Редован<br/>рад", x => x.RedovanRad),
        Ekran<Preko65GodinaStavka>.Broj("Прековремени<br/>сати", x => x.PrekovremeniSati),
        Ekran<Preko65GodinaStavka>.Broj("Дежурство<br/>сати", x => x.DezurstvoSati),
        Ekran<Preko65GodinaStavka>.Broj("По позиву<br/>сати", x => x.PoPozivuSati),
        Ekran<Preko65GodinaStavka>.Broj("Приправност<br/>сати", x => x.PripravnostSati),
        Ekran<Preko65GodinaStavka>.Broj("Укупно<br/>нето", x => x.UkupanNeto, "fw-bold")
    ];

    public override IReadOnlyList<KolonaExcela<Preko65GodinaStavka>> KoloneExcela =>
    [
        Excel<Preko65GodinaStavka>.Tekst("Период", x => x.PeriodUvoza.ToString("MM.yyyy"), centrirano: true),
        Excel<Preko65GodinaStavka>.Jmbg(x => x.Jmbg),
        Excel<Preko65GodinaStavka>.Tekst("Име", x => x.Ime),
        Excel<Preko65GodinaStavka>.Tekst("Презиме", x => x.Prezime),
        Excel<Preko65GodinaStavka>.Tekst("Датум рођења", x => x.DatumRodjenja.ToString("dd.MM.yyyy"), centrirano: true),
        Excel<Preko65GodinaStavka>.Broj("Редован рад", x => x.RedovanRad),
        Excel<Preko65GodinaStavka>.Broj("Прековремени сати", x => x.PrekovremeniSati),
        Excel<Preko65GodinaStavka>.Broj("Дежурство сати", x => x.DezurstvoSati),
        Excel<Preko65GodinaStavka>.Broj("По позиву сати", x => x.PoPozivuSati),
        Excel<Preko65GodinaStavka>.Broj("Приправност сати", x => x.PripravnostSati),
        Excel<Preko65GodinaStavka>.Broj("Укупно нето", x => x.UkupanNeto, podebljano: true)
    ];

    public override IReadOnlyList<KolonaPdfa<Preko65GodinaStavka>> KolonePdfa =>
    [
        Pdf<Preko65GodinaStavka>.Centrirano("Период", 1.7, x => x.PeriodUvoza.ToString("MM.yyyy")),
        Pdf<Preko65GodinaStavka>.Jmbg(2.6, x => x.Jmbg),
        Pdf<Preko65GodinaStavka>.Tekst("Име", 2.6, x => x.Ime),
        Pdf<Preko65GodinaStavka>.Podebljano("Презиме", 3.0, x => x.Prezime),
        Pdf<Preko65GodinaStavka>.Centrirano("Датум\nрођења", 2.2, x => x.DatumRodjenja.ToString("dd.MM.yyyy")),
        Pdf<Preko65GodinaStavka>.Broj("Редован\nрад", 2.0, x => x.RedovanRad),
        Pdf<Preko65GodinaStavka>.Broj("Прековр.\nсати", 2.0, x => x.PrekovremeniSati),
        Pdf<Preko65GodinaStavka>.Broj("Дежурство\nсати", 2.0, x => x.DezurstvoSati),
        Pdf<Preko65GodinaStavka>.Broj("По позиву\nсати", 2.0, x => x.PoPozivuSati),
        Pdf<Preko65GodinaStavka>.Broj("Приправ.\nсати", 2.0, x => x.PripravnostSati),
        Pdf<Preko65GodinaStavka>.Broj("Укупно\nнето", 2.6, x => x.UkupanNeto, podebljano: true)
    ];

    protected override async Task<List<Preko65GodinaStavka>> UcitajAsync(IReadOnlyList<string> izabraniKljucevi)
    {
        var zuZapisi = await _pomocnik.UcitajIzabraneZuZapiseAsync(izabraniKljucevi);
        var rezultat = new List<Preko65GodinaStavka>();

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

                rezultat.Add(new Preko65GodinaStavka
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
