using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UporediPlate.Models;

namespace UporediPlate.Controllers
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

        // АЖУРИРАНА АКЦИЈА: Прилагођена за асинхрони Drag & Drop прихват
        [HttpPost]
        public async Task<IActionResult> ObradiUvozRfzo(DateTime period, IFormFile excelFajl)
        {
            // 1. Провера да ли је фајл уопште стигао
            if (excelFajl == null || excelFajl.Length == 0)
            {
                return Json(new { success = false, message = "Грешка: Фајл није примљен или је празан." });
            }

            // 2. Сигурносна провера екстензије
            var ekstenzija = Path.GetExtension(excelFajl.FileName).ToLowerInvariant();
            if (ekstenzija != ".xlsx" && ekstenzija != ".xls")
            {
                return Json(new { success = false, message = "Грешка: Дозвољени су само Excel фајлови (.xlsx, .xls)." });
            }

            try
            {
                // Привремено читање фајла у меморијски стрим (овде ће касније доћи EF Core логика)
                using (var stream = new MemoryStream())
                {
                    await excelFajl.CopyToAsync(stream);
                    
                    // TODO: Овде ћемо имплементирати ExcelDataReader или EPPlus за парсирање
                    // и унос у MS SQL преко вашег DbContext-а.
                }

                // Враћамо успех фронтенду
                return Json(new { 
                    success = true, 
                    message = $"Успех: Фајл '{excelFajl.FileName}' је успешно примљен на серверу!" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Грешка приликом слања РФЗО Excel фајла.");
                return Json(new { success = false, message = $"Системска грешка: {ex.Message}" });
            }
        }

        // Фабричка страница за приказ грешака у систему
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
