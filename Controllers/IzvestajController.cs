using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UporediPlate.Models;
using UporediPlate.Services;

namespace UporediPlate.Controllers
{
    public class IzvestajController : Controller
    {
        private readonly UvozZuServis _uvozZuServis;
        private readonly IzvestajServis _izvestajServis;
        private readonly ExcelIzvozServis _excelServis;
        private readonly PdfIzvozServis _pdfServis;

        public IzvestajController(
            UvozZuServis uvozZuServis,
            IzvestajServis izvestajServis,
            ExcelIzvozServis excelServis,
            PdfIzvozServis pdfServis)
        {
            _uvozZuServis = uvozZuServis;
            _izvestajServis = izvestajServis;
            _excelServis = excelServis;
            _pdfServis = pdfServis;
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
        public async Task<IActionResult> GenerisiIzvestaj(List<string> izabraniKljucevi)
        {
            if (izabraniKljucevi == null || izabraniKljucevi.Count == 0)
            {
                TempData["GreskaPoruka"] = "Молимо изаберите бар један фајл за креирање извештаја.";
                return RedirectToAction(nameof(Index));
            }

            var nepronadjeni = await _izvestajServis.GenerisiIzvestajNepronadjeniURfzoAsync(izabraniKljucevi);
            ViewBag.IzabraniKljucevi = string.Join(",", izabraniKljucevi);
            return View("IzvestajNepronadjeni", nepronadjeni);
        }

        [HttpPost]
        public async Task<IActionResult> IzvozUExcel(string izabraniKljuceviStr)
        {
            if (string.IsNullOrWhiteSpace(izabraniKljuceviStr))
                return RedirectToAction(nameof(Index));

            var kljucevi = izabraniKljuceviStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            var podaci = await _izvestajServis.GenerisiIzvestajNepronadjeniURfzoAsync(kljucevi);

            byte[] content = _excelServis.GenerisiExcelNepronadjeni(podaci);
            string nazivFajla = $"Izvestaj_Kontrola_Zarada_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nazivFajla);
        }

        [HttpPost]
        public async Task<IActionResult> IzvozUPdf(string izabraniKljuceviStr)
        {
            if (string.IsNullOrWhiteSpace(izabraniKljuceviStr))
                return RedirectToAction(nameof(Index));

            var kljucevi = izabraniKljuceviStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            var podaci = await _izvestajServis.GenerisiIzvestajNepronadjeniURfzoAsync(kljucevi);

            if (podaci.Count == 0)
                return RedirectToAction(nameof(Index));

            var (sadrzaj, contentType, nazivFajla) = _pdfServis.GenerisiPdfIliZipNepronadjeni(podaci);
            return File(sadrzaj, contentType, nazivFajla);
        }
    }
}
