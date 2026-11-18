using Microsoft.AspNetCore.Mvc;
using UporediPlate.Services.Excel;
using UporediPlate.Services.Izvestaji;
using UporediPlate.Services.Pdf;

namespace UporediPlate.Controllers;

[Route("Izvestaj/[action]")]
public class IzvozIzvestajaController : Controller
{
    private readonly ExcelNepronadjeniURfzoServis _excelNepronadjeniURfzo;
    private readonly ExcelNepronadjeniUZuServis _excelNepronadjeniUZu;
    private readonly ExcelNepunoRadnoVremeServis _excelNepunoRadnoVreme;
    private readonly ExcelNeslaganjeKoeficijenataServis _excelNeslaganjeKoef;
    private readonly ExcelPreko65GodinaServis _excelPreko65Godina;
    private readonly ExcelPrekovremeniSatiServis _excelPrekovremeniSati;
    private readonly IzvestajNepronadjeniURfzoServis _nepronadjeniURfzoServis;
    private readonly IzvestajNepronadjeniUZuServis _nepronadjeniUZuServis;
    private readonly IzvestajNepunoRadnoVremeServis _nepunoRadnoVremeServis;
    private readonly IzvestajNeslaganjeKoeficijenataServis _neslaganjeKoefServis;

    private readonly PdfNepronadjeniURfzoServis _pdfNepronadjeniURfzo;
    private readonly PdfNepronadjeniUZuServis _pdfNepronadjeniUZu;
    private readonly PdfNepunoRadnoVremeServis _pdfNepunoRadnoVreme;
    private readonly PdfNeslaganjeKoeficijenataServis _pdfNeslaganjeKoef;
    private readonly PdfPreko65GodinaServis _pdfPreko65Godina;
    private readonly PdfPrekovremeniSatiServis _pdfPrekovremeniSati;
    private readonly IzvestajPreko65GodinaServis _preko65GodinaServis;
    private readonly IzvestajPrekovremeniSatiServis _prekovremeniSatiServis;

    public IzvozIzvestajaController(
        IzvestajNepronadjeniURfzoServis nepronadjeniURfzoServis,
        IzvestajNepronadjeniUZuServis nepronadjeniUZuServis,
        IzvestajNeslaganjeKoeficijenataServis neslaganjeKoefServis,
        IzvestajPrekovremeniSatiServis prekovremeniSatiServis,
        IzvestajPreko65GodinaServis preko65GodinaServis,
        IzvestajNepunoRadnoVremeServis nepunoRadnoVremeServis,
        ExcelNepronadjeniURfzoServis excelNepronadjeniURfzo,
        ExcelNepronadjeniUZuServis excelNepronadjeniUZu,
        ExcelNeslaganjeKoeficijenataServis excelNeslaganjeKoef,
        ExcelPrekovremeniSatiServis excelPrekovremeniSati,
        ExcelPreko65GodinaServis excelPreko65Godina,
        ExcelNepunoRadnoVremeServis excelNepunoRadnoVreme,
        PdfNepronadjeniURfzoServis pdfNepronadjeniURfzo,
        PdfNepronadjeniUZuServis pdfNepronadjeniUZu,
        PdfNeslaganjeKoeficijenataServis pdfNeslaganjeKoef,
        PdfPrekovremeniSatiServis pdfPrekovremeniSati,
        PdfPreko65GodinaServis pdfPreko65Godina,
        PdfNepunoRadnoVremeServis pdfNepunoRadnoVreme)
    {
        _nepronadjeniURfzoServis = nepronadjeniURfzoServis;
        _nepronadjeniUZuServis = nepronadjeniUZuServis;
        _neslaganjeKoefServis = neslaganjeKoefServis;
        _prekovremeniSatiServis = prekovremeniSatiServis;
        _preko65GodinaServis = preko65GodinaServis;
        _nepunoRadnoVremeServis = nepunoRadnoVremeServis;

        _excelNepronadjeniURfzo = excelNepronadjeniURfzo;
        _excelNepronadjeniUZu = excelNepronadjeniUZu;
        _excelNeslaganjeKoef = excelNeslaganjeKoef;
        _excelPrekovremeniSati = excelPrekovremeniSati;
        _excelPreko65Godina = excelPreko65Godina;
        _excelNepunoRadnoVreme = excelNepunoRadnoVreme;

        _pdfNepronadjeniURfzo = pdfNepronadjeniURfzo;
        _pdfNepronadjeniUZu = pdfNepronadjeniUZu;
        _pdfNeslaganjeKoef = pdfNeslaganjeKoef;
        _pdfPrekovremeniSati = pdfPrekovremeniSati;
        _pdfPreko65Godina = pdfPreko65Godina;
        _pdfNepunoRadnoVreme = pdfNepunoRadnoVreme;
    }

    // ИЗВОЗ ЗА ИЗВЕШТАЈ 1
    [HttpPost]
    public Task<IActionResult> IzvozUExcel(string izabraniKljuceviStr) =>
        IzveziExcelPomocnik(izabraniKljuceviStr, _nepronadjeniURfzoServis.GenerisiAsync,
            _excelNepronadjeniURfzo.Generisi, "Izvestaj_Nepronadjeni_U_RFZO");

    [HttpPost]
    public Task<IActionResult> IzvozUPdf(string izabraniKljuceviStr) =>
        IzveziPdfPomocnik(izabraniKljuceviStr, _nepronadjeniURfzoServis.GenerisiAsync, _pdfNepronadjeniURfzo.Generisi);

    // ИЗВОЗ ЗА ИЗВЕШТАЈ 2
    [HttpPost]
    public Task<IActionResult> IzvozUExcelNepronadjeniUZu(string izabraniKljuceviStr) =>
        IzveziExcelPomocnik(izabraniKljuceviStr, _nepronadjeniUZuServis.GenerisiAsync, _excelNepronadjeniUZu.Generisi,
            "Izvestaj_Nepronadjeni_U_ZU");

    [HttpPost]
    public Task<IActionResult> IzvozUPdfNepronadjeniUZu(string izabraniKljuceviStr) =>
        IzveziPdfPomocnik(izabraniKljuceviStr, _nepronadjeniUZuServis.GenerisiAsync, _pdfNepronadjeniUZu.Generisi);

    // ИЗВОЗ ЗА ИЗВЕШТАЈ 3
    [HttpPost]
    public Task<IActionResult> IzvozUExcelNeslaganjeKoeficijenata(string izabraniKljuceviStr) =>
        IzveziExcelPomocnik(izabraniKljuceviStr, _neslaganjeKoefServis.GenerisiAsync, _excelNeslaganjeKoef.Generisi,
            "Izvestaj_Neslaganje_Koeficijenata");

    [HttpPost]
    public Task<IActionResult> IzvozUPdfNeslaganjeKoeficijenata(string izabraniKljuceviStr) =>
        IzveziPdfPomocnik(izabraniKljuceviStr, _neslaganjeKoefServis.GenerisiAsync, _pdfNeslaganjeKoef.Generisi);

    // ИЗВОЗ ЗА ИЗВЕШТАЈ 4
    [HttpPost]
    public Task<IActionResult> IzvozUExcelPrekovremeniSati(string izabraniKljuceviStr) =>
        IzveziExcelPomocnik(izabraniKljuceviStr, _prekovremeniSatiServis.GenerisiAsync, _excelPrekovremeniSati.Generisi,
            "Izvestaj_Prekovremeni_Sati");

    [HttpPost]
    public Task<IActionResult> IzvozUPdfPrekovremeniSati(string izabraniKljuceviStr) =>
        IzveziPdfPomocnik(izabraniKljuceviStr, _prekovremeniSatiServis.GenerisiAsync, _pdfPrekovremeniSati.Generisi);

    // ИЗВОЗ ЗА ИЗВЕШТАЈ 5
    [HttpPost]
    public Task<IActionResult> IzvozUExcelPreko65Godina(string izabraniKljuceviStr) =>
        IzveziExcelPomocnik(izabraniKljuceviStr, _preko65GodinaServis.GenerisiAsync, _excelPreko65Godina.Generisi,
            "Izvestaj_Preko_65_Godina");

    [HttpPost]
    public Task<IActionResult> IzvozUPdfPreko65Godina(string izabraniKljuceviStr) =>
        IzveziPdfPomocnik(izabraniKljuceviStr, _preko65GodinaServis.GenerisiAsync, _pdfPreko65Godina.Generisi);

    // ИЗВОЗ ЗА ИЗВЕШТАЈ 6
    [HttpPost]
    public Task<IActionResult> IzvozUExcelNepunoRadnoVreme(string izabraniKljuceviStr) =>
        IzveziExcelPomocnik(izabraniKljuceviStr, _nepunoRadnoVremeServis.GenerisiAsync, _excelNepunoRadnoVreme.Generisi,
            "Izvestaj_Nepuno_Radno_Vreme");

    [HttpPost]
    public Task<IActionResult> IzvozUPdfNepunoRadnoVreme(string izabraniKljuceviStr) =>
        IzveziPdfPomocnik(izabraniKljuceviStr, _nepunoRadnoVremeServis.GenerisiAsync, _pdfNepunoRadnoVreme.Generisi);

    // ПОМОЋНЕ ГЕНЕРИЧКЕ МЕТОДЕ
    private async Task<IActionResult> IzveziExcelPomocnik<T>(
        string izabraniKljuceviStr,
        Func<List<string>, Task<List<T>>> dohvatiPodatke,
        Func<List<T>, byte[]> generisiExcel,
        string prefiksFajla)
    {
        if (string.IsNullOrWhiteSpace(izabraniKljuceviStr))
        {
            return RedirectToAction("Index", "Izvestaj");
        }

        var kljucevi = izabraniKljuceviStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        var podaci = await dohvatiPodatke(kljucevi);

        var content = generisiExcel(podaci);
        var nazivFajla = $"{prefiksFajla}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nazivFajla);
    }

    private async Task<IActionResult> IzveziPdfPomocnik<T>(
        string izabraniKljuceviStr,
        Func<List<string>, Task<List<T>>> dohvatiPodatke,
        Func<List<T>, (byte[] Sadrzaj, string ContentType, string NazivFajla)> generisiPdf)
    {
        if (string.IsNullOrWhiteSpace(izabraniKljuceviStr))
        {
            return RedirectToAction("Index", "Izvestaj");
        }

        var kljucevi = izabraniKljuceviStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        var podaci = await dohvatiPodatke(kljucevi);

        if (podaci.Count == 0)
        {
            return RedirectToAction("Index", "Izvestaj");
        }

        var (sadrzaj, contentType, nazivFajla) = generisiPdf(podaci);
        return File(sadrzaj, contentType, nazivFajla);
    }
}