using System;
using System.Diagnostics;
using System.IO;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ExcelDataReader;
using UporediPlate.Models;

namespace UporediPlate.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(AppDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index() => View();
        public IActionResult Zu() => View();

        public IActionResult Rfzo()
        {
            DataTable dtIstorija = new DataTable();
            dtIstorija.Columns.Add("NazivFajla", typeof(string));
            dtIstorija.Columns.Add("PeriodUvoza", typeof(DateTime));
            dtIstorija.Columns.Add("DatumUvozaPodataka", typeof(DateTime));
            dtIstorija.Columns.Add("BrojRedova", typeof(int));

            try
            {
                var grupe = _context.RfzoUvozi
                    .GroupBy(x => new { x.NazivFajla, x.PeriodUvoza, x.DatumUvozaPodataka })
                    .Select(g => new
                    {
                        NazivFajla = g.Key.NazivFajla ?? "Непознат назив",
                        PeriodUvoza = g.Key.PeriodUvoza,
                        DatumUvozaPodataka = g.Key.DatumUvozaPodataka,
                        BrojRedova = g.Count()
                    })
                    .OrderByDescending(x => x.DatumUvozaPodataka)
                    .ToList();

                foreach (var g in grupe)
                {
                    dtIstorija.Rows.Add(g.NazivFajla, g.PeriodUvoza, g.DatumUvozaPodataka, g.BrojRedova);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка приликом читања историје.");
            }

            return View(dtIstorija);
        }

        public IActionResult KreirajIzvestaj() => View();

        [HttpPost]
        public IActionResult ObradiUvozZu(int ustanovaId, DateTime period, IFormFile excelFajl)
        {
            return RedirectToAction("Zu");
        }

        [HttpPost]
        public async Task<IActionResult> ObradiUvozRfzo(DateTime period, IFormFile excelFajl)
        {
            if (excelFajl == null || excelFajl.Length == 0) return RedirectToAction("Rfzo");

            var ekstenzija = Path.GetExtension(excelFajl.FileName).ToLowerInvariant();
            if (ekstenzija != ".xlsx" && ekstenzija != ".xls") return RedirectToAction("Rfzo");

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            DateTime trenutnoVremeUvoza = DateTime.Now;

            try
            {
                using (var stream = new MemoryStream())
                {
                    await excelFajl.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                        });

                        DataTable tabela = result.Tables[0]; 

                        foreach (DataRow red in tabela.Rows)
                        {
                            string siroviJmbg = red["jmbg"]?.ToString()?.Split('.')?.FirstOrDefault()?.Trim() ?? "";
                            if (siroviJmbg.Length == 12) siroviJmbg = "0" + siroviJmbg;
                            if (string.IsNullOrEmpty(siroviJmbg)) continue;

                            var noviZapis = new RfzoUvoz
                            {
                                Jmbg = siroviJmbg,
                                Lbo = red["lbo"]?.ToString()?.Split('.')?.FirstOrDefault(),
                                Prezime = red["prezime"]?.ToString(),
                                Ime = red["ime"]?.ToString(),
                                NazivSpreme = red["NazivSpreme"]?.ToString(),
                                NazivSpecijalnosti = red["NazivSpecijalnosti"]?.ToString(),
                                NazivSubSpecijalnosti = red["NazivSubSpecijalnosti"]?.ToString(),
                                NazivZvanja = red["NazivZvanja"]?.ToString(),
                                
                                DatumZaposlenja = KonvertujExcelDatum(red["DatumZaposlenja"]),
                                
                                NazivZanimanja = red["NazivZanimanja"]?.ToString(),
                                NazivRV = red["NazivRV"]?.ToString(),
                                NazivRukovodioc = red["NazivRukovodioc"]?.ToString(),
                                NazivZZ = red["NazivZZ"]?.ToString(),
                                NazivRM = red["nazivRM"]?.ToString(),
                                
                                Koef1 = KonvertujUDecimal(red["Koef1"]),
                                DodKzaRuk = KonvertujUDecimal(red["DodKzaRuk"]),
                                DodKzaSiNzv = KonvertujUDecimal(red["DodKzaSiNzv"]),
                                Dodatni036 = KonvertujUDecimal(red["Dodatni036"]),
                                UkrsG = KonvertujUDecimal(red["ukrs_G"]),
                                UkrsM = KonvertujUDecimal(red["ukrs_M"]),
                                
                                NapRadnik = red["NapRadnik"]?.ToString(),
                                NapRz = red["NapRz"]?.ToString(),
                                ProcRVr = KonvertujUDecimal(red["procRVr"]),
                                NazivOJ = red["nazivOJ"]?.ToString(),
                                SifraZu = red["sifrazu"]?.ToString()?.Split('.')?.FirstOrDefault(),
                                NazivZU = red["nazivZU"]?.ToString(),
                                PovecanjeId = Convert.ToInt32(KonvertujUDecimal(red["povecanjeid"]) ?? 0),
                                
                                DatumKapitacija = KonvertujExcelDatum(red["datum_kapitacija"]),
                                DatumPrestankaRO = KonvertujExcelDatum(red["datumPrestankaRO"]),
                                NaSpecijalizaciji = KonvertujUDecimal(red["naSpecijalizaciji"]),
                                
                                PeriodUvoza = period,
                                DatumUvozaPodataka = trenutnoVremeUvoza,
                                NazivFajla = excelFajl.FileName
                            };

                            _context.RfzoUvozi.Add(noviZapis);
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                return RedirectToAction("Rfzo");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка приликом увоза.");
                return RedirectToAction("Rfzo");
            }
        }

        public async Task<IActionResult> ObrisiUvozRfzo(string vremeKljuc)
        {
            try
            {
                var sviZapisi = _context.RfzoUvozi.ToList();
                var zaBrisanje = sviZapisi.Where(x => x.DatumUvozaPodataka.ToString("yyyyMMddHHmmssfff") == vremeKljuc).ToList();

                if (zaBrisanje.Any())
                {
                    _context.RfzoUvozi.RemoveRange(zaBrisanje);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка приликом брисања.");
            }
            return RedirectToAction("Rfzo");
        }

        public IActionResult PregledUvozaRfzo(string vremeKljuc)
        {
            try
            {
                var sviZapisi = _context.RfzoUvozi.ToList();
                var radnici = sviZapisi.Where(x => x.DatumUvozaPodataka.ToString("yyyyMMddHHmmssfff") == vremeKljuc).OrderBy(x => x.Prezime).ThenBy(x => x.Ime).ToList();

                if (radnici.Any())
                {
                    ViewBag.NazivFajla = radnici.First().NazivFajla;
                    ViewBag.Period = radnici.First().PeriodUvoza.ToString("MM/yyyy");
                    return View(radnici);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка приликом отварања прегледа.");
            }
            return RedirectToAction("Rfzo");
        }

        private DateTime? KonvertujExcelDatum(object obj)
        {
            if (obj == null || string.IsNullOrWhiteSpace(obj.ToString())) return null;
            if (double.TryParse(obj.ToString(), out double brojDana)) return DateTime.FromOADate(brojDana);
            if (DateTime.TryParse(obj.ToString(), out DateTime regularanDatum)) return regularanDatum;
            return null;
        }

        private decimal? KonvertujUDecimal(object obj)
        {
            if (obj == null || string.IsNullOrWhiteSpace(obj.ToString())) return null;
            if (decimal.TryParse(obj.ToString(), out decimal rezultat)) return rezultat;
            return null;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
