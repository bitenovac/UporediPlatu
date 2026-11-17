using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UporediPlate.Services.Excel;
using UporediPlate.Services.Izvestaji;
using UporediPlate.Services.Pdf;

namespace UporediPlate.Controllers
{
    [Route("Izvestaj/[action]")]
    public class IzvozIzvestajaController : Controller
    {
        private readonly IzvestajNepronadjeniURfzoServis _nepronadjeniURfzoServis;
        private readonly IzvestajNepronadjeniUZuServis _nepronadjeniUZuServis;
        private readonly IzvestajNeslaganjeKoeficijenataServis _neslaganjeKoefServis;
        private readonly IzvestajPrekovremeniSatiServis _prekovremeniSatiServis;
        private readonly ExcelNepronadjeniURfzoServis _excelNepronadjeniURfzo;
        private readonly ExcelNepronadjeniUZuServis _excelNepronadjeniUZu;
        private readonly ExcelNeslaganjeKoeficijenataServis _excelNeslaganjeKoef;
        private readonly ExcelPrekovremeniSatiServis _excelPrekovremeniSati;
        private readonly PdfNepronadjeniURfzoServis _pdfNepronadjeniURfzo;
        private readonly PdfNepronadjeniUZuServis _pdfNepronadjeniUZu;
        private readonly PdfNeslaganjeKoeficijenataServis _pdfNeslaganjeKoef;
        private readonly PdfPrekovremeniSatiServis _pdfPrekovremeniSati;

        public IzvozIzvestajaController(
            IzvestajNepronadjeniURfzoServis nepronadjeniURfzoServis,
            IzvestajNepronadjeniUZuServis nepronadjeniUZuServis,
            IzvestajNeslaganjeKoeficijenataServis neslaganjeKoefServis,
            IzvestajPrekovremeniSatiServis prekovremeniSatiServis,
            ExcelNepronadjeniURfzoServis excelNepronadjeniURfzo,
            ExcelNepronadjeniUZuServis excelNepronadjeniUZu,
            ExcelNeslaganjeKoeficijenataServis excelNeslaganjeKoef,
            ExcelPrekovremeniSatiServis excelPrekovremeniSati,
            PdfNepronadjeniURfzoServis pdfNepronadjeniURfzo,
            PdfNepronadjeniUZuServis pdfNepronadjeniUZu,
            PdfNeslaganjeKoeficijenataServis pdfNeslaganjeKoef,
            PdfPrekovremeniSatiServis pdfPrekovremeniSati)
        {
            _nepronadjeniURfzoServis = nepronadjeniURfzoServis;
            _nepronadjeniUZuServis = nepronadjeniUZuServis;
            _neslaganjeKoefServis = neslaganjeKoefServis;
            _prekovremeniSatiServis = prekovremeniSatiServis;
            _excelNepronadjeniURfzo = excelNepronadjeniURfzo;
            _excelNepronadjeniUZu = excelNepronadjeniUZu;
            _excelNeslaganjeKoef = excelNeslaganjeKoef;
            _excelPrekovremeniSati = excelPrekovremeniSati;
            _pdfNepronadjeniURfzo = pdfNepronadjeniURfzo;
            _pdfNepronadjeniUZu = pdfNepronadjeniUZu;
            _pdfNeslaganjeKoef = pdfNeslaganjeKoef;
            _pdfPrekovremeniSati = pdfPrekovremeniSati;
        }

        // ИЗВОЗ ЗА ИЗВЕШТАЈ 1
        [HttpPost]
        public Task<IActionResult> IzvozUExcel(string izabraniKljuceviStr) =>
            IzveziExcelPomocnik(izabraniKljuceviStr, _nepronadjeniURfzoServis.GenerisiAsync, _excelNepronadjeniURfzo.Generisi, "Izvestaj_Nepronadjeni_U_RFZO");

        [HttpPost]
        public Task<IActionResult> IzvozUPdf(string izabraniKljuceviStr) =>
            IzveziPdfPomocnik(izabraniKljuceviStr, _nepronadjeniURfzoServis.GenerisiAsync, _pdfNepronadjeniURfzo.Generisi);

        // ИЗВОЗ ЗА ИЗВЕШТАЈ 2
        [HttpPost]
        public Task<IActionResult> IzvozUExcelNepronadjeniUZu(string izabraniKljuceviStr) =>
            IzveziExcelPomocnik(izabraniKljuceviStr, _nepronadjeniUZuServis.GenerisiAsync, _excelNepronadjeniUZu.Generisi, "Izvestaj_Nepronadjeni_U_ZU");

        [HttpPost]
        public Task<IActionResult> IzvozUPdfNepronadjeniUZu(string izabraniKljuceviStr) =>
            IzveziPdfPomocnik(izabraniKljuceviStr, _nepronadjeniUZuServis.GenerisiAsync, _pdfNepronadjeniUZu.Generisi);

        // ИЗВОЗ ЗА ИЗВЕШТАЈ 3
        [HttpPost]
        public Task<IActionResult> IzvozUExcelNeslaganjeKoeficijenata(string izabraniKljuceviStr) =>
            IzveziExcelPomocnik(izabraniKljuceviStr, _neslaganjeKoefServis.GenerisiAsync, _excelNeslaganjeKoef.Generisi, "Izvestaj_Neslaganje_Koeficijenata");

        [HttpPost]
        public Task<IActionResult> IzvozUPdfNeslaganjeKoeficijenata(string izabraniKljuceviStr) =>
            IzveziPdfPomocnik(izabraniKljuceviStr, _neslaganjeKoefServis.GenerisiAsync, _pdfNeslaganjeKoef.Generisi);

        // ИЗВОЗ ЗА ИЗВЕШТАЈ 4
        [HttpPost]
        public Task<IActionResult> IzvozUExcelPrekovremeniSati(string izabraniKljuceviStr) =>
            IzveziExcelPomocnik(izabraniKljuceviStr, _prekovremeniSatiServis.GenerisiAsync, _excelPrekovremeniSati.Generisi, "Izvestaj_Prekovremeni_Sati");

        [HttpPost]
        public Task<IActionResult> IzvozUPdfPrekovremeniSati(string izabraniKljuceviStr) =>
            IzveziPdfPomocnik(izabraniKljuceviStr, _prekovremeniSatiServis.GenerisiAsync, _pdfPrekovremeniSati.Generisi);

        // ПОМОЋНЕ ГЕНЕРИЧКЕ МЕТОДЕ
        private async Task<IActionResult> IzveziExcelPomocnik<T>(
            string izabraniKljuceviStr,
            Func<List<string>, Task<List<T>>> dohvatiPodatke,
            Func<List<T>, byte[]> generisiExcel,
            string prefiksFajla)
        {
            if (string.IsNullOrWhiteSpace(izabraniKljuceviStr))
                return RedirectToAction("Index", "Izvestaj");

            var kljucevi = izabraniKljuceviStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            var podaci = await dohvatiPodatke(kljucevi);

            byte[] content = generisiExcel(podaci);
            string nazivFajla = $"{prefiksFajla}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nazivFajla);
        }

        private async Task<IActionResult> IzveziPdfPomocnik<T>(
            string izabraniKljuceviStr,
            Func<List<string>, Task<List<T>>> dohvatiPodatke,
            Func<List<T>, (byte[] Sadrzaj, string ContentType, string NazivFajla)> generisiPdf)
        {
            if (string.IsNullOrWhiteSpace(izabraniKljuceviStr))
                return RedirectToAction("Index", "Izvestaj");

            var kljucevi = izabraniKljuceviStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            var podaci = await dohvatiPodatke(kljucevi);

            if (podaci.Count == 0)
                return RedirectToAction("Index", "Izvestaj");

            var (sadrzaj, contentType, nazivFajla) = generisiPdf(podaci);
            return File(sadrzaj, contentType, nazivFajla);
        }
    }
}
