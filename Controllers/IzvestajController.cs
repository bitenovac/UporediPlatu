using Microsoft.AspNetCore.Mvc;
using UporediPlate.Izvestaji.Okvir;
using UporediPlate.Uvoz;

namespace UporediPlate.Controllers;

public class IzvestajController : Controller
{
    private readonly RegistarIzvestaja _registar;
    private readonly UvozZuServis _uvozZuServis;

    public IzvestajController(RegistarIzvestaja registar, UvozZuServis uvozZuServis)
    {
        _registar = registar;
        _uvozZuServis = uvozZuServis;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var sortirano = _uvozZuServis.DobijIstorijuUvoza()
            .OrderByDescending(x => x.PeriodUvoza)
            .ThenBy(x => x.NazivFajla)
            .ToList();
        return View(sortirano);
    }

    [HttpPost]
    public async Task<IActionResult> GenerisiIzvestaj(List<string> izabraniKljucevi, List<string> izabraniIzvestaji)
    {
        if (izabraniKljucevi.Count == 0 || izabraniIzvestaji.Count == 0)
        {
            TempData["GreskaPoruka"] = "Молимо изаберите бар један извештај и бар један ЗУ фајл.";
            return RedirectToAction(nameof(Index));
        }

        var izabraniKljuceviStr = string.Join(",", izabraniKljucevi);

        if (izabraniIzvestaji.Count == 1)
        {
            var izvestaj = _registar.Nadji(izabraniIzvestaji[0]);
            if (izvestaj == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var rezultat = await izvestaj.GenerisiAsync(izabraniKljucevi);
            return View("Izvestaj", rezultat.Prikaz(izabraniKljuceviStr));
        }

        var prikazi = new List<(int, PrikazIzvestaja)>();
        foreach (var izvestaj in _registar.Svi.Where(i => izabraniIzvestaji.Contains(i.Kljuc)))
        {
            var rezultat = await izvestaj.GenerisiAsync(izabraniKljucevi);
            prikazi.Add((_registar.RedniBroj(izvestaj), rezultat.Prikaz(izabraniKljuceviStr)));
        }

        return View("SviIzvestaji", new ObjedinjeniPrikaz(prikazi));
    }
}
