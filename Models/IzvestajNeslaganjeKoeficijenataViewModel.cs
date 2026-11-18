namespace UporediPlate.Models;

public class IzvestajNeslaganjeKoeficijenataViewModel
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
}