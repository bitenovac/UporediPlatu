using Microsoft.EntityFrameworkCore;
using UporediPlate.Data;
using UporediPlate.Izvestaji.Okvir;
using UporediPlate.Models;

namespace UporediPlate.Izvestaji;

// Радници из ЗУ фајла који нису пронађени у РФЗО евиденцији
public class NepronadjeniURfzoStavka : IStavkaIzvestaja
{
    public string SifraZuOriginal { get; set; } = string.Empty;
    public string SifraZuNormalizovana { get; set; } = string.Empty;
    public string NazivZu { get; set; } = string.Empty;
    public string Nivo { get; set; } = string.Empty;
    public string Jmbg { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;
    public string Ime { get; set; } = string.Empty;
    public string RadnoMesto { get; set; } = string.Empty;
    public decimal UkupniKoef { get; set; }
    public decimal UkupanNeto { get; set; }
    public decimal Bruto1 { get; set; }
    public DateTime PeriodUvoza { get; set; }
    public string NazivFajla { get; set; } = string.Empty;
    public string RazlogNeslaganja { get; set; } = string.Empty;
}

public class NepronadjeniURfzoIzvestaj : Izvestaj<NepronadjeniURfzoStavka>
{
    private readonly AppDbContext _context;
    private readonly IzvestajPomocnik _pomocnik;

    public NepronadjeniURfzoIzvestaj(AppDbContext context, IzvestajPomocnik pomocnik)
    {
        _context = context;
        _pomocnik = pomocnik;
    }

    public override string Kljuc => "NepronadjeniURfzo";
    public override string Naslov => "Радници који се налазе на списку ЗУ а који нису на списку РФЗО-а";
    public override string Opis =>
        "Приказ запослених из изабраних ЗУ фајлова чији се ЈМБГ и Шифра ЗУ (без водеће јединице) не налазе у РФЗО бази за дати период.";
    public override string Napomena => "ЈМБГ није пронађен у РФЗО евиденцији за овај период";
    public override string PorukaBezRezultata => "✅ Сви радници из изабраних ЗУ фајлова су успешно пронађени у РФЗО евиденцији!";
    public override string NazivKartice => "1. На списку ЗУ,<br/>а нису у РФЗО";
    public override string PrefiksFajla => "Izvestaj_Nepronadjeni_U_RFZO";
    public override bool Polozeno => false;
    public override bool CentrirajRedove => false;

    public override IOrderedEnumerable<NepronadjeniURfzoStavka> Sortiraj(IEnumerable<NepronadjeniURfzoStavka> stavke) =>
        stavke.OrderBy(x => x.SifraZuNormalizovana).ThenBy(x => x.Nivo).ThenBy(x => x.Prezime).ThenBy(x => x.Ime);

    public override IReadOnlyList<KolonaEkrana<NepronadjeniURfzoStavka>> KoloneEkrana =>
    [
        Ekran<NepronadjeniURfzoStavka>.Tekst("Период", x => x.PeriodUvoza.ToString("MM.yyyy")),
        Ekran<NepronadjeniURfzoStavka>.Tekst("Ниво", x => x.Nivo),
        Ekran<NepronadjeniURfzoStavka>.Jmbg(x => x.Jmbg, centrirano: false),
        Ekran<NepronadjeniURfzoStavka>.Tekst("Презиме и име", x => $"{x.Prezime} {x.Ime}", klasaCelije: "fw-semibold"),
        Ekran<NepronadjeniURfzoStavka>.Tekst("Радно место", x => x.RadnoMesto)
    ];

    public override IReadOnlyList<KolonaExcela<NepronadjeniURfzoStavka>> KoloneExcela =>
    [
        Excel<NepronadjeniURfzoStavka>.Tekst("Период", x => x.PeriodUvoza.ToString("MM.yyyy"), centrirano: true),
        Excel<NepronadjeniURfzoStavka>.Tekst("Ниво", x => x.Nivo, centrirano: true),
        Excel<NepronadjeniURfzoStavka>.Jmbg(x => x.Jmbg),
        Excel<NepronadjeniURfzoStavka>.Tekst("Презиме и име", x => $"{x.Prezime} {x.Ime}"),
        Excel<NepronadjeniURfzoStavka>.Tekst("Радно место", x => x.RadnoMesto, prelom: true)
    ];

    public override IReadOnlyList<KolonaPdfa<NepronadjeniURfzoStavka>> KolonePdfa =>
    [
        Pdf<NepronadjeniURfzoStavka>.Centrirano("Период", 1.8, x => x.PeriodUvoza.ToString("MM.yyyy")),
        Pdf<NepronadjeniURfzoStavka>.Centrirano("Ниво", 1.4, x => x.Nivo),
        Pdf<NepronadjeniURfzoStavka>.Jmbg(2.8, x => x.Jmbg),
        Pdf<NepronadjeniURfzoStavka>.Podebljano("Презиме и име", 4.6, x => $"{x.Prezime} {x.Ime}"),
        Pdf<NepronadjeniURfzoStavka>.Tekst("Радно место", 8.0, x => x.RadnoMesto)
    ];

    protected override async Task<List<NepronadjeniURfzoStavka>> UcitajAsync(IReadOnlyList<string> izabraniKljucevi)
    {
        var rezultat = new List<NepronadjeniURfzoStavka>();
        var zuZapisi = await _pomocnik.UcitajIzabraneZuZapiseAsync(izabraniKljucevi);

        if (zuZapisi.Count == 0)
        {
            return rezultat;
        }

        var periodi = zuZapisi
            .Select(x => new { x.PeriodUvoza.Year, x.PeriodUvoza.Month })
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

        var rfzoParovi = new HashSet<string>(
            rfzoZapisi.Select(r => $"{r.PeriodUvoza:yyyyMM}|{(r.Jmbg ?? "").Trim()}|{(r.SifraZu ?? "").Trim()}")
        );

        var rfzoSamoJmbg = new HashSet<string>(
            rfzoZapisi.Select(r => $"{r.PeriodUvoza:yyyyMM}|{(r.Jmbg ?? "").Trim()}")
        );

        foreach (var zu in zuZapisi)
        {
            var jmbgClean = (zu.Jmbg ?? string.Empty).Trim();
            var sifraNorm = _pomocnik.NormalizujSifruZu(zu.SifraZu);
            var periodKljuc = zu.PeriodUvoza.ToString("yyyyMM");

            var punKljuc = $"{periodKljuc}|{jmbgClean}|{sifraNorm}";

            if (!rfzoParovi.Contains(punKljuc))
            {
                var razlog = rfzoSamoJmbg.Contains($"{periodKljuc}|{jmbgClean}")
                    ? "ЈМБГ постоји у РФЗО, али под другом шифром ЗУ"
                    : "ЈМБГ није пронађен у РФЗО евиденцији за овај период";

                rezultat.Add(new NepronadjeniURfzoStavka
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
