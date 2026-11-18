using Microsoft.EntityFrameworkCore;
using UporediPlate.Data;
using UporediPlate.Izvestaji.Okvir;
using UporediPlate.Models;

namespace UporediPlate.Izvestaji;

// Радници из РФЗО евиденције који нису достављени у ЗУ фајлу
public class NepronadjeniUZuStavka : IStavkaIzvestaja
{
    public string SifraZuNormalizovana { get; set; } = string.Empty;
    public string NazivZu { get; set; } = string.Empty;
    public string NazivOJ { get; set; } = string.Empty;
    public string Jmbg { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;
    public string Ime { get; set; } = string.Empty;
    public string NazivZanimanja { get; set; } = string.Empty;
    public string NazivRM { get; set; } = string.Empty;
    public decimal? Koef1 { get; set; }
    public decimal? ProcRVr { get; set; }
    public DateTime? DatumPrestankaRO { get; set; }
    public DateTime PeriodUvoza { get; set; }
    public string NazivFajlaRfzo { get; set; } = string.Empty;
    public string RazlogNeslaganja { get; set; } = string.Empty;

    // РФЗО евиденција нема оригиналну шифру ЗУ из фајла
    public string? SifraZuOriginal => null;

    public string RadnoMestoIliZanimanje => string.IsNullOrWhiteSpace(NazivRM) ? NazivZanimanja : NazivRM;
}

public class NepronadjeniUZuIzvestaj : Izvestaj<NepronadjeniUZuStavka>
{
    private readonly AppDbContext _context;
    private readonly IzvestajPomocnik _pomocnik;

    public NepronadjeniUZuIzvestaj(AppDbContext context, IzvestajPomocnik pomocnik)
    {
        _context = context;
        _pomocnik = pomocnik;
    }

    public override string Kljuc => "NepronadjeniUZu";
    public override string Naslov => "Радници на списку РФЗО-а који нису на списку ЗУ";
    public override string Opis =>
        "Приказ запослених из РФЗО евиденције (за изабране ЗУ установе и периоде) чији се ЈМБГ не налази у достављеним ЗУ фајловима.";
    public override string Napomena => "Радник је на списку РФЗО-а, али није достављен у ЗУ фајлу";
    public override string PorukaBezRezultata => "✅ Сви радници из РФЗО евиденције за изабране установе су пронађени у ЗУ фајловима!";
    public override string NazivKartice => "2. На списку РФЗО,<br/>а нису у ЗУ";
    public override string PrefiksFajla => "Izvestaj_Nepronadjeni_U_ZU";
    public override bool Polozeno => false;
    public override bool CentrirajRedove => false;

    public override IOrderedEnumerable<NepronadjeniUZuStavka> Sortiraj(IEnumerable<NepronadjeniUZuStavka> stavke) =>
        stavke.OrderBy(x => x.SifraZuNormalizovana).ThenBy(x => x.Prezime).ThenBy(x => x.Ime);

    public override IReadOnlyList<KolonaEkrana<NepronadjeniUZuStavka>> KoloneEkrana =>
    [
        Ekran<NepronadjeniUZuStavka>.Tekst("Период", x => x.PeriodUvoza.ToString("MM.yyyy")),
        Ekran<NepronadjeniUZuStavka>.Jmbg(x => x.Jmbg, centrirano: false),
        Ekran<NepronadjeniUZuStavka>.Tekst("Презиме и име", x => $"{x.Prezime} {x.Ime}", klasaCelije: "fw-semibold"),
        Ekran<NepronadjeniUZuStavka>.Tekst("Радно место / Занимање", x => x.RadnoMestoIliZanimanje)
    ];

    public override IReadOnlyList<KolonaExcela<NepronadjeniUZuStavka>> KoloneExcela =>
    [
        Excel<NepronadjeniUZuStavka>.Tekst("Период", x => x.PeriodUvoza.ToString("MM.yyyy"), centrirano: true),
        Excel<NepronadjeniUZuStavka>.Jmbg(x => x.Jmbg),
        Excel<NepronadjeniUZuStavka>.Tekst("Презиме и име", x => $"{x.Prezime} {x.Ime}"),
        Excel<NepronadjeniUZuStavka>.Tekst("Радно место / Занимање", x => x.RadnoMestoIliZanimanje, prelom: true)
    ];

    public override IReadOnlyList<KolonaPdfa<NepronadjeniUZuStavka>> KolonePdfa =>
    [
        Pdf<NepronadjeniUZuStavka>.Centrirano("Период", 2.0, x => x.PeriodUvoza.ToString("MM.yyyy")),
        Pdf<NepronadjeniUZuStavka>.Jmbg(3.0, x => x.Jmbg),
        Pdf<NepronadjeniUZuStavka>.Podebljano("Презиме и име", 5.0, x => $"{x.Prezime} {x.Ime}"),
        Pdf<NepronadjeniUZuStavka>.Tekst("Радно место / Занимање", 8.6, x => x.RadnoMestoIliZanimanje)
    ];

    protected override async Task<List<NepronadjeniUZuStavka>> UcitajAsync(IReadOnlyList<string> izabraniKljucevi)
    {
        var rezultat = new List<NepronadjeniUZuStavka>();
        var zuZapisi = await _pomocnik.UcitajIzabraneZuZapiseAsync(izabraniKljucevi);

        if (zuZapisi.Count == 0)
        {
            return rezultat;
        }

        var izabraneUstanoveIPeriodi = zuZapisi
            .Select(z => new
            {
                z.PeriodUvoza.Year,
                z.PeriodUvoza.Month,
                SifraNorm = _pomocnik.NormalizujSifruZu(z.SifraZu),
                z.NazivZu
            })
            .Distinct()
            .ToList();

        var dozvoljeniParoviUstanova = new HashSet<string>(
            izabraneUstanoveIPeriodi.Select(u => $"{u.Year:D4}{u.Month:D2}|{u.SifraNorm}")
        );

        var naziviZuMapa = izabraneUstanoveIPeriodi
            .GroupBy(u => u.SifraNorm)
            .ToDictionary(g => g.Key, g => g.First().NazivZu);

        var zuParoviRadnika = new HashSet<string>(
            zuZapisi.Select(z =>
                $"{z.PeriodUvoza:yyyyMM}|{(z.Jmbg ?? "").Trim()}|{_pomocnik.NormalizujSifruZu(z.SifraZu)}")
        );

        var zuSamoJmbg = new HashSet<string>(
            zuZapisi.Select(z => $"{z.PeriodUvoza:yyyyMM}|{(z.Jmbg ?? "").Trim()}")
        );

        var periodi = izabraneUstanoveIPeriodi
            .Select(x => new { x.Year, x.Month })
            .Distinct()
            .ToList();

        var rfzoZapisi = new List<RfzoUvoz>();
        foreach (var p in periodi)
        {
            var rfzoZaPeriod = await _context.RfzoUvozi
                .AsNoTracking()
                .Where(r => r.PeriodUvoza.Year == p.Year && r.PeriodUvoza.Month == p.Month)
                .ToListAsync();

            rfzoZapisi.AddRange(rfzoZaPeriod);
        }

        foreach (var rfzo in rfzoZapisi)
        {
            var sifraRfzo = (rfzo.SifraZu ?? string.Empty).Trim();
            var periodKljuc = rfzo.PeriodUvoza.ToString("yyyyMM");
            var kljucUstanove = $"{periodKljuc}|{sifraRfzo}";

            if (!dozvoljeniParoviUstanova.Contains(kljucUstanove))
            {
                continue;
            }

            var jmbgClean = (rfzo.Jmbg ?? string.Empty).Trim();
            var punKljucRadnika = $"{periodKljuc}|{jmbgClean}|{sifraRfzo}";

            if (!zuParoviRadnika.Contains(punKljucRadnika))
            {
                var razlog = zuSamoJmbg.Contains($"{periodKljuc}|{jmbgClean}")
                    ? "ЈМБГ постоји у другом изабраном ЗУ фајлу, али не у овој установи"
                    : "Радник је на списку РФЗО-а, али није достављен у ЗУ фајлу";

                var nazivUstanove = !string.IsNullOrWhiteSpace(rfzo.NazivZU)
                    ? rfzo.NazivZU
                    : naziviZuMapa.ContainsKey(sifraRfzo)
                        ? naziviZuMapa[sifraRfzo]
                        : string.Empty;

                rezultat.Add(new NepronadjeniUZuStavka
                {
                    SifraZuNormalizovana = sifraRfzo,
                    NazivZu = nazivUstanove,
                    NazivOJ = rfzo.NazivOJ ?? string.Empty,
                    Jmbg = jmbgClean,
                    Prezime = rfzo.Prezime ?? string.Empty,
                    Ime = rfzo.Ime ?? string.Empty,
                    NazivZanimanja = rfzo.NazivZanimanja ?? string.Empty,
                    NazivRM = rfzo.NazivRM ?? string.Empty,
                    Koef1 = rfzo.Koef1,
                    ProcRVr = rfzo.ProcRVr,
                    DatumPrestankaRO = rfzo.DatumPrestankaRO,
                    PeriodUvoza = rfzo.PeriodUvoza,
                    NazivFajlaRfzo = rfzo.NazivFajla ?? string.Empty,
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
