using Microsoft.EntityFrameworkCore;
using UporediPlate.Data;
using UporediPlate.Izvestaji.Okvir;
using UporediPlate.Models;

namespace UporediPlate.Izvestaji;

// Запослени са непуним радним временом у РФЗО и њихови сати у ЗУ
public class NepunoRadnoVremeStavka : IStavkaIzvestaja
{
    public string SifraZuOriginal { get; set; } = string.Empty;
    public string SifraZuNormalizovana { get; set; } = string.Empty;
    public string NazivZu { get; set; } = string.Empty;
    public DateTime PeriodUvoza { get; set; }

    public string Jmbg { get; set; } = string.Empty;
    public string Ime { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;
    public string RadnoMesto { get; set; } = string.Empty;

    // ProcRVr iz RFZO tabele (npr. 80, 50)
    public decimal ProcenatRadnogVremenaRfzo { get; set; }

    // Maksimalni broj sati koji sme da radi = Max(SatiUkupno) za ZU i period * (ProcRVr / 100)
    public decimal MaksimalnoDozvoljeniSati { get; set; }

    // Sati i neto iz ZU tabele
    public decimal RedovanRadZu { get; set; }
    public decimal PrekovremeniSatiZu { get; set; }
    public decimal DezurstvoSatiZu { get; set; }
    public decimal UkupanNetoZu { get; set; }
}

public class NepunoRadnoVremeIzvestaj : Izvestaj<NepunoRadnoVremeStavka>
{
    private readonly AppDbContext _context;
    private readonly IzvestajPomocnik _pomocnik;

    public NepunoRadnoVremeIzvestaj(AppDbContext context, IzvestajPomocnik pomocnik)
    {
        _context = context;
        _pomocnik = pomocnik;
    }

    public override string Kljuc => "NepunoRadnoVreme";
    public override string Naslov => "Контрола запослених са непуним радним временом (РФЗО vs ЗУ)";
    public override string Opis =>
        "Приказ запослених који у РФЗО евиденцији имају проценат радног времена мањи од 100%, уз обрачун максимално дозвољених сати на основу фонда установе.";
    public override string Napomena =>
        "Напомена: Црвеним фонтом су истакнути запослени код којих су сати редовног рада већи од максимално дозвољеног фонда.";
    public override string PorukaBezRezultata => "✅ У изабраним фајловима нема запослених са процентом радног времена мањим од 100%!";
    public override string NazivKartice => "6. Непуно радно<br/>време (РФЗО vs ЗУ)";
    public override string PrefiksFajla => "Izvestaj_Nepuno_Radno_Vreme";
    public override Func<NepunoRadnoVremeStavka, bool> IstaknutRed => x => x.RedovanRadZu > x.MaksimalnoDozvoljeniSati;

    public override IOrderedEnumerable<NepunoRadnoVremeStavka> Sortiraj(IEnumerable<NepunoRadnoVremeStavka> stavke) =>
        stavke.OrderBy(x => x.SifraZuNormalizovana).ThenBy(x => x.ProcenatRadnogVremenaRfzo)
            .ThenBy(x => x.Prezime).ThenBy(x => x.Ime);

    public override IReadOnlyList<KolonaEkrana<NepunoRadnoVremeStavka>> KoloneEkrana =>
    [
        Ekran<NepunoRadnoVremeStavka>.Centrirano("Период", x => x.PeriodUvoza.ToString("MM.yyyy")),
        Ekran<NepunoRadnoVremeStavka>.Jmbg(x => x.Jmbg),
        Ekran<NepunoRadnoVremeStavka>.Tekst("Име", x => x.Ime),
        Ekran<NepunoRadnoVremeStavka>.Tekst("Презиме", x => x.Prezime, klasaCelije: "fw-semibold"),
        Ekran<NepunoRadnoVremeStavka>.RadnoMesto("Радно место", x => x.RadnoMesto),
        Ekran<NepunoRadnoVremeStavka>.Broj("% РВ<br/>(РФЗО)", x => x.ProcenatRadnogVremenaRfzo, "fw-bold", sufiks: "%"),
        Ekran<NepunoRadnoVremeStavka>.Broj("Макс. сати<br/>(РФЗО)", x => x.MaksimalnoDozvoljeniSati, "fw-semibold"),
        Ekran<NepunoRadnoVremeStavka>.Broj("Редован<br/>рад (ЗУ)", x => x.RedovanRadZu),
        Ekran<NepunoRadnoVremeStavka>.Broj("Прековремено<br/>(ЗУ)", x => x.PrekovremeniSatiZu),
        Ekran<NepunoRadnoVremeStavka>.Broj("Дежурство<br/>(ЗУ)", x => x.DezurstvoSatiZu),
        Ekran<NepunoRadnoVremeStavka>.Broj("Укупно<br/>нето (ЗУ)", x => x.UkupanNetoZu, "fw-bold")
    ];

    public override IReadOnlyList<KolonaExcela<NepunoRadnoVremeStavka>> KoloneExcela =>
    [
        Excel<NepunoRadnoVremeStavka>.Tekst("Период", x => x.PeriodUvoza.ToString("MM.yyyy"), centrirano: true),
        Excel<NepunoRadnoVremeStavka>.Jmbg(x => x.Jmbg),
        Excel<NepunoRadnoVremeStavka>.Tekst("Име", x => x.Ime),
        Excel<NepunoRadnoVremeStavka>.Tekst("Презиме", x => x.Prezime),
        Excel<NepunoRadnoVremeStavka>.Tekst("Радно место", x => x.RadnoMesto, prelom: true, sirina: 36),
        Excel<NepunoRadnoVremeStavka>.Broj("% РВ (РФЗО)", x => x.ProcenatRadnogVremenaRfzo, podebljano: true),
        Excel<NepunoRadnoVremeStavka>.Broj("Макс. сати (РФЗО)", x => x.MaksimalnoDozvoljeniSati),
        Excel<NepunoRadnoVremeStavka>.Broj("Редован рад (ЗУ)", x => x.RedovanRadZu),
        Excel<NepunoRadnoVremeStavka>.Broj("Прековремено (ЗУ)", x => x.PrekovremeniSatiZu),
        Excel<NepunoRadnoVremeStavka>.Broj("Дежурство (ЗУ)", x => x.DezurstvoSatiZu),
        Excel<NepunoRadnoVremeStavka>.Broj("Укупно нето (ЗУ)", x => x.UkupanNetoZu, podebljano: true)
    ];

    public override IReadOnlyList<KolonaPdfa<NepunoRadnoVremeStavka>> KolonePdfa =>
    [
        Pdf<NepunoRadnoVremeStavka>.Centrirano("Период", 1.6, x => x.PeriodUvoza.ToString("MM.yyyy")),
        Pdf<NepunoRadnoVremeStavka>.Jmbg(2.4, x => x.Jmbg),
        Pdf<NepunoRadnoVremeStavka>.Tekst("Име", 2.1, x => x.Ime),
        Pdf<NepunoRadnoVremeStavka>.Podebljano("Презиме", 2.4, x => x.Prezime),
        Pdf<NepunoRadnoVremeStavka>.Tekst("Радно место", 5.8, x => x.RadnoMesto),
        Pdf<NepunoRadnoVremeStavka>.Broj("% РВ\n(РФЗО)", 1.7, x => x.ProcenatRadnogVremenaRfzo, podebljano: true, sufiks: "%"),
        Pdf<NepunoRadnoVremeStavka>.Broj("Макс.\nсати", 1.9, x => x.MaksimalnoDozvoljeniSati),
        Pdf<NepunoRadnoVremeStavka>.Broj("Редован\nрад (ЗУ)", 1.9, x => x.RedovanRadZu),
        Pdf<NepunoRadnoVremeStavka>.Broj("Прековр.\n(ЗУ)", 1.9, x => x.PrekovremeniSatiZu),
        Pdf<NepunoRadnoVremeStavka>.Broj("Дежурство\n(ЗУ)", 1.9, x => x.DezurstvoSatiZu),
        Pdf<NepunoRadnoVremeStavka>.Broj("Укупно\nнето (ЗУ)", 2.5, x => x.UkupanNetoZu, podebljano: true)
    ];

    protected override async Task<List<NepunoRadnoVremeStavka>> UcitajAsync(IReadOnlyList<string> izabraniKljucevi)
    {
        var rezultat = new List<NepunoRadnoVremeStavka>();
        var zuZapisi = await _pomocnik.UcitajIzabraneZuZapiseAsync(izabraniKljucevi);

        if (zuZapisi.Count == 0)
        {
            return rezultat;
        }

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
            .Where(x => !string.IsNullOrWhiteSpace(x.Jmbg) &&
                        normSifre.Contains(_pomocnik.NormalizujSifruZu(x.SifraZu)))
            .GroupBy(x => (
                Sifra: _pomocnik.NormalizujSifruZu(x.SifraZu),
                Period: x.PeriodUvoza.Date,
                Jmbg: x.Jmbg.Trim()
            ))
            .ToDictionary(g => g.Key, g => g.ToList());

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
                .OrderBy(r => r.ProcRVr ?? 100m)
                .First();

            // Svi zaposleni kod kojih je u RFZO evidenciji procenat radnog vremena manji od 100%
            if (rfzo.ProcRVr.HasValue && rfzo.ProcRVr.Value < 100m)
            {
                maxSatiPoZuIPeriodu.TryGetValue((Sifra: normSifra, Period: zu.PeriodUvoza.Date),
                    out var maxFondUstanove);
                var dozvoljeniSati = Math.Round(maxFondUstanove * (rfzo.ProcRVr.Value / 100m), 2);

                var radnoMesto = !string.IsNullOrWhiteSpace(rfzo.NazivRM)
                    ? rfzo.NazivRM.Trim()
                    : !string.IsNullOrWhiteSpace(zu.RadnoMesto)
                        ? zu.RadnoMesto.Trim()
                        : "-";

                rezultat.Add(new NepunoRadnoVremeStavka
                {
                    SifraZuOriginal = zu.SifraZu,
                    SifraZuNormalizovana = normSifra,
                    NazivZu = string.IsNullOrWhiteSpace(zu.NazivZu) ? rfzo.NazivZU ?? string.Empty : zu.NazivZu,
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
