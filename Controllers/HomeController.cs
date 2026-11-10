using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UporediPlate.Models;
using UporediPlate; // Усклађено са Вашим AppDbContext простором
using UporediPlate.Services;

namespace UporediPlate.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly UvozRfzoServis _uvozRfzoServis;
        private readonly UvozZuServis _uvozZuServis; // ДОДАТО

        // Конструктор сада убризгава оба сервиса (и за РФЗО и за ЗУ)
        public HomeController(AppDbContext context, ILogger<HomeController> logger, UvozRfzoServis uvozRfzoServis, UvozZuServis uvozZuServis)
        {
            _context = context;
            _logger = logger;
            _uvozRfzoServis = uvozRfzoServis;
            _uvozZuServis = uvozZuServis; // ДОДАТО
        }

        public IActionResult Index() => View();
        public IActionResult Kreirajizvestaj() => View();

        // РФЗО Страница - учитава историју преко свог сервиса
        public IActionResult Rfzo()
        {
            var dtIstorija = _uvozRfzoServis.DobijIstorijuUvoza();
            return View(dtIstorija);
        }

        // Покретање РФЗО увоза
        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UveziRfzoPodatke(DateTime period, IFormFile excelFajl)
        {
            var (uspeh, poruka) = await _uvozRfzoServis.ObradiRfzoExcel(excelFajl, period);

            if (uspeh) TempData["UspehPoruka"] = poruka;
            else TempData["GreskaPoruka"] = poruka;

            return RedirectToAction("Rfzo");
        }

        // Брисање РФЗО увоза преко сервиса
        public async Task<IActionResult> ObrisiUvozRfzo(string vremeKljuc)
        {
            var (uspeh, poruka) = await _uvozRfzoServis.ObrisiUvoz(vremeKljuc);

            if (uspeh) TempData["UspehPoruka"] = poruka;
            else TempData["GreskaPoruka"] = poruka;

            return RedirectToAction("Rfzo");
        }

        // Преглед конкретног РФЗО увоза
        public IActionResult PregledUvozaRfzo(string vremeKljuc)
        {
            try
            {
                if (string.IsNullOrEmpty(vremeKljuc) || vremeKljuc.Length < 14) return RedirectToAction("Rfzo");

                int godina = int.Parse(vremeKljuc.Substring(0, 4));
                int mesec = int.Parse(vremeKljuc.Substring(4, 2));
                int dan = int.Parse(vremeKljuc.Substring(6, 2));
                int sat = int.Parse(vremeKljuc.Substring(8, 2));
                int minut = int.Parse(vremeKljuc.Substring(10, 2));
                int sekund = int.Parse(vremeKljuc.Substring(12, 2));

                DateTime pocetakSekunde = new DateTime(godina, mesec, dan, sat, minut, sekund);
                DateTime krajSekunde = pocetakSekunde.AddSeconds(1);

                var radnici = _context.RfzoUvozi
                    .Where(x => x.DatumUvozaPodataka >= pocetakSekunde && x.DatumUvozaPodataka < krajSekunde)
                    .OrderBy(x => x.Prezime)
                    .ThenBy(x => x.Ime)
                    .ToList();

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


        // ====================================================================
        // ЗУ СЕКЦИЈА (ДОДАТЕ ФУНКЦИОНАЛНОСТИ)
        // ====================================================================

        // ГЕТ: Приказ ЗУ странице са историјом преко ЗУ сервиса
        public IActionResult Zu()
        {
            var dtIstorija = _uvozZuServis.DobijIstorijuUvoza();
            return View(dtIstorija);
        }

        // ПОСТ: Покретање увоза ЗУ Excel табеле преко сервиса
        [HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UveziZuPodatke(DateTime period, IFormFile excelFajl)
        {
            var (uspeh, poruka) = await _uvozZuServis.ObradiZuExcel(excelFajl, period);

            if (uspeh) TempData["UspehPoruka"] = poruka;
            else TempData["GreskaPoruka"] = poruka;

            return RedirectToAction("Zu");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        // ПОСТАВЉЕНО: Брисање конкретног ЗУ увоза преко сервиса
        public async Task<IActionResult> ObrisiUvozZu(string vremeKljuc)
        {
            var (uspeh, poruka) = await _uvozZuServis.ObrisiUvoz(vremeKljuc);

            if (uspeh) TempData["UspehPoruka"] = poruka;
            else TempData["GreskaPoruka"] = poruka;

            return RedirectToAction("Zu");
        }

        // ПОСТАВЉЕНО: Преглед радника учитаних у оквиру селектованог ЗУ фајла
        public IActionResult PregledUvozaZu(string vremeKljuc)
        {
            try
            {
                if (string.IsNullOrEmpty(vremeKljuc) || vremeKljuc.Length < 14) return RedirectToAction("Zu");

                int godina = int.Parse(vremeKljuc.Substring(0, 4));
                int mesec = int.Parse(vremeKljuc.Substring(4, 2));
                int dan = int.Parse(vremeKljuc.Substring(6, 2));
                int sat = int.Parse(vremeKljuc.Substring(8, 2));
                int minut = int.Parse(vremeKljuc.Substring(10, 2));
                int sekund = int.Parse(vremeKljuc.Substring(12, 2));

                DateTime pocetakSekunde = new DateTime(godina, mesec, dan, sat, minut, sekund);
                DateTime krajSekunde = pocetakSekunde.AddSeconds(1);

                var radnici = _context.ZuUvozi
                    .Where(x => x.DatumUvozaPodataka >= pocetakSekunde && x.DatumUvozaPodataka < krajSekunde)
                    .OrderBy(x => x.Prezime)
                    .ThenBy(x => x.Ime)
                    .ToList();

                if (radnici.Any())
                {
                    // Извлачимо првог радника да бисмо узели његову шифру ЗУ и ниво
                    var prvi = radnici.First();

                    // Формирамо исти приказ и за страницу детаљног прегледа радника
                    ViewBag.NazivFajla = $"{prvi.SifraZu}_{prvi.Nivo}.xlsx";
                    ViewBag.Period = prvi.PeriodUvoza.ToString("MM/yyyy");
                    return View(radnici);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка приликом отварања прегледа ЗУ.");
            }
            return RedirectToAction("Zu");
        }

    }
}
