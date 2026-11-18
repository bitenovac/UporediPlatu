namespace UporediPlate.Uvoz;

// Један учитан фајл (РФЗО или ЗУ) у историји увоза
public record IstorijaUvoza(string NazivFajla, DateTime PeriodUvoza, DateTime DatumUvozaPodataka, int BrojRedova)
{
    public string VremeKljuc => VremenskiKljuc.Formiraj(DatumUvozaPodataka);
}
