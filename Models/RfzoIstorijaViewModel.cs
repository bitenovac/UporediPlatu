using System;

namespace UporediPlate.Models
{
    public class RfzoIstorijaViewModel
    {
        public string NazivFajla { get; set; } = string.Empty;
        public DateTime PeriodUvoza { get; set; }
        public DateTime DatumUvozaPodataka { get; set; }
        public int BrojRedova { get; set; }
    }
}
