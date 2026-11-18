using UporediPlate.Izvestaji.Okvir;
using UporediPlate.Models;

namespace UporediPlate.Izvestaji;

// Прековремени рад, дежурство, рад по позиву и приправност из ЗУ фајла
public class PrekovremeniSatiStavka : IStavkaIzvestaja
{
    public string SifraZuOriginal { get; set; } = string.Empty;
    public string SifraZuNormalizovana { get; set; } = string.Empty;
    public string NazivZu { get; set; } = string.Empty;
    public DateTime PeriodUvoza { get; set; }

    public string Jmbg { get; set; } = string.Empty;
    public string Ime { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;
    public string RadnoMesto { get; set; } = string.Empty;

    public decimal PrekovremeniSati { get; set; }
    public decimal DezurstvoSati { get; set; }
    public decimal PoPozivuSati { get; set; }
    public decimal PripravnostSati { get; set; }

    // Zbir prethodne četiri kolone (Prekovremeni + Dežurstvo + Po pozivu + Pripravnost)
    public decimal UkupnoDodatnihSati { get; set; }

    // Poslednja kolona u izveštaju pod nazivom "Радно време" (SatiUkupno iz ZU tabele)
    public decimal RadnoVreme { get; set; }
}

public class PrekovremeniSatiIzvestaj : Izvestaj<PrekovremeniSatiStavka>
{
    private readonly IzvestajPomocnik _pomocnik;

    public PrekovremeniSatiIzvestaj(IzvestajPomocnik pomocnik) => _pomocnik = pomocnik;

    public override string Kljuc => "PrekovremeniSati";
    public override string Naslov => "Преглед прековременог рада, дежурства, рада по позиву и приправности (ЗУ)";
    public override string Opis =>
        "Приказ запослених из ЗУ евиденције код којих је збир додатних сати већи од нуле, сортирано опадајуће по укупним додатним сатима.";
    public override string Napomena =>
        "Приказ запослених из ЗУ евиденције код којих је збир додатних сати већи од нуле (сортирано опадајуће)";
    public override string PorukaBezRezultata =>
        "✅ У изабраним ЗУ фајловима нема запослених са прековременим радом, дежурством, радом по позиву или приправношћу!";
    public override string NazivKartice => "4. Прековремени<br/>и додатни сати";
    public override string PrefiksFajla => "Izvestaj_Prekovremeni_Sati";

    public override IOrderedEnumerable<PrekovremeniSatiStavka> Sortiraj(IEnumerable<PrekovremeniSatiStavka> stavke) =>
        stavke.OrderBy(x => x.SifraZuNormalizovana).ThenByDescending(x => x.UkupnoDodatnihSati)
            .ThenBy(x => x.Prezime).ThenBy(x => x.Ime);

    public override IReadOnlyList<KolonaEkrana<PrekovremeniSatiStavka>> KoloneEkrana =>
    [
        Ekran<PrekovremeniSatiStavka>.Jmbg(x => x.Jmbg),
        Ekran<PrekovremeniSatiStavka>.Tekst("Име", x => x.Ime),
        Ekran<PrekovremeniSatiStavka>.Tekst("Презиме", x => x.Prezime, klasaCelije: "fw-semibold"),
        Ekran<PrekovremeniSatiStavka>.RadnoMesto("Радно место", x => x.RadnoMesto),
        Ekran<PrekovremeniSatiStavka>.Broj("Прековремени<br/>сати", x => x.PrekovremeniSati),
        Ekran<PrekovremeniSatiStavka>.Broj("Дежурство<br/>сати", x => x.DezurstvoSati),
        Ekran<PrekovremeniSatiStavka>.Broj("По позиву<br/>сати", x => x.PoPozivuSati),
        Ekran<PrekovremeniSatiStavka>.Broj("Приправност<br/>сати", x => x.PripravnostSati),
        Ekran<PrekovremeniSatiStavka>.Broj("Укупно<br/>сати", x => x.UkupnoDodatnihSati, "fw-bold text-primary"),
        Ekran<PrekovremeniSatiStavka>.Broj("Радно<br/>време", x => x.RadnoVreme)
    ];

    public override IReadOnlyList<KolonaExcela<PrekovremeniSatiStavka>> KoloneExcela =>
    [
        Excel<PrekovremeniSatiStavka>.Jmbg(x => x.Jmbg),
        Excel<PrekovremeniSatiStavka>.Tekst("Име", x => x.Ime),
        Excel<PrekovremeniSatiStavka>.Tekst("Презиме", x => x.Prezime),
        Excel<PrekovremeniSatiStavka>.Tekst("Радно место", x => x.RadnoMesto, prelom: true, sirina: 36),
        Excel<PrekovremeniSatiStavka>.Broj("Прековремени сати", x => x.PrekovremeniSati),
        Excel<PrekovremeniSatiStavka>.Broj("Дежурство сати", x => x.DezurstvoSati),
        Excel<PrekovremeniSatiStavka>.Broj("По позиву сати", x => x.PoPozivuSati),
        Excel<PrekovremeniSatiStavka>.Broj("Приправност сати", x => x.PripravnostSati),
        Excel<PrekovremeniSatiStavka>.Broj("Укупно сати", x => x.UkupnoDodatnihSati, podebljano: true),
        Excel<PrekovremeniSatiStavka>.Broj("Радно време", x => x.RadnoVreme)
    ];

    public override IReadOnlyList<KolonaPdfa<PrekovremeniSatiStavka>> KolonePdfa =>
    [
        Pdf<PrekovremeniSatiStavka>.Jmbg(2.5, x => x.Jmbg),
        Pdf<PrekovremeniSatiStavka>.Tekst("Име", 2.3, x => x.Ime),
        Pdf<PrekovremeniSatiStavka>.Podebljano("Презиме", 2.6, x => x.Prezime),
        Pdf<PrekovremeniSatiStavka>.Tekst("Радно место", 7.5, x => x.RadnoMesto),
        Pdf<PrekovremeniSatiStavka>.Broj("Прековр.\nсати", 2.0, x => x.PrekovremeniSati),
        Pdf<PrekovremeniSatiStavka>.Broj("Дежурство\nсати", 2.0, x => x.DezurstvoSati),
        Pdf<PrekovremeniSatiStavka>.Broj("По позиву\nсати", 2.0, x => x.PoPozivuSati),
        Pdf<PrekovremeniSatiStavka>.Broj("Приправ.\nсати", 2.0, x => x.PripravnostSati),
        Pdf<PrekovremeniSatiStavka>.Broj("Укупно\nсати", 2.2, x => x.UkupnoDodatnihSati, podebljano: true),
        Pdf<PrekovremeniSatiStavka>.Broj("Радно\nвреме", 2.2, x => x.RadnoVreme)
    ];

    protected override async Task<List<PrekovremeniSatiStavka>> UcitajAsync(IReadOnlyList<string> izabraniKljucevi)
    {
        var zuZapisi = await _pomocnik.UcitajIzabraneZuZapiseAsync(izabraniKljucevi);
        var rezultat = new List<PrekovremeniSatiStavka>();

        foreach (var zu in zuZapisi)
        {
            var ukupnoDodatnih = zu.PrekovremeniSati + zu.DezurstvoSati + zu.PoPozivuSati + zu.PripravnostSati;

            if (ukupnoDodatnih > 0)
            {
                rezultat.Add(new PrekovremeniSatiStavka
                {
                    SifraZuOriginal = zu.SifraZu ?? string.Empty,
                    SifraZuNormalizovana = _pomocnik.NormalizujSifruZu(zu.SifraZu),
                    NazivZu = zu.NazivZu ?? string.Empty,
                    PeriodUvoza = zu.PeriodUvoza,
                    Jmbg = zu.Jmbg ?? string.Empty,
                    Ime = zu.Ime ?? string.Empty,
                    Prezime = zu.Prezime ?? string.Empty,
                    RadnoMesto = zu.RadnoMesto ?? string.Empty,
                    PrekovremeniSati = zu.PrekovremeniSati,
                    DezurstvoSati = zu.DezurstvoSati,
                    PoPozivuSati = zu.PoPozivuSati,
                    PripravnostSati = zu.PripravnostSati,
                    UkupnoDodatnihSati = ukupnoDodatnih,
                    RadnoVreme = zu.SatiUkupno
                });
            }
        }

        return rezultat
            .OrderBy(x => x.SifraZuNormalizovana)
            .ThenByDescending(x => x.UkupnoDodatnihSati)
            .ThenBy(x => x.Prezime)
            .ThenBy(x => x.Ime)
            .ToList();
    }
}
