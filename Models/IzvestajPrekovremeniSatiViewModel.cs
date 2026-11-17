using System;

namespace UporediPlate.Models
{
    public class IzvestajPrekovremeniSatiViewModel
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
}
