using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using UporediPlate.Models;

namespace UporediPlate.Services.Izvestaji
{
    public class KoordinatorIzvestajaServis
    {
        private readonly UvozZuServis _uvozZuServis;
        private readonly IzvestajNepronadjeniURfzoServis _nepronadjeniURfzoServis;
        private readonly IzvestajNepronadjeniUZuServis _nepronadjeniUZuServis;
        private readonly IzvestajNeslaganjeKoeficijenataServis _neslaganjeKoefServis;
        private readonly IzvestajPrekovremeniSatiServis _prekovremeniSatiServis;

        public KoordinatorIzvestajaServis(
            UvozZuServis uvozZuServis,
            IzvestajNepronadjeniURfzoServis nepronadjeniURfzoServis,
            IzvestajNepronadjeniUZuServis nepronadjeniUZuServis,
            IzvestajNeslaganjeKoeficijenataServis neslaganjeKoefServis,
            IzvestajPrekovremeniSatiServis prekovremeniSatiServis)
        {
            _uvozZuServis = uvozZuServis;
            _nepronadjeniURfzoServis = nepronadjeniURfzoServis;
            _nepronadjeniUZuServis = nepronadjeniUZuServis;
            _neslaganjeKoefServis = neslaganjeKoefServis;
            _prekovremeniSatiServis = prekovremeniSatiServis;
        }

        public List<ZuIstorijaViewModel> VratiSortiranuIstorijuZuFajlova()
        {
            DataTable dtIstorija = _uvozZuServis.DobijIstorijuUvoza();
            var listaFajlova = new List<ZuIstorijaViewModel>();

            if (dtIstorija != null && dtIstorija.Rows.Count > 0)
            {
                foreach (DataRow red in dtIstorija.Rows)
                {
                    DateTime dtUvoza = Convert.ToDateTime(red["DatumUvozaPodataka"]);

                    listaFajlova.Add(new ZuIstorijaViewModel
                    {
                        NazivFajla = red["NazivFajla"]?.ToString() ?? string.Empty,
                        PeriodUvoza = Convert.ToDateTime(red["PeriodUvoza"]),
                        DatumUvozaPodataka = dtUvoza,
                        BrojRedova = Convert.ToInt32(red["BrojRedova"]),
                        VremeKljuc = dtUvoza.ToString("yyyyMMddHHmmss")
                    });
                }
            }

            return listaFajlova
                .OrderByDescending(x => x.PeriodUvoza)
                .ThenBy(x => x.NazivFajla)
                .ToList();
        }

        public async Task<SviIzvestajiViewModel> PripremiObjedinjeneIzvestajeAsync(
            List<string> izabraniKljucevi,
            List<string> izabraniIzvestaji)
        {
            var model = new SviIzvestajiViewModel
            {
                IzabraniIzvestaji = izabraniIzvestaji,
                IzabraniKljuceviStr = string.Join(",", izabraniKljucevi)
            };

            if (izabraniIzvestaji.Contains("NepronadjeniURfzo"))
                model.NepronadjeniURfzo = await _nepronadjeniURfzoServis.GenerisiAsync(izabraniKljucevi);

            if (izabraniIzvestaji.Contains("NepronadjeniUZu"))
                model.NepronadjeniUZu = await _nepronadjeniUZuServis.GenerisiAsync(izabraniKljucevi);

            if (izabraniIzvestaji.Contains("NeslaganjeKoeficijenata"))
                model.NeslaganjeKoeficijenata = await _neslaganjeKoefServis.GenerisiAsync(izabraniKljucevi);

            if (izabraniIzvestaji.Contains("PrekovremeniSati"))
                model.PrekovremeniSati = await _prekovremeniSatiServis.GenerisiAsync(izabraniKljucevi);

            return model;
        }
    }
}
