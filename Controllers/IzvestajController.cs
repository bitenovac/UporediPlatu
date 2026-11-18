using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UporediPlate.Services.Izvestaji;

namespace UporediPlate.Controllers
{
    public class IzvestajController : Controller
    {
        private readonly KoordinatorIzvestajaServis _koordinator;

        public IzvestajController(KoordinatorIzvestajaServis koordinator)
        {
            _koordinator = koordinator;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var sortirano = _koordinator.VratiSortiranuIstorijuZuFajlova();
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
            var model = await _koordinator.PripremiObjedinjeneIzvestajeAsync(izabraniKljucevi, izabraniIzvestaji);

            if (izabraniIzvestaji.Count == 1)
            {
                return izabraniIzvestaji[0] switch
                {
                    "NepronadjeniURfzo" => View("IzvestajNepronadjeni", model.NepronadjeniURfzo),
                    "NepronadjeniUZu" => View("IzvestajNepronadjeniUZu", model.NepronadjeniUZu),
                    "NeslaganjeKoeficijenata" => View("IzvestajNeslaganjeKoeficijenata", model.NeslaganjeKoeficijenata),
                    "PrekovremeniSati" => View("IzvestajPrekovremeniSati", model.PrekovremeniSati),
                    "Preko65Godina" => View("IzvestajPreko65Godina", model.Preko65Godina),
                    "NepunoRadnoVreme" => View("IzvestajNepunoRadnoVreme", model.NepunoRadnoVreme),
                    _ => RedirectToAction(nameof(Index))
                };
            }

            return View("SviIzvestaji", model);
        }
    }
}
