namespace UporediPlate.Izvestaji.Okvir;

// Сви извештаји, редоследом којим су регистровани (тај редослед прате и картице у обједињеном приказу)
public class RegistarIzvestaja(IEnumerable<IIzvestaj> izvestaji)
{
    public IReadOnlyList<IIzvestaj> Svi { get; } = izvestaji.ToList();

    public IIzvestaj? Nadji(string kljuc) => Svi.FirstOrDefault(i => i.Kljuc == kljuc);

    // Редни број извештаја (1..n) - користи се за ознаке картица
    public int RedniBroj(IIzvestaj izvestaj) => Svi.ToList().IndexOf(izvestaj) + 1;
}

// Модел за Views/Izvestaj/SviIzvestaji.cshtml
public record ObjedinjeniPrikaz(IReadOnlyList<(int RedniBroj, PrikazIzvestaja Prikaz)> Izvestaji);
