using System.Collections.Generic;

namespace UporediPlate.Models
{
    public class SviIzvestajiViewModel
    {
        public List<string> IzabraniIzvestaji { get; set; } = new List<string>();
        public string IzabraniKljuceviStr { get; set; } = string.Empty;

        public List<IzvestajNepronadjeniViewModel>? NepronadjeniURfzo { get; set; }
        public List<IzvestajNepronadjeniUZuViewModel>? NepronadjeniUZu { get; set; }
        public List<IzvestajNeslaganjeKoeficijenataViewModel>? NeslaganjeKoeficijenata { get; set; }
        public List<IzvestajPrekovremeniSatiViewModel>? PrekovremeniSati { get; set; }
    }
}
