using System.Data;
using UporediPlate.Models;

namespace UporediPlate.Services.Izvestaji;

public class KoordinatorIzvestajaServis
{
    private readonly IzvestajNepronadjeniURfzoServis _nepronadjeniURfzoServis;
    private readonly IzvestajNepronadjeniUZuServis _nepronadjeniUZuServis;
    private readonly IzvestajNepunoRadnoVremeServis _nepunoRadnoVremeServis;
    private readonly IzvestajNeslaganjeKoeficijenataServis _neslaganjeKoefServis;
    private readonly IzvestajPreko65GodinaServis _preko65GodinaServis;
    private readonly IzvestajPrekovremeniSatiServis _prekovremeniSatiServis;
    private readonly UvozZuServis _uvozZuServis;

    public KoordinatorIzvestajaServis(
        UvozZuServis uvozZuServis,
        IzvestajNepronadjeniURfzoServis nepronadjeniURfzoServis,
        IzvestajNepronadjeniUZuServis nepronadjeniUZuServis,
        IzvestajNeslaganjeKoeficijenataServis neslaganjeKoefServis,
        IzvestajPrekovremeniSatiServis prekovremeniSatiServis,
        IzvestajPreko65GodinaServis preko65GodinaServis,
        IzvestajNepunoRadnoVremeServis nepunoRadnoVremeServis)
    {
        _uvozZuServis = uvozZuServis;
        _nepronadjeniURfzoServis = nepronadjeniURfzoServis;
        _nepronadjeniUZuServis = nepronadjeniUZuServis;
        _neslaganjeKoefServis = neslaganjeKoefServis;
        _prekovremeniSatiServis = prekovremeniSatiServis;
        _preko65GodinaServis = preko65GodinaServis;
        _nepunoRadnoVremeServis = nepunoRadnoVremeServis;
    }

    public List<ZuIstorijaViewModel> VratiSortiranuIstorijuZuFajlova()
    {
        var dtIstorija = _uvozZuServis.DobijIstorijuUvoza();
        var listaFajlova = new List<ZuIstorijaViewModel>();

        if (dtIstorija != null && dtIstorija.Rows.Count > 0)
        {
            foreach (DataRow red in dtIstorija.Rows)
            {
                var dtUvoza = Convert.ToDateTime(red["DatumUvozaPodataka"]);

                listaFajlova.Add(new ZuIstorijaViewModel
                {
                    NazivFajla = red["NazivFajla"]?.ToString() ?? string.Empty,
                    PeriodUvoza = Convert.ToDateTime(red["PeriodUvoza"]),
                    DatumUvozaPodataka = dtUvoza,
                    BrojRedova = Convert.ToInt32(red["BrojRedova"]),
                    VremeKljuc = VremenskiKljuc.Formiraj(dtUvoza)
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
        {
            model.NepronadjeniURfzo = await _nepronadjeniURfzoServis.GenerisiAsync(izabraniKljucevi);
        }

        if (izabraniIzvestaji.Contains("NepronadjeniUZu"))
        {
            model.NepronadjeniUZu = await _nepronadjeniUZuServis.GenerisiAsync(izabraniKljucevi);
        }

        if (izabraniIzvestaji.Contains("NeslaganjeKoeficijenata"))
        {
            model.NeslaganjeKoeficijenata = await _neslaganjeKoefServis.GenerisiAsync(izabraniKljucevi);
        }

        if (izabraniIzvestaji.Contains("PrekovremeniSati"))
        {
            model.PrekovremeniSati = await _prekovremeniSatiServis.GenerisiAsync(izabraniKljucevi);
        }

        if (izabraniIzvestaji.Contains("Preko65Godina"))
        {
            model.Preko65Godina = await _preko65GodinaServis.GenerisiAsync(izabraniKljucevi);
        }

        if (izabraniIzvestaji.Contains("NepunoRadnoVreme"))
        {
            model.NepunoRadnoVreme = await _nepunoRadnoVremeServis.GenerisiAsync(izabraniKljucevi);
        }

        return model;
    }
}