using System;

namespace UporediPlate.Models
{
    public class IzvestajNepronadjeniUZuViewModel
    {
        public string SifraZuNormalizovana { get; set; } = string.Empty;
        public string NazivZu { get; set; } = string.Empty;
        public string NazivOJ { get; set; } = string.Empty;
        public string Jmbg { get; set; } = string.Empty;
        public string Prezime { get; set; } = string.Empty;
        public string Ime { get; set; } = string.Empty;
        public string NazivZanimanja { get; set; } = string.Empty;
        public string NazivRM { get; set; } = string.Empty;
        public decimal? Koef1 { get; set; }
        public decimal? ProcRVr { get; set; }
        public DateTime? DatumPrestankaRO { get; set; }
        public DateTime PeriodUvoza { get; set; }
        public string NazivFajlaRfzo { get; set; } = string.Empty;
        public string RazlogNeslaganja { get; set; } = string.Empty;
    }
}
