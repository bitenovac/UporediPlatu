namespace UporediPlate.Models;

public class IzvestajNepunoRadnoVremeViewModel
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