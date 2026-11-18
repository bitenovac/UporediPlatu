namespace UporediPlate.Izvestaji.Okvir;

// Модел за заједнички поглед Views/Izvestaj/Izvestaj.cshtml - све је већ форматирано, поглед само исцртава
public class PrikazIzvestaja
{
    private const string StilIstaknutogReda = "color: #dc3545 !important; font-weight: 700;";
    private const string StilObicnogReda = "color: #212529 !important;";

    public required string Kljuc { get; init; }
    public required string Naslov { get; init; }
    public required string NaslovStranice { get; init; }
    public required string Opis { get; init; }
    public required string Napomena { get; init; }
    public required string PorukaBezRezultata { get; init; }
    public required string NazivKartice { get; init; }
    public required string IzabraniKljuceviStr { get; init; }
    public required IReadOnlyList<KolonaPrikaza> Kolone { get; init; }
    public required IReadOnlyList<GrupaPrikaza> Grupe { get; init; }

    public int Ukupno => Grupe.Sum(g => g.Redovi.Count);

    public static PrikazIzvestaja Napravi<T>(Izvestaj<T> izvestaj, List<T> stavke, string izabraniKljuceviStr)
        where T : IStavkaIzvestaja
    {
        var kolone = izvestaj.KoloneEkrana;
        var istaknutRed = izvestaj.IstaknutRed;

        RedPrikaza Red(T stavka)
        {
            // Извештаји са истицањем редова боје сваку ћелију (и истакнуте и обичне)
            var stil = istaknutRed is null ? null : istaknutRed(stavka) ? StilIstaknutogReda : StilObicnogReda;
            var celije = kolone.Select(k => new CelijaPrikaza(
                k.Vrednost(stavka),
                k.UslovnaKlasa is null ? k.KlasaCelije : $"{k.KlasaCelije} {k.UslovnaKlasa(stavka)}",
                k.RadnoMesto)).ToList();
            return new RedPrikaza(stil, celije);
        }

        // Ставке су већ сортиране по шифри ЗУ - нова група почиње када се шифра промени
        var grupe = new List<GrupaPrikaza>();
        foreach (var stavka in stavke)
        {
            if (grupe.Count == 0 || grupe[^1].SifraZuNormalizovana != stavka.SifraZuNormalizovana)
            {
                grupe.Add(new GrupaPrikaza(stavka.SifraZuNormalizovana, Izvestaj<T>.SifraZaNaslov(stavka), stavka.NazivZu, []));
            }

            grupe[^1].Redovi.Add(Red(stavka));
        }

        return new PrikazIzvestaja
        {
            Kljuc = izvestaj.Kljuc,
            Naslov = izvestaj.Naslov,
            NaslovStranice = izvestaj.NaslovStranice,
            Opis = izvestaj.Opis,
            Napomena = izvestaj.Napomena,
            PorukaBezRezultata = izvestaj.PorukaBezRezultata,
            NazivKartice = izvestaj.NazivKartice,
            IzabraniKljuceviStr = izabraniKljuceviStr,
            Kolone = kolone.Select(k => new KolonaPrikaza(k.Zaglavlje, k.KlasaZaglavlja)).ToList(),
            Grupe = grupe
        };
    }
}

public record KolonaPrikaza(string Zaglavlje, string? Klasa);

public record GrupaPrikaza(string SifraZuNormalizovana, string SifraZaNaslov, string NazivZu, List<RedPrikaza> Redovi);

public record RedPrikaza(string? Stil, List<CelijaPrikaza> Celije);

public record CelijaPrikaza(string Tekst, string? Klasa, bool RadnoMesto);
