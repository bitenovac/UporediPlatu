using System;

namespace UporediPlate.Models
{
    public class ZuIstorijaViewModel
    {
        public string NazivFajla { get; set; } = string.Empty;
        public DateTime PeriodUvoza { get; set; }
        public DateTime DatumUvozaPodataka { get; set; }
        public int BrojRedova { get; set; }
        public string VremeKljuc { get; set; } = string.Empty;
    }
}
