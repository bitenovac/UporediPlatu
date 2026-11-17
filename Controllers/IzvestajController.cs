using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UporediPlate.Models;
using UporediPlate.Services;
using UporediPlate.Services.Izvestaji;
using UporediPlate.Services.Excel;
using UporediPlate.Services.Pdf;

namespace UporediPlate.Controllers
{
    public class IzvestajController : Controller
    {
        private readonly UvozZuServis _uvozZuServis;
        private readonly IzvestajNepronadjeniURfzoServis _nepronadjeniURfzoServis;
        private readonly IzvestajNepronadjeniUZuServis _nepronadjeniUZuServis;
        private readonly IzvestajNeslaganjeKoeficijenataServis _neslaganjeKoefServis;
        private readonly ExcelNepronadjeniURfzoServis _excelNepronadjeniURfzo;
        private readonly ExcelNepronadjeniUZuServis _excelNepronadjeniUZu;
        private readonly ExcelNeslaganjeKoeficijenataServis _excelNeslaganjeKoef;
        private readonly PdfNepronadjeniURfzoServis _pdfNepronadjeniURfzo;
        private readonly PdfNepronadjeniUZuServis _pdfNepronadjeniUZu;
        private readonly PdfNeslaganjeKoeficijenataServis _pdfNeslaganjeKoef;

        public IzvestajController(
            UvozZuServis uvozZuServis,
            IzvestajNepronadjeniURfzoServis nepronadjeniURfzoServis,
            IzvestajNepronadjeniUZuServis nepronadjeniUZuServis,
            IzvestajNeslaganjeKoeficijenataServis neslaganjeKoefServis,
            ExcelNepronadjeniURfzoServis excelNepronadjeniURfzo,
            ExcelNepronadjeniUZuServis excelNepronadjeniUZu,
            ExcelNeslaganjeKoeficijenataServis excelNeslaganjeKoef,
            PdfNepronadjeniURfzoServis pdfNepronadjeniURfzo,
            PdfNepronadjeniUZuServis pdfNepronadjeniUZu,
            PdfNeslaganjeKoeficijenataServis pdfNeslaganjeKoef)
        {
            _uvozZuServis = uvozZuServis;
            _nepronadjeniURfzoServis = nepronadjeniURfzoServis;
            _nepronadjeniUZuServis = nepronadjeniUZuServis;
            _neslaganjeKoefServis = neslaganjeKoefServis;
            _excelNepronadjeniURfzo = excelNepronadjeniURfzo;
            _excelNepronadjeniUZu = excelNepronadjeniUZu;
            _excelNeslaganjeKoef = excelNeslaganjeKoef;
            _pdfNepronadjeniURfzo = pdfNepronadjeniURfzo;
            _pdfNepronadjeniUZu = pdfNepronadjeniUZu;
            _pdfNeslaganjeKoef = pdfNeslaganjeKoef;
        }

        [HttpGet]
        public IActionResult Index()
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

            List<ZuIstorijaViewModel> sortirano = listaFajlova
                .OrderByDescending(x => x.PeriodUvoza)
                .ThenBy(x => x.NazivFajla)
                .ToList();

            return View(sortirano);
        }

        [HttpPost]
        public async Task<IActionResult> GenerisiIzvestaj(List<string> izabraniKljucevi, List<string> izabraniIzvestaji)
        {
            if (izabraniKljucevi == null || izabraniKljucevi.Count == 0 ||
                izabraniIzvestaji == null || izabraniIzvestaji.Count == 0)
            {
                TempData["GreskaPoruka"] = "Молимо изаберите бар један извештај и бар један ЗУ фајл.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.IzabraniKljucevi = string.Join(",", izabraniKljucevi);

            if (izabraniIzvestaji.Count == 1 && izabraniIzvestaji.Contains("NepronadjeniURfzo"))
            {
                var nepronadjeni = await _nepronadjeniURfzoServis.GenerisiAsync(izabraniKljucevi);
                return View("IzvestajNepronadjeni", nepronadjeni);
            }

            if (izabraniIzvestaji.Count == 1 && izabraniIzvestaji.Contains("NepronadjeniUZu"))
            {
                var nepronadjeniUZu = await _nepronadjeniUZuServis.GenerisiAsync(izabraniKljucevi);
                return View("IzvestajNepronadjeniUZu", nepronadjeniUZu);
            }

            if (izabraniIzvestaji.Count == 1 && izabraniIzvestaji.Contains("NeslaganjeKoeficijenata"))
            {
                var neslaganjeKoef = await _neslaganjeKoefServis.GenerisiAsync(izabraniKljucevi);
                return View("IzvestajNeslaganjeKoeficijenata", neslaganjeKoef);
            }

            var objedinjeniModel = new SviIzvestajiViewModel
            {
                IzabraniIzvestaji = izabraniIzvestaji,
                IzabraniKljuceviStr = string.Join(",", izabraniKljucevi)
            };

            if (izabraniIzvestaji.Contains("NepronadjeniURfzo"))
            {
                objedinjeniModel.NepronadjeniURfzo = await _nepronadjeniURfzoServis.GenerisiAsync(izabraniKljucevi);
            }

            if (izabraniIzvestaji.Contains("NepronadjeniUZu"))
            {
                objedinjeniModel.NepronadjeniUZu = await _nepronadjeniUZuServis.GenerisiAsync(izabraniKljucevi);
            }

            if (izabraniIzvestaji.Contains("NeslaganjeKoeficijenata"))
            {
                objedinjeniModel.NeslaganjeKoeficijenata = await _neslaganjeKoefServis.GenerisiAsync(izabraniKljucevi);
            }

            return View("SviIzvestaji", objedinjeniModel);
        }

        // ИЗВОЗ ЗА ИЗВЕШТАЈ 1 (У ЗУ, нису у РФЗО)
        [HttpPost]
        public async Task<IActionResult> IzvozUExcel(string izabraniKljuceviStr)
        {
            if (string.IsNullOrWhiteSpace(izabraniKljuceviStr))
                return RedirectToAction(nameof(Index));

            var kljucevi = izabraniKljuceviStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            var podaci = await _nepronadjeniURfzoServis.GenerisiAsync(kljucevi);

            byte[] content = _excelNepronadjeniURfzo.Generisi(podaci);
            string nazivFajla = $"Izvestaj_Nepronadjeni_U_RFZO_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nazivFajla);
        }

        [HttpPost]
        public async Task<IActionResult> IzvozUPdf(string izabraniKljuceviStr)
        {
            if (string.IsNullOrWhiteSpace(izabraniKljuceviStr))
                return RedirectToAction(nameof(Index));

            var kljucevi = izabraniKljuceviStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            var podaci = await _nepronadjeniURfzoServis.GenerisiAsync(kljucevi);

            if (podaci.Count == 0)
                return RedirectToAction(nameof(Index));

            var (sadrzaj, contentType, nazivFajla) = _pdfNepronadjeniURfzo.Generisi(podaci);
            return File(sadrzaj, contentType, nazivFajla);
        }

        // ИЗВОЗ ЗА ИЗВЕШТАЈ 2 (У РФЗО, нису у ЗУ)
        [HttpPost]
        public async Task<IActionResult> IzvozUExcelNepronadjeniUZu(string izabraniKljuceviStr)
        {
            if (string.IsNullOrWhiteSpace(izabraniKljuceviStr))
                return RedirectToAction(nameof(Index));

            var kljucevi = izabraniKljuceviStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            var podaci = await _nepronadjeniUZuServis.GenerisiAsync(kljucevi);

            byte[] content = _excelNepronadjeniUZu.Generisi(podaci);
            string nazivFajla = $"Izvestaj_Nepronadjeni_U_ZU_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nazivFajla);
        }

        [HttpPost]
        public async Task<IActionResult> IzvozUPdfNepronadjeniUZu(string izabraniKljuceviStr)
        {
            if (string.IsNullOrWhiteSpace(izabraniKljuceviStr))
                return RedirectToAction(nameof(Index));

            var kljucevi = izabraniKljuceviStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            var podaci = await _nepronadjeniUZuServis.GenerisiAsync(kljucevi);

            if (podaci.Count == 0)
                return RedirectToAction(nameof(Index));

            var (sadrzaj, contentType, nazivFajla) = _pdfNepronadjeniUZu.Generisi(podaci);
            return File(sadrzaj, contentType, nazivFajla);
        }

        // ИЗВОЗ ЗА ИЗВЕШТАЈ 3 (Неслагање у коефицијентима)
        [HttpPost]
        public async Task<IActionResult> IzvozUExcelNeslaganjeKoeficijenata(string izabraniKljuceviStr)
        {
            if (string.IsNullOrWhiteSpace(izabraniKljuceviStr))
                return RedirectToAction(nameof(Index));

            var kljucevi = izabraniKljuceviStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            var podaci = await _neslaganjeKoefServis.GenerisiAsync(kljucevi);

            byte[] content = _excelNeslaganjeKoef.Generisi(podaci);
            string nazivFajla = $"Izvestaj_Neslaganje_Koeficijenata_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nazivFajla);
        }

        [HttpPost]
        public async Task<IActionResult> IzvozUPdfNeslaganjeKoeficijenata(string izabraniKljuceviStr)
        {
            if (string.IsNullOrWhiteSpace(izabraniKljuceviStr))
                return RedirectToAction(nameof(Index));

            var kljucevi = izabraniKljuceviStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            var podaci = await _neslaganjeKoefServis.GenerisiAsync(kljucevi);
            if (podaci.Count == 0)
                return RedirectToAction(nameof(Index));
            var (sadrzaj, contentType, nazivFajla) = _pdfNeslaganjeKoef.Generisi(podaci);
            return File(sadrzaj, contentType, nazivFajla);
        }
    }
}