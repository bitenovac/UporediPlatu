using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UporediPlate.Models; // UKLJUČENO ISPRAVNO IME PROJEKTA

namespace UporediPlate.Controllers // UKLJUČENO ISPRAVNO IME PROJEKTA
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // 1. Почетна страница (Index)
        public IActionResult Index()
        {
            return View();
        }

        // 2. Страница за Здравствене установе (ЗУ)
        public IActionResult Zu()
        {
            return View();
        }

        // 3. Страница за РФЗО
        public IActionResult Rfzo()
        {
            return View();
        }

        // 4. Страница за Креирање извештаја
        public IActionResult KreirajIzvestaj()
        {
            return View();
        }

        // Празне метод акције за прихват увоза (биће допуњене логиком касније)
        [HttpPost]
        public IActionResult ObradiUvozZu(int ustanovaId, DateTime period, IFormFile excelFajl)
        {
            return RedirectToAction("Zu");
        }

        [HttpPost]
        public IActionResult ObradiUvozRfzo(DateTime period, IFormFile excelFajl)
        {
            return RedirectToAction("Rfzo");
        }

        // Фабричка страница за приказ грешака у систему
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
