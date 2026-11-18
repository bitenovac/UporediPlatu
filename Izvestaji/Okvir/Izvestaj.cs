namespace UporediPlate.Izvestaji.Okvir;

// Ред извештаја - сваки извештај групише своје редове по здравственој установи
public interface IStavkaIzvestaja
{
    // null када извештај нема оригиналну (ненормализовану) шифру ЗУ
    string? SifraZuOriginal { get; }
    string SifraZuNormalizovana { get; }
    string NazivZu { get; }
}

// Оно што контролери виде: извештај без обзира на тип његових редова
public interface IIzvestaj
{
    string Kljuc { get; }
    string NazivKartice { get; }
    Task<IRezultatIzvestaja> GenerisiAsync(IReadOnlyList<string> kljuceviUvoza);
}

public interface IRezultatIzvestaja
{
    int BrojStavki { get; }
    PrikazIzvestaja Prikaz(string izabraniKljuceviStr);
    Datoteka Excel();
    Datoteka Pdf();
}

public record Datoteka(byte[] Sadrzaj, string ContentType, string Naziv);

// Опис једног извештаја: шта се учитава, како се сортира и које колоне има на екрану, у Excel-у и у PDF-у.
// Приказ и извоз су заједнички за све извештаје (PrikazIzvestaja, ExcelIzvoz, PdfIzvoz).
public abstract class Izvestaj<T> : IIzvestaj where T : IStavkaIzvestaja
{
    public abstract string Kljuc { get; }
    public abstract string Naslov { get; }

    // Наслов картице прегледача, ако се разликује од наслова извештаја
    public virtual string NaslovStranice => Naslov;

    public abstract string Opis { get; }
    public abstract string Napomena { get; }
    public abstract string PorukaBezRezultata { get; }
    public abstract string NazivKartice { get; }

    // Префикс назива фајлова за извоз, нпр. "Izvestaj_Neslaganje_Koeficijenata"
    public abstract string PrefiksFajla { get; }

    public virtual bool Polozeno => true;
    public virtual bool CentrirajRedove => true;

    // Цео ред се истиче црвеном бојом (на екрану, у Excel-у и у PDF-у)
    public virtual Func<T, bool>? IstaknutRed => null;

    public abstract IReadOnlyList<KolonaEkrana<T>> KoloneEkrana { get; }
    public abstract IReadOnlyList<KolonaExcela<T>> KoloneExcela { get; }
    public abstract IReadOnlyList<KolonaPdfa<T>> KolonePdfa { get; }

    public abstract IOrderedEnumerable<T> Sortiraj(IEnumerable<T> stavke);

    protected abstract Task<List<T>> UcitajAsync(IReadOnlyList<string> kljuceviUvoza);

    public async Task<IRezultatIzvestaja> GenerisiAsync(IReadOnlyList<string> kljuceviUvoza)
    {
        var stavke = Sortiraj(await UcitajAsync(kljuceviUvoza)).ToList();
        return new Rezultat(this, stavke);
    }

    // Шифра ЗУ за наслове: "оригинална (нормализована)" или само нормализована
    public static string SifraZaNaslov(T stavka) =>
        stavka.SifraZuOriginal is null
            ? stavka.SifraZuNormalizovana
            : $"{stavka.SifraZuOriginal} ({stavka.SifraZuNormalizovana})";

    private sealed class Rezultat(Izvestaj<T> izvestaj, List<T> stavke) : IRezultatIzvestaja
    {
        public int BrojStavki => stavke.Count;
        public PrikazIzvestaja Prikaz(string izabraniKljuceviStr) => PrikazIzvestaja.Napravi(izvestaj, stavke, izabraniKljuceviStr);
        public Datoteka Excel() => ExcelIzvoz.Generisi(izvestaj, stavke);
        public Datoteka Pdf() => PdfIzvoz.Generisi(izvestaj, stavke);
    }
}
