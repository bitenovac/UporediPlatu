using System.Collections.Generic;

namespace UporediPlate.Models
{
    public class SviIzvestajiViewModel
    {
        public List<string> IzabraniIzvestaji { get; set; } = new List<string>();
        public string IzabraniKljuceviStr { get; set; } = string.Empty;

        // Извештај 1: У ЗУ, а нису у РФЗО
        public List<IzvestajNepronadjeniViewModel>? NepronadjeniURfzo { get; set; }

        // Извештај 2: У РФЗО, а нису у ЗУ
        public List<IzvestajNepronadjeniUZuViewModel>? NepronadjeniUZu { get; set; }
    }
}
