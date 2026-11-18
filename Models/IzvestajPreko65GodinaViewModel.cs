namespace UporediPlate.Models;

public class IzvestajPreko65GodinaViewModel
{
    public string SifraZuOriginal { get; set; } = string.Empty;
    public string SifraZuNormalizovana { get; set; } = string.Empty;
    public string NazivZu { get; set; } = string.Empty;
    public DateTime PeriodUvoza { get; set; }

    public string Jmbg { get; set; } = string.Empty;
    public string Ime { get; set; } = string.Empty;
    public string Prezime { get; set; } = string.Empty;

    public DateTime DatumRodjenja { get; set; }
    public DateTime Datum65Rodjendana { get; set; }

    // True ako radnik tačno u obračunskom mesecu i godini puni 65 godina (crveni font)
    public bool Puni65UTomMesecu { get; set; }

    // SatiUkupno iz ZU tabele pod nazivom "Редован рад"
    public decimal RedovanRad { get; set; }
    public decimal PrekovremeniSati { get; set; }
    public decimal DezurstvoSati { get; set; }
    public decimal PoPozivuSati { get; set; }
    public decimal PripravnostSati { get; set; }
    public decimal UkupanNeto { get; set; }
}