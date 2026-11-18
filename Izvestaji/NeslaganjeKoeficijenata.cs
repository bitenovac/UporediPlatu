using Microsoft.EntityFrameworkCore;
using UporediPlate.Data;
using UporediPlate.Izvestaji.Okvir;
using UporediPlate.Models;

namespace UporediPlate.Izvestaji;

// Разлика у основном или укупном коефицијенту између РФЗО и ЗУ
public class NeslaganjeKoeficijenataStavka : IStavkaIzvestaja
{
    public string SifraZuOriginal { get; set; } = string.Empty;
    public string SifraZuNormalizovana { get; set; } = string.Empty;
    public string NazivZu { get; set; } = string.Empty;
    public DateTime PeriodUvoza { get; set; }

    // Из ЗУ табеле
    public string Jmbg { get; set; } = string.Empty;
    public string Ime { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;

    // Из РФЗО табеле
    public string NazivRM { get; set; } = string.Empty;

    // Коефицијенти
    public decimal OsnovniKoefRfzo { get; set; }
    public decimal OsnovniKoefZu { get; set; }
    public decimal UkupniKoefRfzo { get; set; }
    public decimal UkupniKoefZu { get; set; }

    public bool RazlikaOsnovni => Math.Abs(OsnovniKoefRfzo - OsnovniKoefZu) >= 0.0001m;
    public bool RazlikaUkupni => Math.Abs(UkupniKoefRfzo - UkupniKoefZu) >= 0.0001m;
}

public class NeslaganjeKoeficijenataIzvestaj : Izvestaj<NeslaganjeKoeficijenataStavka>
{
    private readonly AppDbContext _context;
    private readonly IzvestajPomocnik _pomocnik;

    public NeslaganjeKoeficijenataIzvestaj(AppDbContext context, IzvestajPomocnik pomocnik)
    {
        _context = context;
        _pomocnik = pomocnik;
    }

    public override string Kljuc => "NeslaganjeKoeficijenata";
    public override string Naslov => "Неслагање у основним и укупним коефицијентима (РФЗО и ЗУ)";
    public override string Opis =>
        "Поређење основног коефицијента и збира коефицијената из РФЗО евиденције са подацима достављеним у ЗУ фајлу.";
    public override string Napomena => "Утврђена је разлика у основном или укупном коефицијенту између РФЗО и ЗУ евиденције";
    public override string PorukaBezRezultata =>
        "✅ Сви радници који се налазе у обе евиденције имају усклађене основне и укупне коефицијенте!";
    public override string NazivKartice => "3. Неслагање у<br/>коефицијентима";
    public override string PrefiksFajla => "Izvestaj_Neslaganje_Koeficijenata";

    public override IOrderedEnumerable<NeslaganjeKoeficijenataStavka> Sortiraj(IEnumerable<NeslaganjeKoeficijenataStavka> stavke) =>
        stavke.OrderBy(x => x.SifraZuNormalizovana).ThenBy(x => x.Prezime).ThenBy(x => x.Ime);

    // Коефицијент који се разликује истиче се црвеним на екрану
    private static string Razlika(bool postoji) => postoji ? "fw-bold text-danger" : "";

    public override IReadOnlyList<KolonaEkrana<NeslaganjeKoeficijenataStavka>> KoloneEkrana =>
    [
        Ekran<NeslaganjeKoeficijenataStavka>.Centrirano("Период", x => x.PeriodUvoza.ToString("MM.yyyy")),
        Ekran<NeslaganjeKoeficijenataStavka>.Jmbg(x => x.Jmbg),
        Ekran<NeslaganjeKoeficijenataStavka>.Tekst("Име", x => x.Ime),
        Ekran<NeslaganjeKoeficijenataStavka>.Tekst("Презиме", x => x.Prezime, klasaCelije: "fw-semibold"),
        Ekran<NeslaganjeKoeficijenataStavka>.RadnoMesto("Назив радног места (РФЗО)", x => x.NazivRM),
        Ekran<NeslaganjeKoeficijenataStavka>.Broj("Осн. коеф.<br/>(РФЗО)", x => x.OsnovniKoefRfzo, uslovnaKlasa: x => Razlika(x.RazlikaOsnovni)),
        Ekran<NeslaganjeKoeficijenataStavka>.Broj("Осн. коеф.<br/>(ЗУ)", x => x.OsnovniKoefZu, uslovnaKlasa: x => Razlika(x.RazlikaOsnovni)),
        Ekran<NeslaganjeKoeficijenataStavka>.Broj("Укуп. коеф.<br/>(РФЗО)", x => x.UkupniKoefRfzo, uslovnaKlasa: x => Razlika(x.RazlikaUkupni)),
        Ekran<NeslaganjeKoeficijenataStavka>.Broj("Укуп. коеф.<br/>(ЗУ)", x => x.UkupniKoefZu, uslovnaKlasa: x => Razlika(x.RazlikaUkupni))
    ];

    public override IReadOnlyList<KolonaExcela<NeslaganjeKoeficijenataStavka>> KoloneExcela =>
    [
        Excel<NeslaganjeKoeficijenataStavka>.Tekst("Период", x => x.PeriodUvoza.ToString("MM.yyyy"), centrirano: true),
        Excel<NeslaganjeKoeficijenataStavka>.Jmbg(x => x.Jmbg),
        Excel<NeslaganjeKoeficijenataStavka>.Tekst("Име", x => x.Ime),
        Excel<NeslaganjeKoeficijenataStavka>.Tekst("Презиме", x => x.Prezime),
        Excel<NeslaganjeKoeficijenataStavka>.Tekst("Назив радног места (РФЗО)", x => x.NazivRM, prelom: true, sirina: 38),
        Excel<NeslaganjeKoeficijenataStavka>.Broj("Осн. коеф. (РФЗО)", x => x.OsnovniKoefRfzo),
        Excel<NeslaganjeKoeficijenataStavka>.Broj("Осн. коеф. (ЗУ)", x => x.OsnovniKoefZu),
        Excel<NeslaganjeKoeficijenataStavka>.Broj("Укуп. коеф. (РФЗО)", x => x.UkupniKoefRfzo),
        Excel<NeslaganjeKoeficijenataStavka>.Broj("Укуп. коеф. (ЗУ)", x => x.UkupniKoefZu)
    ];

    public override IReadOnlyList<KolonaPdfa<NeslaganjeKoeficijenataStavka>> KolonePdfa =>
    [
        Pdf<NeslaganjeKoeficijenataStavka>.Centrirano("Период", 1.6, x => x.PeriodUvoza.ToString("MM.yyyy")),
        Pdf<NeslaganjeKoeficijenataStavka>.Jmbg(2.7, x => x.Jmbg),
        Pdf<NeslaganjeKoeficijenataStavka>.Tekst("Име", 2.7, x => x.Ime),
        Pdf<NeslaganjeKoeficijenataStavka>.Podebljano("Презиме", 3.0, x => x.Prezime),
        Pdf<NeslaganjeKoeficijenataStavka>.Tekst("Назив радног места (РФЗО)", 8.5, x => x.NazivRM),
        Pdf<NeslaganjeKoeficijenataStavka>.Broj("Осн. коеф.\n(РФЗО)", 2.2, x => x.OsnovniKoefRfzo),
        Pdf<NeslaganjeKoeficijenataStavka>.Broj("Осн. коеф.\n(ЗУ)", 2.2, x => x.OsnovniKoefZu),
        Pdf<NeslaganjeKoeficijenataStavka>.Broj("Укуп. коеф.\n(РФЗО)", 2.2, x => x.UkupniKoefRfzo),
        Pdf<NeslaganjeKoeficijenataStavka>.Broj("Укуп. коеф.\n(ЗУ)", 2.2, x => x.UkupniKoefZu)
    ];

    protected override async Task<List<NeslaganjeKoeficijenataStavka>> UcitajAsync(IReadOnlyList<string> izabraniKljucevi)
    {
        var rezultat = new List<NeslaganjeKoeficijenataStavka>();
        var zuZapisi = await _pomocnik.UcitajIzabraneZuZapiseAsync(izabraniKljucevi);

        if (zuZapisi.Count == 0)
        {
            return rezultat;
        }

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
            .Where(x => !string.IsNullOrWhiteSpace(x.Jmbg) &&
                        normSifre.Contains(_pomocnik.NormalizujSifruZu(x.SifraZu)))
            .GroupBy(x => (
                Sifra: _pomocnik.NormalizujSifruZu(x.SifraZu),
                Period: x.PeriodUvoza.Date,
                Jmbg: x.Jmbg.Trim()
            ))
            .ToDictionary(g => g.Key, g => g.ToList());

        const decimal tolerancija = 0.0001m;

        foreach (var zu in zuZapisi)
        {
            if (string.IsNullOrWhiteSpace(zu.Jmbg))
            {
                continue;
            }

            var normSifra = _pomocnik.NormalizujSifruZu(zu.SifraZu);
            var kljuc = (Sifra: normSifra, Period: zu.PeriodUvoza.Date, Jmbg: zu.Jmbg.Trim());

            if (!rfzoPoKljucu.TryGetValue(kljuc, out var poklapanjaRfzo))
            {
                continue;
            }

            var rfzo = poklapanjaRfzo
                .OrderByDescending(r => r.Koef1 ?? 0m)
                .First();

            var osnovniRfzo = Math.Round(rfzo.Koef1 ?? 0m, 4);
            var ukupniRfzo = Math.Round(
                (rfzo.Koef1 ?? 0m) +
                (rfzo.DodKzaRuk ?? 0m) +
                (rfzo.DodKzaSiNzv ?? 0m) +
                (rfzo.Dodatni036 ?? 0m), 4);

            var osnovniZu = Math.Round(zu.OsnovniKoef, 4);
            var ukupniZu = Math.Round(zu.UkupniKoef, 4);

            var razlikaOsnovni = Math.Abs(osnovniRfzo - osnovniZu) >= tolerancija;
            var razlikaUkupni = Math.Abs(ukupniRfzo - ukupniZu) >= tolerancija;

            if (razlikaOsnovni || razlikaUkupni)
            {
                rezultat.Add(new NeslaganjeKoeficijenataStavka
                {
                    SifraZuOriginal = zu.SifraZu,
                    SifraZuNormalizovana = normSifra,
                    NazivZu = string.IsNullOrWhiteSpace(zu.NazivZu) ? rfzo.NazivZU ?? string.Empty : zu.NazivZu,
                    PeriodUvoza = zu.PeriodUvoza,
                    Jmbg = zu.Jmbg.Trim(),
                    Ime = zu.Ime?.Trim() ?? string.Empty,
                    Prezime = zu.Prezime?.Trim() ?? string.Empty,
                    NazivRM =
                        string.IsNullOrWhiteSpace(rfzo.NazivRM) ? rfzo.NazivZanimanja ?? "-" : rfzo.NazivRM.Trim(),
                    OsnovniKoefRfzo = osnovniRfzo,
                    OsnovniKoefZu = osnovniZu,
                    UkupniKoefRfzo = ukupniRfzo,
                    UkupniKoefZu = ukupniZu
                });
            }
        }

        return rezultat
            .OrderBy(x => x.SifraZuNormalizovana)
            .ThenBy(x => x.Prezime)
            .ThenBy(x => x.Ime)
            .ToList();
    }
}
