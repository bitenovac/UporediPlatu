using System;

namespace UporediPlate.Models
{
    public class IzvestajNepronadjeniViewModel
    {
        public string SifraZuOriginal { get; set; } = string.Empty;
        public string SifraZuNormalizovana { get; set; } = string.Empty;
        public string NazivZu { get; set; } = string.Empty;
        public string Nivo { get; set; } = string.Empty;
        public string Jmbg { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public string Ime { get; set; } = string.Empty;
        public string RadnoMesto { get; set; } = string.Empty;
        public decimal UkupniKoef { get; set; }
        public decimal UkupanNeto { get; set; }
        public decimal Bruto1 { get; set; }
        public DateTime PeriodUvoza { get; set; }
        public string NazivFajla { get; set; } = string.Empty;
        public string RazlogNeslaganja { get; set; } = string.Empty;
    }
}
